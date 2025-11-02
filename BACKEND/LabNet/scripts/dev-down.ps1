param(
    [switch]$KeepDbVolume
)

$ErrorActionPreference = "Stop"
Write-Host "🛑 Dev Down: Observabilidad + API" -ForegroundColor Cyan

$scriptsDir = $PSScriptRoot

# 1) Observabilidad primero
Write-Host "▶️  Bajando observabilidad..." -ForegroundColor Cyan
& pwsh -NoProfile -ExecutionPolicy Bypass -File (Join-Path $scriptsDir "observability.ps1") -Down

# 2) API
Write-Host "▶️  Bajando API..." -ForegroundColor Cyan
if ($KeepDbVolume) {
    # down regular (mantiene volúmenes)
    & pwsh -NoProfile -ExecutionPolicy Bypass -File (Join-Path $scriptsDir "up.ps1") -Down
} else {
    # down -v para limpiar volúmenes
    $rootDir = Split-Path -Parent $scriptsDir
    $composeFile = Join-Path $rootDir "docker-compose.yml"
    $hasDocker = Get-Command docker -ErrorAction SilentlyContinue
    if (-not $hasDocker) { throw "Docker no disponible" }
    try {
        docker compose -f $composeFile down -v
    } catch {
        docker-compose -f $composeFile down -v
    }
}

Write-Host "✅ Todo abajo." -ForegroundColor Green
