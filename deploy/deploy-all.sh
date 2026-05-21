#!/usr/bin/env bash
# Build, push, and deploy AMMS.API and AMMS.Web to Cloud Run.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
AMMS_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
# shellcheck source=/dev/null
source "${SCRIPT_DIR}/.env"

: "${GCP_PROJECT_ID:?}"
: "${GCP_REGION:?}"
: "${CLOUDSQL_INSTANCE_NAME:?}"
: "${AR_REPO:?}"
: "${RUN_SERVICE_ACCOUNT:?}"
: "${API_SERVICE_NAME:?}"
: "${WEB_SERVICE_NAME:?}"
: "${SECRET_DB_CONNECTION:?}"
: "${SECRET_JWT_KEY:?}"
: "${JWT_ISSUER:?}"
: "${JWT_AUDIENCE:?}"

CLOUDSQL_CONNECTION_NAME="${GCP_PROJECT_ID}:${GCP_REGION}:${CLOUDSQL_INSTANCE_NAME}"
AR_HOST="${GCP_REGION}-docker.pkg.dev"
IMAGE_PREFIX="${AR_HOST}/${GCP_PROJECT_ID}/${AR_REPO}"
SA_EMAIL="${RUN_SERVICE_ACCOUNT}@${GCP_PROJECT_ID}.iam.gserviceaccount.com"
API_IMAGE="${IMAGE_PREFIX}/${API_SERVICE_NAME}:latest"
WEB_IMAGE="${IMAGE_PREFIX}/${WEB_SERVICE_NAME}:latest"

gcloud config set project "${GCP_PROJECT_ID}"
gcloud auth configure-docker "${AR_HOST}" --quiet

echo "==> Building and pushing ${API_SERVICE_NAME}"
docker build -f "${AMMS_ROOT}/AMMS.API/Dockerfile" -t "${API_IMAGE}" "${AMMS_ROOT}"
docker push "${API_IMAGE}"

echo "==> Building and pushing ${WEB_SERVICE_NAME}"
docker build -f "${AMMS_ROOT}/AMMS.Web/Dockerfile" -t "${WEB_IMAGE}" "${AMMS_ROOT}"
docker push "${WEB_IMAGE}"

API_DEPLOY_ARGS=(
  run deploy "${API_SERVICE_NAME}"
  --image="${API_IMAGE}"
  --region="${GCP_REGION}"
  --platform=managed
  --allow-unauthenticated
  --port=8080
  --set-secrets="ConnectionStrings__DefaultConnection=${SECRET_DB_CONNECTION}:latest,Jwt__Key=${SECRET_JWT_KEY}:latest"
  --set-env-vars="Jwt__Issuer=${JWT_ISSUER},Jwt__Audience=${JWT_AUDIENCE},ASPNETCORE_ENVIRONMENT=Production"
  --min-instances=1
  --service-account="${SA_EMAIL}"
)

if [ -n "${VPC_CONNECTOR:-}" ]; then
  API_DEPLOY_ARGS+=(
    --vpc-connector="${VPC_CONNECTOR}"
    --vpc-egress=private-ranges-only
  )
else
  API_DEPLOY_ARGS+=(--add-cloudsql-instances="${CLOUDSQL_CONNECTION_NAME}")
fi

echo "==> Deploying ${API_SERVICE_NAME}"
gcloud "${API_DEPLOY_ARGS[@]}"

echo "==> Deploying ${WEB_SERVICE_NAME}"
gcloud run deploy "${WEB_SERVICE_NAME}" \
  --image="${WEB_IMAGE}" \
  --region="${GCP_REGION}" \
  --platform=managed \
  --allow-unauthenticated \
  --port=8080 \
  --set-env-vars="ASPNETCORE_ENVIRONMENT=Production" \
  --service-account="${SA_EMAIL}"

echo ""
echo "Deployed services:"
gcloud run services describe "${API_SERVICE_NAME}" --region="${GCP_REGION}" --format='value(status.url)'
gcloud run services describe "${WEB_SERVICE_NAME}" --region="${GCP_REGION}" --format='value(status.url)'
