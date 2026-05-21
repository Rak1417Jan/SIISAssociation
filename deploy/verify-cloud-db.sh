#!/usr/bin/env bash
# Verify Cloud SQL AMMS database has schema, seed user, and login stored procedure.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TOOLS_DIR="${SCRIPT_DIR}/tools"
# shellcheck source=/dev/null
source "${SCRIPT_DIR}/.env"

export PATH="${HOME}/.dotnet/tools:${TOOLS_DIR}:/opt/homebrew/opt/mssql-tools18/bin:${PATH}"

: "${GCP_PROJECT_ID:?}"
: "${GCP_REGION:?}"
: "${CLOUDSQL_INSTANCE_NAME:?}"
: "${SQL_USER:?}"
: "${SQL_PASSWORD:?}"
: "${SQL_DATABASE:?}"

PROXY_PORT="${PROXY_PORT:-1433}"
INSTANCE="${GCP_PROJECT_ID}:${GCP_REGION}:${CLOUDSQL_INSTANCE_NAME}"
# Default: Cloud SQL Auth Proxy (works even when your IP changes). Set USE_CLOUD_SQL_PROXY=0 for direct public IP.
USE_CLOUD_SQL_PROXY="${USE_CLOUD_SQL_PROXY:-1}"

PROXY_PID=""

cleanup() {
  if [ -n "${PROXY_PID}" ]; then
    kill "${PROXY_PID}" 2>/dev/null || true
  fi
}
trap cleanup EXIT

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
  echo "Run ./deploy/install-tools.sh first." >&2
  return 1
}

authorize_current_ip() {
  echo "==> Authorizing your current public IP on Cloud SQL..."
  local my_ip
  my_ip="$(curl -fsSL https://api.ipify.org 2>/dev/null || curl -fsSL https://ifconfig.me)"
  echo "    IP: ${my_ip}/32"
  gcloud sql instances patch "${CLOUDSQL_INSTANCE_NAME}" \
    --project="${GCP_PROJECT_ID}" \
    --authorized-networks="${my_ip}/32" \
    --quiet
  local pub_ip
  pub_ip="$(gcloud sql instances describe "${CLOUDSQL_INSTANCE_NAME}" --project="${GCP_PROJECT_ID}" --format='value(ipAddresses[0].ipAddress)')"
  TARGET="${pub_ip},1433"
  echo "    Target: ${TARGET}"
  echo "    Waiting 30s for firewall rule to apply..."
  sleep 30
}

setup_connection() {
  if [ "${USE_CLOUD_SQL_PROXY}" = "1" ]; then
    local proxy_bin
    proxy_bin="$(ensure_cloud_sql_proxy)"
    echo "==> Starting Cloud SQL Auth Proxy on 127.0.0.1:${PROXY_PORT}"
    echo "    (Run: gcloud auth application-default login  if this fails)"
    "${proxy_bin}" "${INSTANCE}" --port "${PROXY_PORT}" &
    PROXY_PID=$!
    sleep 5
    TARGET="127.0.0.1,${PROXY_PORT}"
  else
    authorize_current_ip
  fi
}

echo "=== Cloud SQL verification: ${SQL_DATABASE} ==="

setup_connection
echo "    Connecting via: ${TARGET}"
echo ""

run_query() {
  local title="$1"
  local query="$2"
  echo "--- ${title} ---"
  if ! sqlcmd -S "${TARGET}" -U "${SQL_USER}" -P "${SQL_PASSWORD}" -d "${SQL_DATABASE}" -C -I -Q "${query}" -W -s "|"; then
    echo ""
    echo "Connection failed."
    if [ "${USE_CLOUD_SQL_PROXY}" = "1" ]; then
      echo "  gcloud auth application-default login"
      echo "  gcloud auth application-default set-quota-project ${GCP_PROJECT_ID}"
    else
      echo "  Try proxy mode: ./deploy/verify-cloud-db.sh"
      echo "  (or your IP changed — re-run to re-authorize)"
    fi
    exit 1
  fi
  echo ""
}

run_query "Clients" "SELECT CLIENT_ID, CLIENT_CODE, CLIENT_NAME FROM dbo.CLIENTS;"
run_query "Users" "SELECT USER_ID, CLIENT_ID, USERNAME, LEN(PASSWORD_SALT) AS SaltLen FROM dbo.USERS;"
run_query "Stored procedure" "SELECT name FROM sys.procedures WHERE name = 'sp_GetUserByUsername';"
run_query "Company types (for seed)" "SELECT COUNT(*) AS CompanyTypeCount FROM dbo.COMPANY_TYPE;"

echo "--- Test sp_GetUserByUsername (clientId=1, seed.admin) ---"
sqlcmd -S "${TARGET}" -U "${SQL_USER}" -P "${SQL_PASSWORD}" -d "${SQL_DATABASE}" -C -I -Q \
  "EXEC dbo.sp_GetUserByUsername @ClientId=1, @Username=N'seed.admin';" -W || exit 1

echo ""
echo "Verification complete."
echo "If Clients/Users are empty or SP is missing, run:"
echo "  FRESH_DB=1 ./deploy/publish-database.sh"
echo "(publish uses proxy by default; omit USE_CLOUD_SQL_PROXY=0 unless you need direct IP)"
