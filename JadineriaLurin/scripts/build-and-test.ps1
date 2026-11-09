#!/usr/bin/env pwsh
[CmdletBinding()]
param(
    [switch],
    [switch]
)

$ErrorActionPreference = ""Stop""

$root = Split-Path -Parent $PSScriptRoot
$backend = Join-Path $root ""backend""

$services = @(
    @{Name=""ms-auth""; Solution=""ms-auth.sln""},
    @{Name=""ms-inventory""; Solution=""MsInventory.sln""},
    @{Name=""ms-iot-monitoring""; Solution=""Iot.sln""},
    @{Name=""ms-irrigation""; Solution=""Irrigation.sln""},
    @{Name=""ms-delivery""; Solution=""MsDelivery.sln""},
    @{Name=""ms-services""; Solution=""MsServices.sln""}
)

foreach ($s in $services) {
    $path = Join-Path $backend $s.Name
    $sln = Join-Path $path $s.Solution
    if (-not (Test-Path $sln)) {
        Write-Host ""[WARN] Solution not found: $sln"" -ForegroundColor Yellow
        continue
    }
    Write-Host ""=== Building $s.Name ==="" -ForegroundColor Cyan
    if (-not $SkipRestore) {
        dotnet restore $sln
    }
    dotnet build $sln -c Release --no-restore
    if (-not $SkipTests) {
        Write-Host ""=== Testing $s.Name ==="" -ForegroundColor Cyan
        dotnet test $sln -c Release --no-restore --no-build
    }
}

Write-Host ""All done."" -ForegroundColor Green
