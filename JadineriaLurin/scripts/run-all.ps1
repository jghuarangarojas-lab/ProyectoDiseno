#!/usr/bin/env pwsh
<#
    Levanta todos los microservicios del backend y el frontend web.
    Requiere PostgreSQL y, para riego/IoT, un broker MQTT (Mosquitto) en ejecucion.
    Ejemplos:
        .\scripts\run-all.ps1
        .\scripts\run-all.ps1 -SkipBuild -SkipInstall
        .\scripts\run-all.ps1 -DryRun
#>
[CmdletBinding()]
param(
    [switch]$SkipBuild,
    [switch]$SkipFrontend,
    [switch]$SkipInstall,
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$frontendDir = Join-Path $repoRoot "frontend\lurin-smart-web"

$services = @(
    @{ Name = "ms-auth";           Project = "backend\ms-auth\Auth.Api";               Url = "http://localhost:5080" },
    @{ Name = "ms-inventory";      Project = "backend\ms-inventory\src\MsInventory.Api"; Url = "http://localhost:5213" },
    @{ Name = "ms-delivery";       Project = "backend\ms-delivery\src\MsDelivery.Api";   Url = "http://localhost:5214" },
    @{ Name = "ms-services";       Project = "backend\ms-services\src\MsServices.Api";   Url = "http://localhost:5215" },
    @{ Name = "ms-iot-monitoring"; Project = "backend\ms-iot-monitoring\Iot.Api";        Url = "http://localhost:5084" },
    @{ Name = "ms-irrigation";     Project = "backend\ms-irrigation\Irrigation.Api";     Url = "http://localhost:5083" }
)

function Resolve-ProjectPath {
    param([string]$RelativePath)

    $full = Join-Path $repoRoot $RelativePath
    if (-not (Test-Path -LiteralPath $full)) {
        throw "No se encontro el proyecto: $full"
    }

    return $full
}

Write-Host "Repositorio: $repoRoot" -ForegroundColor Cyan

$resolved = foreach ($service in $services) {
    [pscustomobject]@{
        Name        = $service.Name
        ProjectPath = Resolve-ProjectPath -RelativePath $service.Project
        Url         = $service.Url
    }
}

if (-not $SkipBuild) {
    foreach ($service in $resolved) {
        Write-Host "Compilando $($service.Name)..." -ForegroundColor Cyan
        if ($DryRun) {
            Write-Host "  dotnet build `"$($service.ProjectPath)`"" -ForegroundColor DarkGray
            continue
        }

        dotnet build $service.ProjectPath --nologo
        if ($LASTEXITCODE -ne 0) {
            throw "Fallo la compilacion de $($service.Name)."
        }
    }
}

$env:ASPNETCORE_ENVIRONMENT = "Development"

foreach ($service in $resolved) {
    $arguments = @(
        "run",
        "--project", $service.ProjectPath,
        "--no-launch-profile",
        "--urls", $service.Url
    )
    if (-not $SkipBuild) {
        $arguments += "--no-build"
    }

    Write-Host "Iniciando $($service.Name) en $($service.Url)..." -ForegroundColor Green
    if ($DryRun) {
        Write-Host "  dotnet $($arguments -join ' ')" -ForegroundColor DarkGray
        continue
    }

    Start-Process -FilePath "dotnet" -ArgumentList $arguments -WorkingDirectory $service.ProjectPath | Out-Null
    Start-Sleep -Milliseconds 800
}

if (-not $SkipFrontend) {
    $envFile = Join-Path $frontendDir ".env"
    $exampleFile = Join-Path $frontendDir ".env.example"

    if ((-not (Test-Path -LiteralPath $envFile)) -and (Test-Path -LiteralPath $exampleFile)) {
        Write-Host "Creando .env desde .env.example..." -ForegroundColor Cyan
        if ($DryRun) {
            Write-Host "  Copy-Item `"$exampleFile`" `"$envFile`"" -ForegroundColor DarkGray
        }
        else {
            Copy-Item -LiteralPath $exampleFile -Destination $envFile
        }
    }

    $nodeModules = Join-Path $frontendDir "node_modules"
    if ((-not $SkipInstall) -and (-not (Test-Path -LiteralPath $nodeModules))) {
        Write-Host "Instalando dependencias del frontend..." -ForegroundColor Cyan
        if ($DryRun) {
            Write-Host "  npm install (en $frontendDir)" -ForegroundColor DarkGray
        }
        else {
            Push-Location $frontendDir
            try {
                npm install
                if ($LASTEXITCODE -ne 0) {
                    throw "Fallo npm install."
                }
            }
            finally {
                Pop-Location
            }
        }
    }

    Write-Host "Iniciando frontend en http://localhost:5173..." -ForegroundColor Green
    if ($DryRun) {
        Write-Host "  npm run dev (en $frontendDir)" -ForegroundColor DarkGray
    }
    else {
        Start-Process -FilePath "npm.cmd" -ArgumentList @("run", "dev") -WorkingDirectory $frontendDir | Out-Null
    }
}

if ($DryRun) {
    Write-Host "Simulacion completada (no se inicio nada)." -ForegroundColor Yellow
    return
}

Write-Host ""
Write-Host "Servicios iniciados:" -ForegroundColor Green
foreach ($service in $resolved) {
    Write-Host ("  {0,-18} {1}" -f $service.Name, $service.Url)
}
if (-not $SkipFrontend) {
    Write-Host ("  {0,-18} {1}" -f "frontend", "http://localhost:5173")
}

Write-Host ""
Write-Host "Cada servicio corre en su propia ventana. Cierra esas ventanas para detenerlos." -ForegroundColor Yellow
