<#
    Sincroniza la base primaria hacia la base de respaldo.
    Programe este script (por ejemplo, cada 5 minutos) para mantener el respaldo actualizado.
#>
param(
    [string]$HostName = "localhost",
    [int]$Port = 5432,
    [string]$User = "postgres",
    [string]$Password = "postgre",
    [string]$PrimaryDatabase = "lurin_smart",
    [string]$BackupDatabase = "lurin_smart_backup",
    [string]$PgBin = "C:\Program Files\PostgreSQL\18\bin",
    [string]$LogPath = (Join-Path $PSScriptRoot "logs\sync-backup.log")
)

$ErrorActionPreference = "Stop"

if (-not [string]::IsNullOrWhiteSpace($LogPath)) {
    $logDirectory = Split-Path -Parent $LogPath
    if ($logDirectory) {
        New-Item -ItemType Directory -Path $logDirectory -Force | Out-Null
    }
}

function Write-Log {
    param([string]$Message)

    $line = "$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') $Message"
    Write-Host $line

    if (-not [string]::IsNullOrWhiteSpace($LogPath)) {
        Add-Content -LiteralPath $LogPath -Value $line -Encoding UTF8
    }
}

$env:PGPASSWORD = $Password

$pgDump = Join-Path $PgBin "pg_dump.exe"
$pgRestore = Join-Path $PgBin "pg_restore.exe"
$psql = Join-Path $PgBin "psql.exe"

foreach ($tool in @($pgDump, $pgRestore, $psql)) {
    if (-not (Test-Path -LiteralPath $tool)) {
        throw "No se encontró '$tool'. Indique la ruta correcta con -PgBin."
    }
}

$tempFile = Join-Path $env:TEMP "lurin_smart_$(Get-Date -Format 'yyyyMMddHHmmss').dump"
$startedAt = Get-Date

Write-Log "Inicio de sincronización '$PrimaryDatabase' -> '$BackupDatabase'."

try {
    Write-Log "Generando respaldo de '$PrimaryDatabase'..."
    & $pgDump -h $HostName -p $Port -U $User -d $PrimaryDatabase -Fc -f $tempFile
    if ($LASTEXITCODE -ne 0) {
        throw "pg_dump falló."
    }

    Write-Log "Cerrando conexiones activas a '$BackupDatabase'..."
    & $psql -h $HostName -p $Port -U $User -d postgres -v ON_ERROR_STOP=1 -c `
        "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '$BackupDatabase' AND pid <> pg_backend_pid();" | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "No se pudieron cerrar las conexiones de la base de respaldo."
    }

    Write-Log "Restaurando en '$BackupDatabase'..."
    & $pgRestore -h $HostName -p $Port -U $User -d $BackupDatabase `
        --clean --if-exists --no-owner --no-privileges --single-transaction $tempFile
    if ($LASTEXITCODE -ne 0) {
        throw "pg_restore falló."
    }

    Write-Log "Respaldo sincronizado correctamente."
}
catch {
    Write-Log "ERROR: $($_.Exception.Message)"
    throw
}
finally {
    if (Test-Path -LiteralPath $tempFile) {
        Remove-Item -LiteralPath $tempFile -Force
    }

    $duration = [math]::Round(((Get-Date) - $startedAt).TotalSeconds, 1)
    Write-Log "Fin de sincronización (duración: $duration s)."
}
