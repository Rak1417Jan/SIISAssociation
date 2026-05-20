#!/usr/bin/env bash
# Grant Cloud Build service account permissions to build, push, and deploy AMMS.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=/dev/null
source "${SCRIPT_DIR}/.env"

: "${GCP_PROJECT_ID:?Set GCP_PROJECT_ID in deploy/.env}"
: "${RUN_SERVICE_ACCOUNT:?Set RUN_SERVICE_ACCOUNT in deploy/.env}"

SA_EMAIL="${RUN_SERVICE_ACCOUNT}@${GCP_PROJECT_ID}.iam.gserviceaccount.com"
PROJECT_NUMBER="$(gcloud projects describe "${GCP_PROJECT_ID}" --format='value(projectNumber)')"
CB_SA="${PROJECT_NUMBER}@cloudbuild.gserviceaccount.com"

echo "==> Project: ${GCP_PROJECT_ID} (${PROJECT_NUMBER})"
echo "==> Cloud Build SA: ${CB_SA}"
echo "==> Cloud Run runtime SA: ${SA_EMAIL}"

gcloud config set project "${GCP_PROJECT_ID}"

ROLES=(
  roles/run.admin
  roles/artifactregistry.writer
  roles/iam.serviceAccountUser
  roles/secretmanager.secretAccessor
  roles/cloudsql.client
)

for ROLE in "${ROLES[@]}"; do
  echo "    Binding ${ROLE} -> ${CB_SA}"
  gcloud projects add-iam-policy-binding "${GCP_PROJECT_ID}" \
    --member="serviceAccount:${CB_SA}" \
    --role="${ROLE}" \
    --quiet >/dev/null
done

# Allow Cloud Build to deploy Cloud Run as amms-run
echo "    Allow Cloud Build to act as ${SA_EMAIL}"
gcloud iam service-accounts add-iam-policy-binding "${SA_EMAIL}" \
  --member="serviceAccount:${CB_SA}" \
  --role="roles/iam.serviceAccountUser" \
  --quiet >/dev/null

echo ""
echo "Cloud Build IAM setup complete."
echo "Connect your repo and create a trigger using cloudbuild.yaml at the repository root."
