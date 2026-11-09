<#
    Aplica las migraciones de Entity Framework a la base unificada indicada.
    Por defecto aplica a la base primaria (lurin_smart).
    Use -Database lurin_smart_backup para preparar la base de respaldo.
#>
param(
    [string]$HostName = "localhost",
    [int]$Port = 5432,
    [string]$User = "postgres",
    [string]$Password = "postgre",
    [string]$Database = "lurin_smart"
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")

$efCommand = Get-Command dotnet-ef -ErrorAction SilentlyContinue
$efPath = if ($efCommand) { $efCommand.Source } else { $null }

if (-not $efPath) {
    $candidate = Join-Path (Join-Path $env:USERPROFILE ".dotnet\tools") "dotnet-ef.exe"
    if (Test-Path -LiteralPath $candidate) {
        $efPath = $candidate
    }
}

if (-not $efPath) {
    throw "No se encontró 'dotnet-ef'. Instale la herramienta: dotnet tool install --global dotnet-ef --version 8.0.11"
}

$env:LURIN_DESIGN_CONNECTION = "Host=$HostName;Port=$Port;Database=$Database;Username=$User;Password=$Password"

$services = @(
    @{ Name = "ms-auth"; WorkingDirectory = "backend\ms-auth"; Project = "Auth.Infrastructure"; StartupProject = "Auth.Api" },
    @{ Name = "ms-inventory"; WorkingDirectory = "backend\ms-inventory"; Project = "src\MsInventory.Infrastructure"; StartupProject = "src\MsInventory.Api" },
    @{ Name = "ms-delivery"; WorkingDirectory = "backend\ms-delivery"; Project = "src\MsDelivery.Infrastructure"; StartupProject = "src\MsDelivery.Api" },
    @{ Name = "ms-services"; WorkingDirectory = "backend\ms-services"; Project = "src\MsServices.Infrastructure"; StartupProject = "src\MsServices.Api" },
    @{ Name = "ms-irrigation"; WorkingDirectory = "backend\ms-irrigation"; Project = "Irrigation.Infrastructure"; StartupProject = "Irrigation.Api" },
    @{ Name = "ms-iot-monitoring"; WorkingDirectory = "backend\ms-iot-monitoring"; Project = "Iot.Infrastructure"; StartupProject = "Iot.Api" }
)

foreach ($service in $services) {
    $workingDirectory = Join-Path $repoRoot $service.WorkingDirectory
    Write-Host "Aplicando migraciones de $($service.Name) en '$Database'..."

    Push-Location $workingDirectory
    try {
        & $efPath database update --project $service.Project --startup-project $service.StartupProject
        if ($LASTEXITCODE -ne 0) {
            throw "Falló la migración de $($service.Name)."
        }
    }
    finally {
        Pop-Location
    }
}

Write-Host "Migraciones aplicadas correctamente en '$Database'."
