#!/usr/bin/env bash
# Redirect: 127.0.0.1 + --add-cloudsql-instances does not work for .NET + SQL Server on Cloud Run.
exec "$(dirname "$0")/enable-cloudrun-private-sql.sh" "$@"
