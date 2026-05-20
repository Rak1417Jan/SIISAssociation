#!/usr/bin/env bash
# Publish AMMS schema to Cloud SQL for SQL Server.
# Works on macOS without Visual Studio (SQL scripts fallback).
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
AMMS_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
TOOLS_DIR="${SCRIPT_DIR}/tools"
DB_ROOT="${AMMS_ROOT}/AMMS.Database"

# shellcheck source=/dev/null
source "${SCRIPT_DIR}/.env"

: "${GCP_PROJECT_ID:?}"
: "${GCP_REGION:?}"
: "${CLOUDSQL_INSTANCE_NAME:?}"
: "${SQL_USER:?}"
: "${SQL_PASSWORD:?}"
: "${SQL_DATABASE:?}"

export PATH="${HOME}/.dotnet/tools:${TOOLS_DIR}:/opt/homebrew/bin:/usr/local/bin:${PATH}"

DACPAC="${AMMS_ROOT}/AMMS.Database/bin/Release/AMMS.Database.dacpac"
SQLPROJ="${AMMS_ROOT}/AMMS.Database/AMMS.Database.sqlproj"
PROXY_PORT="${PROXY_PORT:-1433}"
INSTANCE="${GCP_PROJECT_ID}:${GCP_REGION}:${CLOUDSQL_INSTANCE_NAME}"
# auto | dacpac | sqlscripts
PUBLISH_METHOD="${PUBLISH_METHOD:-auto}"
SKIP_SEED="${SKIP_SEED:-0}"
# Set FRESH_DB=1 to drop and recreate AMMS (use after a failed partial publish)
FRESH_DB="${FRESH_DB:-0}"
TABLE_ORDER_FILE="${SCRIPT_DIR}/table-order.txt"

download_sqlpackage_binary() {
  local zip url extract_dir
  extract_dir="${TOOLS_DIR}/sqlpackage"
  zip="${TOOLS_DIR}/sqlpackage-download.zip"
  url="https://aka.ms/sqlpackage-macos"
  mkdir -p "${extract_dir}"
  echo "==> Downloading SqlPackage for macOS..."
  curl -fsSL -o "${zip}" "${url}"
  if ! file "${zip}" | grep -qiE 'zip|archive'; then
    echo "Download failed (not a zip). Run: ./deploy/install-tools.sh"
    return 1
  fi
  unzip -qo "${zip}" -d "${extract_dir}"
  rm -f "${zip}"
  chmod +x "${extract_dir}/sqlpackage" 2>/dev/null || true
}

ensure_sqlpackage() {
  if command -v sqlpackage &>/dev/null; then
    return 0
  fi
  if [ -x "${HOME}/.dotnet/tools/sqlpackage" ]; then
    return 0
  fi
  if [ -x "${TOOLS_DIR}/sqlpackage/sqlpackage" ]; then
    return 0
  fi
  if command -v dotnet &>/dev/null; then
    echo "==> Installing SqlPackage via dotnet tool..."
    dotnet tool install -g microsoft.sqlpackage 2>/dev/null || dotnet tool update -g microsoft.sqlpackage
    export PATH="${HOME}/.dotnet/tools:${PATH}"
  fi
  if command -v sqlpackage &>/dev/null || [ -x "${HOME}/.dotnet/tools/sqlpackage" ]; then
    return 0
  fi
  download_sqlpackage_binary || return 1
  [ -x "${TOOLS_DIR}/sqlpackage/sqlpackage" ]
}

sqlpackage_cmd() {
  if command -v sqlpackage &>/dev/null; then
    command sqlpackage "$@"
  elif [ -x "${TOOLS_DIR}/sqlpackage/sqlpackage" ]; then
    "${TOOLS_DIR}/sqlpackage/sqlpackage" "$@"
  else
    "${HOME}/.dotnet/tools/sqlpackage" "$@"
  fi
}

ensure_sqlcmd() {
  if command -v sqlcmd &>/dev/null; then
    return 0
  fi
  # Homebrew mssql-tools18
  for p in /opt/homebrew/bin/sqlcmd /usr/local/bin/sqlcmd; do
    if [ -x "${p}" ]; then
      export PATH="$(dirname "${p}"):${PATH}"
      return 0
    fi
  done
  return 1
}

sqlcmd_cmd() {
  local file="$1"
  # -I = QUOTED_IDENTIFIER ON (required for filtered indexes in COMPANY_MASTER etc.)
  sqlcmd -S "${TARGET_SERVER}" -U "${SQL_USER}" -P "${SQL_PASSWORD}" -d "${SQL_DATABASE}" \
    -i "${file}" -b -C -I
}

ensure_cloud_sql_proxy() {
  if command -v cloud-sql-proxy &>/dev/null; then
    echo "cloud-sql-proxy"
    return 0
  fi
  local proxy="${TOOLS_DIR}/cloud-sql-proxy"
  if [ -x "${proxy}" ]; then
    echo "${proxy}"
    return 0
  fi
  local arch pa url
  arch="$(uname -m)"
  case "${arch}" in
    arm64) pa="darwin.arm64" ;;
    x86_64) pa="darwin.amd64" ;;
    aarch64) pa="linux.arm64" ;;
    *) echo "Unsupported architecture: ${arch}" >&2; return 1 ;;
  esac
  mkdir -p "${TOOLS_DIR}"
  url="https://storage.googleapis.com/cloud-sql-connectors/cloud-sql-proxy/v2.14.3/cloud-sql-proxy.${pa}"
  echo "==> Downloading Cloud SQL Auth Proxy..."
  curl -fsSL -o "${proxy}" "${url}"
  chmod +x "${proxy}"
  echo "${proxy}"
}

start_proxy() {
  if [ "${USE_CLOUD_SQL_PROXY:-1}" != "1" ]; then
    : "${TARGET_SERVER:?Set TARGET_SERVER when USE_CLOUD_SQL_PROXY=0}"
    return 0
  fi
  local proxy_bin
  proxy_bin="$(ensure_cloud_sql_proxy)"
  echo "==> Starting Cloud SQL proxy on 127.0.0.1:${PROXY_PORT} (${INSTANCE})"
  echo "    (Requires: gcloud auth application-default login)"
  "${proxy_bin}" "${INSTANCE}" --port "${PROXY_PORT}" 2>&1 &
  PROXY_PID=$!
  trap 'kill ${PROXY_PID} 2>/dev/null || true' EXIT
  sleep 5
  TARGET_SERVER="127.0.0.1,${PROXY_PORT}"
}

authorize_current_ip() {
  if [ "${USE_CLOUD_SQL_PROXY:-1}" = "1" ]; then
    return 0
  fi
  echo "==> Authorizing your public IP on Cloud SQL (for direct connection)"
  local my_ip
  my_ip="$(curl -fsSL https://api.ipify.org 2>/dev/null || curl -fsSL https://ifconfig.me)"
  gcloud sql instances patch "${CLOUDSQL_INSTANCE_NAME}" \
    --authorized-networks="${my_ip}/32" \
    --quiet
  local pub_ip
  pub_ip="$(gcloud sql instances describe "${CLOUDSQL_INSTANCE_NAME}" --format='value(ipAddresses[0].ipAddress)')"
  TARGET_SERVER="${pub_ip},1433"
  echo "    Target: ${TARGET_SERVER}"
}

build_dacpac() {
  if [ -f "${DACPAC}" ]; then
    return 0
  fi
  DACPAC="$(find "${AMMS_ROOT}/AMMS.Database/bin" -name 'AMMS.Database.dacpac' 2>/dev/null | head -1 || true)"
  if [ -n "${DACPAC}" ] && [ -f "${DACPAC}" ]; then
    return 0
  fi
  echo "==> Building database project (requires MSBuild/SSDT)..."
  if command -v msbuild &>/dev/null; then
    msbuild "${SQLPROJ}" /p:Configuration=Release /p:Platform="Any CPU"
  elif command -v dotnet &>/dev/null; then
    dotnet build "${SQLPROJ}" -c Release 2>/dev/null || return 1
  else
    return 1
  fi
  DACPAC="$(find "${AMMS_ROOT}/AMMS.Database/bin" -name 'AMMS.Database.dacpac' 2>/dev/null | head -1 || true)"
  [ -n "${DACPAC}" ] && [ -f "${DACPAC}" ]
}

publish_dacpac() {
  echo "==> Publishing dacpac to ${SQL_DATABASE} on ${TARGET_SERVER}"
  sqlpackage_cmd /Action:Publish \
    /SourceFile:"${DACPAC}" \
    /TargetServerName:"${TARGET_SERVER}" \
    /TargetDatabaseName:"${SQL_DATABASE}" \
    /TargetUser:"${SQL_USER}" \
    /TargetPassword:"${SQL_PASSWORD}" \
    /TargetEncryptConnection:True \
    /TargetTrustServerCertificate:True
}

recreate_database() {
  echo "==> Dropping and recreating database ${SQL_DATABASE} (FRESH_DB=1)"
  sqlcmd -S "${TARGET_SERVER}" -U "${SQL_USER}" -P "${SQL_PASSWORD}" -d master -b -C -I -Q "
    IF EXISTS (SELECT 1 FROM sys.databases WHERE name = N'${SQL_DATABASE}')
    BEGIN
      ALTER DATABASE [${SQL_DATABASE}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
      DROP DATABASE [${SQL_DATABASE}];
    END
    CREATE DATABASE [${SQL_DATABASE}];
  "
}

ordered_table_files() {
  local name f
  while IFS= read -r name || [ -n "${name}" ]; do
    [[ "${name}" =~ ^#.*$ ]] && continue
    [[ -z "${name// }" ]] && continue
    f="${DB_ROOT}/dbo/Tables/${name}.sql"
    if [ -f "${f}" ]; then
      echo "${f}"
    else
      echo "WARNING: missing table script: ${f}" >&2
    fi
  done < "${TABLE_ORDER_FILE}"
}

publish_sqlscripts() {
  if ! ensure_sqlcmd; then
    echo "sqlcmd not found. Run: ./deploy/install-tools.sh"
    exit 1
  fi

  echo "==> Publishing schema via SQL scripts to ${SQL_DATABASE} on ${TARGET_SERVER}"

  if [ "${FRESH_DB}" = "1" ]; then
    recreate_database
  else
    sqlcmd -S "${TARGET_SERVER}" -U "${SQL_USER}" -P "${SQL_PASSWORD}" -d master -b -C -I \
      -Q "IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'${SQL_DATABASE}') CREATE DATABASE [${SQL_DATABASE}];" \
      2>/dev/null || true
  fi

  local procs
  procs="$(find "${DB_ROOT}/dbo/StoredProcedures" -name '*.sql' | sort)"

  while IFS= read -r f; do
    [ -z "${f}" ] && continue
    echo "    TABLE $(basename "${f}")"
    if ! sqlcmd_cmd "${f}"; then
      echo ""
      echo "SQL script failed: ${f}"
      echo "If a previous run failed partway, retry with: FRESH_DB=1 USE_CLOUD_SQL_PROXY=0 ./deploy/publish-database.sh"
      exit 1
    fi
  done < <(ordered_table_files)

  for f in ${procs}; do
    echo "    PROC $(basename "${f}")"
    if ! sqlcmd_cmd "${f}"; then
      echo ""
      echo "SQL script failed: ${f}"
      echo "Retry with: FRESH_DB=1 USE_CLOUD_SQL_PROXY=0 ./deploy/publish-database.sh"
      exit 1
    fi
  done

  if [ "${SKIP_SEED}" != "1" ]; then
    for f in "${DB_ROOT}/Scripts/Seed_Permissions.sql" \
             "${DB_ROOT}/Scripts/Seed_CompanyTypes.sql" \
             "${DB_ROOT}/Scripts/Seed_TestData.sql"; do
      if [ -f "${f}" ]; then
        echo "    SEED $(basename "${f}")"
        if ! sqlcmd_cmd "${f}"; then
          echo "SQL seed failed: ${f}"
          exit 1
        fi
      fi
    done
  else
    echo "    (skipped seed data — SKIP_SEED=1)"
  fi
}

# --- main ---
echo "==> AMMS database publish (method: ${PUBLISH_METHOD})"

authorize_current_ip
start_proxy

case "${PUBLISH_METHOD}" in
  dacpac)
    ensure_sqlpackage || { echo "Run: ./deploy/install-tools.sh"; exit 1; }
    build_dacpac || { echo "dacpac build failed"; exit 1; }
    publish_dacpac
    ;;
  sqlscripts)
    publish_sqlscripts
    ;;
  auto)
    if build_dacpac 2>/dev/null && ensure_sqlpackage 2>/dev/null; then
      publish_dacpac
    else
      echo "==> dacpac not available on this machine; using SQL scripts (normal on macOS)"
      publish_sqlscripts
    fi
    ;;
  *)
    echo "Unknown PUBLISH_METHOD=${PUBLISH_METHOD}"
    exit 1
    ;;
esac

echo "Database publish complete."
