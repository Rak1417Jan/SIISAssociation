#!/usr/bin/env bash
# Provision GCP resources for AMMS on Cloud Run + Cloud SQL.
# Prerequisites: gcloud CLI, billing enabled, owner/editor on project.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=/dev/null
source "${SCRIPT_DIR}/.env"

: "${GCP_PROJECT_ID:?Set GCP_PROJECT_ID in deploy/.env}"
: "${GCP_REGION:?Set GCP_REGION in deploy/.env}"
: "${CLOUDSQL_INSTANCE_NAME:?Set CLOUDSQL_INSTANCE_NAME in deploy/.env}"
: "${SQL_USER:?Set SQL_USER in deploy/.env}"
: "${SQL_PASSWORD:?Set SQL_PASSWORD in deploy/.env}"
: "${SQL_DATABASE:?Set SQL_DATABASE in deploy/.env}"
: "${AR_REPO:?Set AR_REPO in deploy/.env}"
: "${RUN_SERVICE_ACCOUNT:?Set RUN_SERVICE_ACCOUNT in deploy/.env}"
: "${SECRET_DB_CONNECTION:?Set SECRET_DB_CONNECTION in deploy/.env}"
: "${SECRET_JWT_KEY:?Set SECRET_JWT_KEY in deploy/.env}"

CLOUDSQL_CONNECTION_NAME="${GCP_PROJECT_ID}:${GCP_REGION}:${CLOUDSQL_INSTANCE_NAME}"
AR_HOST="${GCP_REGION}-docker.pkg.dev"
IMAGE_PREFIX="${AR_HOST}/${GCP_PROJECT_ID}/${AR_REPO}"
SA_EMAIL="${RUN_SERVICE_ACCOUNT}@${GCP_PROJECT_ID}.iam.gserviceaccount.com"

echo "==> Setting gcloud project to ${GCP_PROJECT_ID}"
gcloud config set project "${GCP_PROJECT_ID}"

echo "==> Enabling required APIs"
gcloud services enable \
  run.googleapis.com \
  sqladmin.googleapis.com \
  secretmanager.googleapis.com \
  artifactregistry.googleapis.com \
  cloudbuild.googleapis.com \
  iam.googleapis.com

echo "==> Creating Artifact Registry repository: ${AR_REPO}"
gcloud artifacts repositories describe "${AR_REPO}" \
  --location="${GCP_REGION}" 2>/dev/null || \
gcloud artifacts repositories create "${AR_REPO}" \
  --repository-format=docker \
  --location="${GCP_REGION}" \
  --description="AMMS container images"

echo "==> Creating Cloud SQL SQL Server instance: ${CLOUDSQL_INSTANCE_NAME}"
if gcloud sql instances describe "${CLOUDSQL_INSTANCE_NAME}" --format="value(name)" 2>/dev/null; then
  echo "    Instance already exists, skipping create."
else
  gcloud sql instances create "${CLOUDSQL_INSTANCE_NAME}" \
    --database-version=SQLSERVER_2022_STANDARD \
    --tier=db-custom-2-7680 \
    --region="${GCP_REGION}" \
    --root-password="${SQL_PASSWORD}" \
    --storage-type=SSD \
    --storage-size=20GB
fi

echo "==> Creating database: ${SQL_DATABASE}"
gcloud sql databases create "${SQL_DATABASE}" \
  --instance="${CLOUDSQL_INSTANCE_NAME}" 2>/dev/null || echo "    Database may already exist."

echo "==> Creating SQL user: ${SQL_USER}"
gcloud sql users create "${SQL_USER}" \
  --instance="${CLOUDSQL_INSTANCE_NAME}" \
  --password="${SQL_PASSWORD}" 2>/dev/null || \
gcloud sql users set-password "${SQL_USER}" \
  --instance="${CLOUDSQL_INSTANCE_NAME}" \
  --password="${SQL_PASSWORD}"

echo "==> Creating service account: ${SA_EMAIL}"
gcloud iam service-accounts describe "${SA_EMAIL}" 2>/dev/null || \
gcloud iam service-accounts create "${RUN_SERVICE_ACCOUNT}" \
  --display-name="AMMS Cloud Run runtime"

for ROLE in roles/cloudsql.client roles/secretmanager.secretAccessor; do
  gcloud projects add-iam-policy-binding "${GCP_PROJECT_ID}" \
    --member="serviceAccount:${SA_EMAIL}" \
    --role="${ROLE}" \
    --quiet >/dev/null
done

# Placeholder secret; after setup run ./deploy/enable-cloudrun-private-sql.sh for private IP + VPC.
DB_CONN="Server=127.0.0.1,1433;Database=${SQL_DATABASE};User Id=${SQL_USER};Password=${SQL_PASSWORD};Encrypt=True;TrustServerCertificate=True;"

echo "==> Creating Secret Manager secrets"
echo -n "${DB_CONN}" | gcloud secrets create "${SECRET_DB_CONNECTION}" \
  --data-file=- 2>/dev/null || \
echo -n "${DB_CONN}" | gcloud secrets versions add "${SECRET_DB_CONNECTION}" --data-file=-

if [ -z "${JWT_KEY:-}" ]; then
  JWT_KEY="$(openssl rand -base64 48)"
  echo "    Generated JWT_KEY (store securely): ${JWT_KEY}"
fi
echo -n "${JWT_KEY}" | gcloud secrets create "${SECRET_JWT_KEY}" \
  --data-file=- 2>/dev/null || \
echo -n "${JWT_KEY}" | gcloud secrets versions add "${SECRET_JWT_KEY}" --data-file=-

echo ""
echo "Setup complete."
echo "  Cloud SQL connection name: ${CLOUDSQL_CONNECTION_NAME}"
echo "  Image prefix: ${IMAGE_PREFIX}"
echo "  Service account: ${SA_EMAIL}"
echo ""
echo "Next:"
echo "  ./deploy/setup-cloudbuild-iam.sh"
echo "  ./deploy/publish-database.sh"
echo "  ./deploy/enable-cloudrun-private-sql.sh   # after first Cloud Run deploy"
echo "  Connect Git repo + Cloud Build trigger (see DEPLOY-GCP.md)"
