# AMMS GCP deployment (Cloud Run + Cloud SQL)

**Full guide for Git repo connect → Cloud Build → Cloud Run:** see [DEPLOY-GCP.md](../DEPLOY-GCP.md).

## Prerequisites

- [Google Cloud SDK](https://cloud.google.com/sdk/docs/install) (`gcloud`)
- [Docker](https://docs.docker.com/get-docker/)
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [SqlPackage](https://aka.ms/sqlpackage) (for database publish)
- [Cloud SQL Auth Proxy](https://cloud.google.com/sql/docs/sqlserver/sql-proxy) (for `publish-database.sh`)

## Quick start

1. Copy environment template and edit values:

   ```bash
   cp deploy/env.example deploy/.env
   # Edit deploy/.env — set GCP_PROJECT_ID, passwords, region, etc.
   ```

2. Provision GCP resources (Cloud SQL, Artifact Registry, secrets, IAM):

   ```bash
   chmod +x deploy/*.sh
   ./deploy/gcp-setup.sh
   ```

3. Install publish tools (once, on macOS):

   ```bash
   ./deploy/install-tools.sh
   export PATH="$HOME/.dotnet/tools:$(pwd)/deploy/tools:$PATH"
   ```

4. Publish database schema:

   ```bash
   ./deploy/publish-database.sh
   ```

   On Mac without Visual Studio, the script applies SQL files automatically if dacpac build is unavailable.

4. Build images and deploy both Cloud Run services:

   ```bash
   ./deploy/deploy-all.sh
   ```

## Local development secrets

Do not commit real passwords. For local API runs, use User Secrets or environment variables:

```bash
cd AMMS.API
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=...;Database=AMMS;..."
dotnet user-secrets set "Jwt:Key" "your-dev-key-at-least-32-chars"
```

## Cloud Build CI/CD

Trigger automated builds with substitutions (set in Cloud Build trigger):

```bash
gcloud builds submit AMMS --config=AMMS/cloudbuild.yaml \
  --substitutions=_REGION=asia-south1,_AR_REPO=amms,_CLOUDSQL_INSTANCE=amms-sql,...
```

See [cloudbuild.yaml](../cloudbuild.yaml) for required substitution variables.

## Connection string on Cloud Run

On Cloud Run, the API connects to Cloud SQL **private IP** over a Serverless VPC Access connector (`.NET SqlClient` does not work with the `127.0.0.1` Cloud SQL sidecar for SQL Server). Run `./deploy/enable-cloudrun-private-sql.sh` once, then keep `VPC_CONNECTOR=amms-connector` in `.env`. The connection string is in Secret Manager (`amms-db-connection`).
