param(
    [switch]$Down,
    [switch]$FollowLogs,
    [switch]$Open
)

$ErrorActionPreference = "Stop"
Write-Host "ℹ️  Observabilidad (Grafana, Prometheus, Tempo, Seq, OTel Collector)" -ForegroundColor Cyan

$rootDir = Split-Path -Parent $PSScriptRoot
$composeFile = Join-Path $rootDir "docker\docker-compose.observability.yml"
if (-not (Test-Path $composeFile)) { throw "No se encontró: $composeFile" }

function Get-ComposeVariant {
    $hasDocker = Get-Command docker -ErrorAction SilentlyContinue
    $hasLegacy = Get-Command docker-compose -ErrorAction SilentlyContinue
    if ($hasDocker) { try { & docker compose version | Out-Null; if ($LASTEXITCODE -eq 0) { return "v2" } } catch { } }
    if ($hasLegacy) { return "legacy" }
    throw "⛔️  No se encontró ni 'docker compose' (v2) ni 'docker-compose' (legacy)."
}
$composeVariant = Get-ComposeVariant
$script:composeVariant = $composeVariant
Write-Host "ℹ️  Docker Compose: $composeVariant" -ForegroundColor Cyan

function Invoke-ComposeObs {
    param([Parameter(ValueFromRemainingArguments=$true)][string[]]$Args)
    if ($script:composeVariant -eq "legacy") {
        & docker-compose -f $composeFile @Args
    } else {
        & docker compose -f $composeFile @Args
    }
}

if ($Down) {
    Write-Host "▶️  Bajando stack de observabilidad..." -ForegroundColor Cyan
    Invoke-ComposeObs down
    Write-Host "✅  Observabilidad detenida." -ForegroundColor Green
    exit 0
}

Write-Host "▶️  Levantando observabilidad..." -ForegroundColor Cyan
Invoke-ComposeObs up -d

# URLs útiles
$urls = @(
    @{ Name = "Seq";         Url = "http://localhost:5341" },
    @{ Name = "Grafana";      Url = "http://localhost:3000" },
    @{ Name = "Prometheus";   Url = "http://localhost:9090" },
    @{ Name = "OTel Metrics"; Url = "http://localhost:9464/metrics" }
)

foreach ($u in $urls) {
    Write-Host ("🌐  {0}: {1}" -f $u.Name, $u.Url) -ForegroundColor Cyan
}

if ($Open) {
    foreach ($u in $urls) {
        try { Start-Process $u.Url } catch { }
    }
}

if ($FollowLogs) {
    Write-Host "▶️  Logs (Ctrl+C para salir)..." -ForegroundColor Cyan
    Invoke-ComposeObs logs -f
}
