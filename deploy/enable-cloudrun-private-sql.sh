#!/usr/bin/env bash
# Cloud Run + .NET SqlClient cannot use the built-in Cloud SQL sidecar on 127.0.0.1:1433 for
# SQL Server (connection refused). Use Cloud SQL private IP + Serverless VPC Access instead.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=/dev/null
source "${SCRIPT_DIR}/.env"

: "${GCP_PROJECT_ID:?}"
: "${GCP_REGION:?}"
: "${CLOUDSQL_INSTANCE_NAME:?}"
: "${SQL_USER:?}"
: "${SQL_PASSWORD:?}"
: "${SQL_DATABASE:?}"
: "${API_SERVICE_NAME:?}"
: "${RUN_SERVICE_ACCOUNT:?}"
: "${SECRET_DB_CONNECTION:?}"
: "${SECRET_JWT_KEY:?}"
: "${JWT_ISSUER:?}"
: "${JWT_AUDIENCE:?}"

VPC_CONNECTOR_NAME="${VPC_CONNECTOR:-amms-connector}"
VPC_PEERING_RANGE_NAME="${VPC_PEERING_RANGE_NAME:-google-managed-services-default}"
VPC_CONNECTOR_RANGE="${VPC_CONNECTOR_RANGE:-10.8.0.0/28}"
NETWORK="${VPC_NETWORK:-default}"

SA_EMAIL="${RUN_SERVICE_ACCOUNT}@${GCP_PROJECT_ID}.iam.gserviceaccount.com"
CONNECTOR_RESOURCE="projects/${GCP_PROJECT_ID}/locations/${GCP_REGION}/connectors/${VPC_CONNECTOR_NAME}"

gcloud config set project "${GCP_PROJECT_ID}" --quiet

echo "==> Enabling APIs (if needed)"
gcloud services enable servicenetworking.googleapis.com vpcaccess.googleapis.com sqladmin.googleapis.com run.googleapis.com --quiet

echo "==> Private service connection for Cloud SQL (VPC: ${NETWORK})"
if ! gcloud compute addresses describe "${VPC_PEERING_RANGE_NAME}" --global --project="${GCP_PROJECT_ID}" &>/dev/null; then
  gcloud compute addresses create "${VPC_PEERING_RANGE_NAME}" \
    --global \
    --purpose=VPC_PEERING \
    --prefix-length=16 \
    --network="${NETWORK}" \
    --project="${GCP_PROJECT_ID}"
fi

gcloud services vpc-peerings connect \
  --service=servicenetworking.googleapis.com \
  --ranges="${VPC_PEERING_RANGE_NAME}" \
  --network="${NETWORK}" \
  --project="${GCP_PROJECT_ID}" \
  --quiet 2>/dev/null || true

echo "==> Assigning private IP on Cloud SQL instance: ${CLOUDSQL_INSTANCE_NAME}"
gcloud sql instances patch "${CLOUDSQL_INSTANCE_NAME}" \
  --project="${GCP_PROJECT_ID}" \
  --network="projects/${GCP_PROJECT_ID}/global/networks/${NETWORK}" \
  --assign-ip \
  --quiet

echo "    Waiting for instance to be RUNNABLE..."
for _ in $(seq 1 60); do
  STATE="$(gcloud sql instances describe "${CLOUDSQL_INSTANCE_NAME}" --format='value(state)' 2>/dev/null || echo "")"
  if [ "${STATE}" = "RUNNABLE" ]; then
    break
  fi
  sleep 10
done

PRIVATE_IP="$(gcloud sql instances describe "${CLOUDSQL_INSTANCE_NAME}" \
  --format="value(ipAddresses.filter(type:PRIVATE).extract(ipAddress).flatten())")"
if [ -z "${PRIVATE_IP}" ]; then
  echo "ERROR: No private IP on ${CLOUDSQL_INSTANCE_NAME}. Check Cloud SQL networking in console." >&2
  exit 1
fi
echo "    Private IP: ${PRIVATE_IP}"

echo "==> Serverless VPC Access connector: ${VPC_CONNECTOR_NAME}"
if ! gcloud compute networks vpc-access connectors describe "${VPC_CONNECTOR_NAME}" \
  --region="${GCP_REGION}" --project="${GCP_PROJECT_ID}" &>/dev/null; then
  gcloud compute networks vpc-access connectors create "${VPC_CONNECTOR_NAME}" \
    --region="${GCP_REGION}" \
    --network="${NETWORK}" \
    --range="${VPC_CONNECTOR_RANGE}" \
    --min-instances=2 \
    --max-instances=3 \
    --project="${GCP_PROJECT_ID}" \
    --quiet
  echo "    Waiting for connector to be READY..."
  for _ in $(seq 1 30); do
    READY="$(gcloud compute networks vpc-access connectors describe "${VPC_CONNECTOR_NAME}" \
      --region="${GCP_REGION}" --format='value(state)' 2>/dev/null || echo "")"
    if [ "${READY}" = "READY" ]; then
      break
    fi
    sleep 10
  done
fi

DB_CONN="Server=${PRIVATE_IP},1433;Database=${SQL_DATABASE};User Id=${SQL_USER};Password=${SQL_PASSWORD};Encrypt=True;TrustServerCertificate=True;"

echo "==> Updating Secret Manager: ${SECRET_DB_CONNECTION}"
echo -n "${DB_CONN}" | gcloud secrets versions add "${SECRET_DB_CONNECTION}" \
  --project="${GCP_PROJECT_ID}" \
  --data-file=-

echo "==> Updating Cloud Run: ${API_SERVICE_NAME}"
gcloud run services update "${API_SERVICE_NAME}" \
  --project="${GCP_PROJECT_ID}" \
  --region="${GCP_REGION}" \
  --vpc-connector="${CONNECTOR_RESOURCE}" \
  --vpc-egress=private-ranges-only \
  --clear-cloudsql-instances \
  --service-account="${SA_EMAIL}" \
  --set-secrets="ConnectionStrings__DefaultConnection=${SECRET_DB_CONNECTION}:latest,Jwt__Key=${SECRET_JWT_KEY}:latest" \
  --set-env-vars="Jwt__Issuer=${JWT_ISSUER},Jwt__Audience=${JWT_AUDIENCE},ASPNETCORE_ENVIRONMENT=Production" \
  --quiet

API_URL="$(gcloud run services describe "${API_SERVICE_NAME}" --region="${GCP_REGION}" --format='value(status.url)')"
echo ""
echo "Done. Private SQL via ${PRIVATE_IP}:1433 through ${VPC_CONNECTOR_NAME}."
echo "Set VPC_CONNECTOR=${VPC_CONNECTOR_NAME} in deploy/.env and Cloud Build substitution _VPC_CONNECTOR."
echo ""
echo "Wait ~30s, then test login:"
echo "  curl -s -X POST \"${API_URL}/api/v1/auth/admin/login\" \\"
echo "    -H \"Content-Type: application/json\" \\"
echo "    -d '{\"clientId\":1,\"userName\":\"seed.admin\",\"password\":\"Pass@123\"}'"
