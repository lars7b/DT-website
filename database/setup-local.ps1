param(
    [string]$DatabaseName = "dt_website",
    [string]$PostgresUser = "postgres",
    [string]$DbHost = "localhost",
    [int]$Port = 5432,
    [string]$Password
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if (-not (Get-Command psql -ErrorAction SilentlyContinue)) {
    throw "psql is niet gevonden. Installeer PostgreSQL client tools en probeer opnieuw."
}

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

$schemaFile = Join-Path $PSScriptRoot "01-schema.sql"
$dataFile = Join-Path $PSScriptRoot "02-data.sql"

if (-not (Test-Path $schemaFile)) {
    throw "Schema bestand niet gevonden: $schemaFile"
}

$previousPassword = $env:PGPASSWORD
if ($Password) {
    $env:PGPASSWORD = $Password
}

try {
    $safeDbName = $DatabaseName.Replace("'", "''")
    $existsOutput = Invoke-Psql -Arguments @("-h", $DbHost, "-p", "$Port", "-U", $PostgresUser, "-d", "postgres", "-t", "-A", "-c", "SELECT 1 FROM pg_database WHERE datname = '$safeDbName';")
    $exists = $existsOutput.Trim()

    if ($exists -ne "1") {
        [void](Invoke-Psql -Arguments @("-h", $DbHost, "-p", "$Port", "-U", $PostgresUser, "-d", "postgres", "-c", "CREATE DATABASE `"$DatabaseName`";"))
        Write-Host "Database '$DatabaseName' aangemaakt."
    }
    else {
        Write-Host "Database '$DatabaseName' bestaat al."
    }

    [void](Invoke-Psql -Arguments @("-h", $DbHost, "-p", "$Port", "-U", $PostgresUser, "-d", $DatabaseName, "-f", $schemaFile))
    Write-Host "Schema toegepast op '$DatabaseName'."

    if ((Get-Item $dataFile -ErrorAction SilentlyContinue) -and (Get-Item $dataFile).Length -gt 0) {
        [void](Invoke-Psql -Arguments @("-h", $DbHost, "-p", "$Port", "-U", $PostgresUser, "-d", $DatabaseName, "-f", $dataFile))
        Write-Host "Data script uitgevoerd op '$DatabaseName'."
    }

    $tableCountOutput = Invoke-Psql -Arguments @("-h", $DbHost, "-p", "$Port", "-U", $PostgresUser, "-d", $DatabaseName, "-t", "-A", "-c", "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema='public' AND table_type='BASE TABLE';")
    $tableCount = $tableCountOutput.Trim()
    Write-Host "Klaar. '$DatabaseName' heeft $tableCount tabellen in schema public."
}
finally {
    $env:PGPASSWORD = $previousPassword
}
