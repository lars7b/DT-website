# parameters met standaardwaarden
param(
    [string]$DatabaseName = "dt_website",
    [string]$PostgresUser = "postgres",
    [string]$DbHost = "localhost",
    [int]$Port = 5432,
    [string]$Password
)

# Strikte modus en foutafhandeling: script stopt bij fouten.
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# psql command is beschikbaar om met PostgreSQL te communiceren.
if (-not (Get-Command psql -ErrorAction SilentlyContinue)) {
    throw "psql is niet gevonden. Installeer PostgreSQL client tools en probeer opnieuw."
}

# om psql commando's uit te voeren en fouten af te handelen.
function Invoke-Psql {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments
    )

    $output = & psql @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "psql command mislukt: psql $($Arguments -join ' ')"
    }

    if ($null -eq $output) {
        return ""
    }

    return ($output | Out-String)
}

# bestanden voor schema en data scripts bepalen op basis van de locatie van dit script.
$schemaFile = Join-Path $PSScriptRoot "01-schema.sql"
$dataFile = Join-Path $PSScriptRoot "02-data.sql"

# schema bestand moet bestaan.
if (-not (Test-Path $schemaFile)) {
    throw "Schema bestand niet gevonden: $schemaFile"
}

# Tjijdelijke wijziging van PGPASSWORD om wachtwoord door te geven aan psql commando's. 
$previousPassword = $env:PGPASSWORD
if ($Password) {
    $env:PGPASSWORD = $Password
}

try {
    # veilige SQL query: we gebruiken eenvoudige aanhalingstekens in de query.
    $safeDbName = $DatabaseName.Replace("'", "''")

    # Check of de database al bestaat door te zoeken in pg_database systeemcatalogus.
    $existsOutput = Invoke-Psql -Arguments @("-h", $DbHost, "-p", "$Port", "-U", $PostgresUser, "-d", "postgres", "-t", "-A", "-c", "SELECT 1 FROM pg_database WHERE datname = '$safeDbName';")
    $exists = $existsOutput.Trim()

    # Creer de database alleen als deze nog niet bestaat. 
    if ($exists -ne "1") {
        [void](Invoke-Psql -Arguments @("-h", $DbHost, "-p", "$Port", "-U", $PostgresUser, "-d", "postgres", "-c", "CREATE DATABASE `"$DatabaseName`";"))
        Write-Host "Database '$DatabaseName' aangemaakt."
    }
    else {
        Write-Host "Database '$DatabaseName' bestaat al."
    }

    # voeg schema toe aan de database. Dit zal tabellen en andere objecten aanmaken zoals gedefinieerd in het schema bestand.
    [void](Invoke-Psql -Arguments @("-h", $DbHost, "-p", "$Port", "-U", $PostgresUser, "-d", $DatabaseName, "-f", $schemaFile))
    Write-Host "Schema toegepast op '$DatabaseName'."

    # Laad data in de database als het data bestand bestaat en niet leeg is.
    if ((Get-Item $dataFile -ErrorAction SilentlyContinue) -and (Get-Item $dataFile).Length -gt 0) {
        [void](Invoke-Psql -Arguments @("-h", $DbHost, "-p", "$Port", "-U", $PostgresUser, "-d", $DatabaseName, "-f", $dataFile))
        Write-Host "Data script uitgevoerd op '$DatabaseName'."
    }

    # Print een simpele controle: het aantal tabellen in het public schema. Dit geeft een indicatie dat de setup succesvol is en dat er daadwerkelijk tabellen zijn aangemaakt.
    $tableCountOutput = Invoke-Psql -Arguments @("-h", $DbHost, "-p", "$Port", "-U", $PostgresUser, "-d", $DatabaseName, "-t", "-A", "-c", "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema='public' AND table_type='BASE TABLE';")
    $tableCount = $tableCountOutput.Trim()
    Write-Host "Klaar. '$DatabaseName' heeft $tableCount tabellen in schema public."
}
finally {
    # restore vorige waarde van PGPASSWORD.
    $env:PGPASSWORD = $previousPassword
}
