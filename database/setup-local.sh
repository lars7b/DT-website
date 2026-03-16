#!/usr/bin/env bash
set -euo pipefail

DATABASE_NAME="${1:-dt_website}"
PGHOST="${PGHOST:-localhost}"
PGPORT="${PGPORT:-5432}"
PGUSER="${PGUSER:-postgres}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SCHEMA_FILE="$SCRIPT_DIR/01-schema.sql"
DATA_FILE="$SCRIPT_DIR/02-data.sql"

if ! command -v psql >/dev/null 2>&1; then
  echo "psql is niet gevonden. Installeer PostgreSQL client tools en probeer opnieuw." >&2
  exit 1
fi

if [[ ! -f "$SCHEMA_FILE" ]]; then
  echo "Schema bestand niet gevonden: $SCHEMA_FILE" >&2
  exit 1
fi

SAFE_DB_NAME=${DATABASE_NAME//\'/\'\'}
EXISTS=$(psql -h "$PGHOST" -p "$PGPORT" -U "$PGUSER" -d postgres -t -A -c "SELECT 1 FROM pg_database WHERE datname = '$SAFE_DB_NAME';" | tr -d '[:space:]')

if [[ "$EXISTS" != "1" ]]; then
  psql -h "$PGHOST" -p "$PGPORT" -U "$PGUSER" -d postgres -c "CREATE DATABASE \"$DATABASE_NAME\";"
  echo "Database '$DATABASE_NAME' aangemaakt."
else
  echo "Database '$DATABASE_NAME' bestaat al."
fi

psql -h "$PGHOST" -p "$PGPORT" -U "$PGUSER" -d "$DATABASE_NAME" -f "$SCHEMA_FILE"
echo "Schema toegepast op '$DATABASE_NAME'."

if [[ -s "$DATA_FILE" ]]; then
  psql -h "$PGHOST" -p "$PGPORT" -U "$PGUSER" -d "$DATABASE_NAME" -f "$DATA_FILE"
  echo "Data script uitgevoerd op '$DATABASE_NAME'."
fi

TABLE_COUNT=$(psql -h "$PGHOST" -p "$PGPORT" -U "$PGUSER" -d "$DATABASE_NAME" -t -A -c "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema='public' AND table_type='BASE TABLE';" | tr -d '[:space:]')
echo "Klaar. '$DATABASE_NAME' heeft $TABLE_COUNT tabellen in schema public."
