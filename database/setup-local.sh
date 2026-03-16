#!/usr/bin/env bash

set -euo pipefail

# Database naam lezen van eerste argument, anders default gebruiken.
DATABASE_NAME="${1:-dt_website}"
#Toelaten overriden van host/port/user via omgevingsvariabelen.
PGHOST="${PGHOST:-localhost}"
PGPORT="${PGPORT:-5432}"
PGUSER="${PGUSER:-postgres}"

# relatieve paden naar schema en data scripts bepalen op basis van de locatie van dit script.
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SCHEMA_FILE="$SCRIPT_DIR/01-schema.sql"
DATA_FILE="$SCRIPT_DIR/02-data.sql"

# Verificatie: psql command beschikbaar, schema bestand bestaat.
if ! command -v psql >/dev/null 2>&1; then
  echo "psql is niet gevonden. Installeer PostgreSQL client tools en probeer opnieuw." >&2
  exit 1
fi

# Schema bestand moet bestaan, anders kunnen we niet verder. Data bestand is optioneel.
if [[ ! -f "$SCHEMA_FILE" ]]; then
  echo "Schema bestand niet gevonden: $SCHEMA_FILE" >&2
  exit 1
fi

# Escape de database naam voor veilige SQL query. We gebruiken eenvoudige aanhalingstekens in de query, dus we moeten eventuele enkele aanhalingstekens in de naam verdubbelen.
SAFE_DB_NAME=${DATABASE_NAME//\'/\'\'}
# Check of de database al bestaat.
EXISTS=$(psql -h "$PGHOST" -p "$PGPORT" -U "$PGUSER" -d postgres -t -A -c "SELECT 1 FROM pg_database WHERE datname = '$SAFE_DB_NAME';" | tr -d '[:space:]')

# Creeer de database alleen als deze nog niet bestaat. Dit voorkomt fouten bij het opnieuw uitvoeren van dit script.
if [[ "$EXISTS" != "1" ]]; then
  psql -h "$PGHOST" -p "$PGPORT" -U "$PGUSER" -d postgres -c "CREATE DATABASE \"$DATABASE_NAME\";"
  echo "Database '$DATABASE_NAME' aangemaakt."
else
  echo "Database '$DATABASE_NAME' bestaat al."
fi

# Toepassen van schema op de database. Dit zal tabellen en andere objecten aanmaken zoals gedefinieerd in het schema bestand.
psql -h "$PGHOST" -p "$PGPORT" -U "$PGUSER" -d "$DATABASE_NAME" -f "$SCHEMA_FILE"
echo "Schema toegepast op '$DATABASE_NAME'."

# Laad data in de database als het data bestand bestaat en niet leeg is.
if [[ -s "$DATA_FILE" ]]; then
  psql -h "$PGHOST" -p "$PGPORT" -U "$PGUSER" -d "$DATABASE_NAME" -f "$DATA_FILE"
  echo "Data script uitgevoerd op '$DATABASE_NAME'."
fi

#  tel het aantal tabellen in het public schema en geef dit weer. Dit geeft een indicatie dat de setup succesvol is en dat er daadwerkelijk tabellen zijn aangemaakt.
TABLE_COUNT=$(psql -h "$PGHOST" -p "$PGPORT" -U "$PGUSER" -d "$DATABASE_NAME" -t -A -c "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema='public' AND table_type='BASE TABLE';" | tr -d '[:space:]')
echo "Klaar. '$DATABASE_NAME' heeft $TABLE_COUNT tabellen in schema public."
