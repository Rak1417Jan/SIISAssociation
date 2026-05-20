#!/usr/bin/env bash
# Quick check: what is deployed on GCP for AMMS?
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=/dev/null
source "${SCRIPT_DIR}/.env"

: "${GCP_PROJECT_ID:?}"
: "${GCP_REGION:?}"
: "${CLOUDSQL_INSTANCE_NAME:?}"
: "${API_SERVICE_NAME:?}"
: "${WEB_SERVICE_NAME:?}"

echo "=== AMMS deployment status ==="
echo "Project: ${GCP_PROJECT_ID}"
echo ""

echo "--- Cloud SQL ---"
if gcloud sql instances describe "${CLOUDSQL_INSTANCE_NAME}" --project="${GCP_PROJECT_ID}" --format="table(name,state,databaseVersion,ipAddresses[0].ipAddress)" 2>/dev/null; then
  echo "Database '${SQL_DATABASE}':"
  gcloud sql databases list --instance="${CLOUDSQL_INSTANCE_NAME}" --project="${GCP_PROJECT_ID}" \
    --filter="name=${SQL_DATABASE}" --format="table(name,charset,collation)" 2>/dev/null || true
else
  echo "  Instance ${CLOUDSQL_INSTANCE_NAME} not found or no access."
fi

echo ""
echo "--- Cloud Run ---"
API_URL="$(gcloud run services describe "${API_SERVICE_NAME}" --region="${GCP_REGION}" --project="${GCP_PROJECT_ID}" --format='value(status.url)' 2>/dev/null || echo '')"
WEB_URL="$(gcloud run services describe "${WEB_SERVICE_NAME}" --region="${GCP_REGION}" --project="${GCP_PROJECT_ID}" --format='value(status.url)' 2>/dev/null || echo '')"

if [ -n "${API_URL}" ]; then
  echo "  ${API_SERVICE_NAME}: DEPLOYED  ${API_URL}"
else
  echo "  ${API_SERVICE_NAME}: NOT DEPLOYED"
fi

if [ -n "${WEB_URL}" ]; then
  echo "  ${WEB_SERVICE_NAME}: DEPLOYED  ${WEB_URL}"
else
  echo "  ${WEB_SERVICE_NAME}: NOT DEPLOYED"
fi

echo ""
echo "--- Artifact Registry (latest images) ---"
gcloud artifacts docker images list "${GCP_REGION}-docker.pkg.dev/${GCP_PROJECT_ID}/${AR_REPO}" \
  --limit=5 --format="table(package,version,createTime)" 2>/dev/null || echo "  (no images or repo empty)"

echo ""
if [ -z "${API_URL}" ] || [ -z "${WEB_URL}" ]; then
  echo "Next: finish DB publish, then deploy:"
  echo "  FRESH_DB=1 USE_CLOUD_SQL_PROXY=0 ./deploy/publish-database.sh"
  echo "  ./deploy/deploy-all.sh"
  echo "  OR push to Git with Cloud Build trigger (see DEPLOY-GCP.md)"
fi
