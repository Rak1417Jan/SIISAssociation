#!/usr/bin/env bash
# Install tools needed for publish-database.sh on macOS/Linux.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TOOLS_DIR="${SCRIPT_DIR}/tools"
mkdir -p "${TOOLS_DIR}"

export PATH="${HOME}/.dotnet/tools:${TOOLS_DIR}:${PATH}"

echo "==> SqlPackage"
if command -v sqlpackage &>/dev/null || [ -x "${TOOLS_DIR}/sqlpackage/sqlpackage" ]; then
  echo "    Already installed."
elif command -v dotnet &>/dev/null; then
  dotnet tool install -g microsoft.sqlpackage 2>/dev/null || dotnet tool update -g microsoft.sqlpackage
  echo "    Installed via dotnet tool."
else
  # Official macOS download (aka.ms/sqlpackage-osx-arm64.zip is a broken short link)
  SQLPACKAGE_URL="https://aka.ms/sqlpackage-macos"
  ZIP="${TOOLS_DIR}/sqlpackage-download.zip"
  echo "    Downloading SqlPackage for macOS (no dotnet required)..."
  curl -fsSL -o "${ZIP}" "${SQLPACKAGE_URL}"
  if ! file "${ZIP}" | grep -qiE 'zip|archive'; then
    echo "    Download failed (not a zip). Try: brew install --cask dotnet-sdk && dotnet tool install -g microsoft.sqlpackage"
    head -3 "${ZIP}" 2>/dev/null || true
    exit 1
  fi
  mkdir -p "${TOOLS_DIR}/sqlpackage"
  unzip -qo "${ZIP}" -d "${TOOLS_DIR}/sqlpackage"
  rm -f "${ZIP}"
  chmod +x "${TOOLS_DIR}/sqlpackage/sqlpackage" 2>/dev/null || true
fi

echo "==> Checking sqlcmd"
if ! command -v sqlcmd &>/dev/null; then
  if command -v brew &>/dev/null; then
    echo "    Installing mssql-tools18 via Homebrew..."
    brew tap microsoft/mssql-release https://github.com/Microsoft/homebrew-mssql-release 2>/dev/null || true
    HOMEBREW_ACCEPT_EULA=Y brew install mssql-tools18 2>/dev/null || {
      echo "    sqlcmd install failed. Script publish will try sqlpackage-only or Docker sqlcmd."
    }
  fi
fi

echo "==> Checking Cloud SQL Auth Proxy"
PROXY_BIN="${TOOLS_DIR}/cloud-sql-proxy"
if ! command -v cloud-sql-proxy &>/dev/null && [ ! -x "${PROXY_BIN}" ]; then
  ARCH="$(uname -m)"
  case "${ARCH}" in
    arm64) PROXY_ARCH="darwin.arm64" ;;
    x86_64) PROXY_ARCH="darwin.amd64" ;;
    *) echo "Unsupported arch: ${ARCH}"; exit 1 ;;
  esac
  PROXY_VERSION="v2.14.3"
  URL="https://storage.googleapis.com/cloud-sql-connectors/cloud-sql-proxy/${PROXY_VERSION}/cloud-sql-proxy.${PROXY_ARCH}"
  echo "    Downloading cloud-sql-proxy from ${URL}"
  curl -fsSL -o "${PROXY_BIN}" "${URL}"
  chmod +x "${PROXY_BIN}"
fi

echo ""
echo "Tools ready. Ensure your shell has:"
echo '  export PATH="$HOME/.dotnet/tools:'"${TOOLS_DIR}"':$PATH"'
echo ""
echo "Then run: ./deploy/publish-database.sh"
