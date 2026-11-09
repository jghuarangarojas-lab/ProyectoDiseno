<#
    Crea la base de datos unificada de Jardinería Lurín Smart y su base de respaldo.
    Cada microservicio usa un esquema propio dentro de la misma base:
        auth, inventory, delivery, services, irrigation, iot
#>
param(
    [string]$HostName = "localhost",
    [int]$Port = 5432,
    [string]$User = "postgres",
    [string]$Password = "postgre",
    [string]$PrimaryDatabase = "lurin_smart",
    [string]$BackupDatabase = "lurin_smart_backup",
    [string]$PgBin = "C:\Program Files\PostgreSQL\18\bin"
)

$ErrorActionPreference = "Stop"

$env:PGPASSWORD = $Password

$psql = Join-Path $PgBin "psql.exe"
if (-not (Test-Path -LiteralPath $psql)) {
    $command = Get-Command psql -ErrorAction SilentlyContinue
    if (-not $command) {
        throw "No se encontró psql. Instale PostgreSQL o indique -PgBin."
    }
    $psql = $command.Source
}

$schemas = @("auth", "inventory", "delivery", "services", "irrigation", "iot")

function Invoke-Psql {
    param(
        [string]$Database,
        [string]$Sql
    )

    & $psql -h $HostName -p $Port -U $User -d $Database -v ON_ERROR_STOP=1 -c $Sql
    if ($LASTEXITCODE -ne 0) {
        throw "psql falló al ejecutar: $Sql"
    }
}

function Test-DatabaseExists {
    param([string]$Database)

    $result = & $psql -h $HostName -p $Port -U $User -d postgres -tAc `
        "SELECT 1 FROM pg_database WHERE datname = '$Database';"

    if ($LASTEXITCODE -ne 0) {
        throw "No se pudo consultar la lista de bases de datos."
    }

    return ($result | Out-String).Trim() -eq "1"
}

foreach ($database in @($PrimaryDatabase, $BackupDatabase)) {
    if (Test-DatabaseExists -Database $database) {
        Write-Host "La base '$database' ya existe."
    }
    else {
        Write-Host "Creando base '$database'..."
        Invoke-Psql -Database "postgres" -Sql "CREATE DATABASE $database;"
    }

    foreach ($schema in $schemas) {
        Invoke-Psql -Database $database -Sql "CREATE SCHEMA IF NOT EXISTS $schema;"
    }

    Write-Host "Esquemas asegurados en '$database': $($schemas -join ', ')."
}

Write-Host ""
Write-Host "Base unificada lista:"
Write-Host "  Primaria: $PrimaryDatabase"
Write-Host "  Respaldo: $BackupDatabase"
