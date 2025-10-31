param(
    [switch]$Seed,
    [switch]$FollowLogs,
    [switch]$NoObservability
)

$ErrorActionPreference = "Stop"
Write-Host "🚀 Dev Up: API + Observabilidad" -ForegroundColor Cyan

$scriptsDir = $PSScriptRoot

# 1) API (DB + Web)
Write-Host "▶️  Levantando API (DB + Web)..." -ForegroundColor Cyan
$apiArgs = @()
if ($Seed) { $apiArgs += "-Seed" }
if ($FollowLogs) { $apiArgs += "-FollowLogs" }

& pwsh -NoProfile -ExecutionPolicy Bypass -File (Join-Path $scriptsDir "up.ps1") @apiArgs
if ($LASTEXITCODE -ne 0) { throw "Fallo el arranque de API" }

# 2) Observabilidad (opcional)
if (-not $NoObservability) {
    Write-Host "▶️  Levantando observabilidad..." -ForegroundColor Cyan
    & pwsh -NoProfile -ExecutionPolicy Bypass -File (Join-Path $scriptsDir "observability.ps1") -Open
    if ($LASTEXITCODE -ne 0) { throw "Fallo el arranque de Observabilidad" }
}

Write-Host "✅ Entorno de desarrollo listo." -ForegroundColor Green
