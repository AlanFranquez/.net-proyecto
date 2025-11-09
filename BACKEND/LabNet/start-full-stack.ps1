#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Inicia el stack completo: API + BD + Observabilidad

.DESCRIPTION
    Script para iniciar todos los servicios en el orden correcto:
    1. Base de datos PostgreSQL
    2. Stack de observabilidad (Prometheus, Grafana, Seq, Tempo)
    3. API .NET

.EXAMPLE
    .\start-full-stack.ps1
#>

param(
    [switch]$Build,
    [switch]$Clean
)

Write-Host "🚀 Iniciando Stack Completo - LabNet Espectáculos" -ForegroundColor Cyan
Write-Host "=" * 60 -ForegroundColor Cyan
Write-Host ""

# Detener todo si se pide clean
if ($Clean) {
    Write-Host "🧹 Limpiando contenedores existentes..." -ForegroundColor Yellow
    docker-compose down -v
    Set-Location docker
    docker-compose -f docker-compose.observability.yml down -v
    Set-Location ..
    Write-Host "✅ Limpieza completada`n" -ForegroundColor Green
}

# Paso 1: Levantar Base de Datos
Write-Host "[1/3] 🗄️  Iniciando PostgreSQL..." -ForegroundColor Yellow
if ($Build) {
    docker-compose up -d --build db
} else {
    docker-compose up -d db
}

# Esperar a que la BD esté healthy
Write-Host "⏳ Esperando a que PostgreSQL esté lista..." -ForegroundColor Gray
$maxAttempts = 30
$attempt = 0
do {
    $attempt++
    Start-Sleep -Seconds 2
    $health = docker inspect espectaculos_db --format '{{.State.Health.Status}}' 2>$null
    if ($health -eq 'healthy') {
        Write-Host "✅ PostgreSQL lista`n" -ForegroundColor Green
        break
    }
    Write-Host "   Intento $attempt/$maxAttempts..." -ForegroundColor Gray
} while ($attempt -lt $maxAttempts)

if ($health -ne 'healthy') {
    Write-Host "❌ ERROR: PostgreSQL no respondió a tiempo" -ForegroundColor Red
    exit 1
}

# Paso 2: Levantar Stack de Observabilidad
Write-Host "[2/3] 📊 Iniciando Stack de Observabilidad..." -ForegroundColor Yellow
Set-Location docker
docker-compose -f docker-compose.observability.yml up -d
Set-Location ..

Start-Sleep -Seconds 5
Write-Host "✅ Observabilidad iniciada`n" -ForegroundColor Green

# Paso 3: Levantar API
Write-Host "[3/3] 🌐 Iniciando API .NET..." -ForegroundColor Yellow
if ($Build) {
    docker-compose up -d --build web
} else {
    docker-compose up -d web
}

# Esperar a que la API responda
Write-Host "⏳ Esperando a que la API esté lista..." -ForegroundColor Gray
$maxAttempts = 30
$attempt = 0
$apiReady = $false

do {
    $attempt++
    Start-Sleep -Seconds 3
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:8080/health" -Method GET -TimeoutSec 2 -ErrorAction SilentlyContinue
        if ($response.StatusCode -eq 200) {
            $apiReady = $true
            Write-Host "✅ API lista y respondiendo`n" -ForegroundColor Green
            break
        }
    } catch {
        Write-Host "   Intento $attempt/$maxAttempts..." -ForegroundColor Gray
    }
} while ($attempt -lt $maxAttempts)

if (-not $apiReady) {
    Write-Host "⚠️  La API no respondió, verificando logs..." -ForegroundColor Yellow
    Write-Host ""
    docker logs espectaculos_web --tail 30
    Write-Host ""
}

# Mostrar estado final
Write-Host "=" * 60 -ForegroundColor Cyan
Write-Host "📊 ESTADO FINAL" -ForegroundColor Cyan
Write-Host "=" * 60 -ForegroundColor Cyan
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" | Select-String -Pattern "espectaculos|grafana|prometheus|seq|tempo|otel"

Write-Host ""
Write-Host "=" * 60 -ForegroundColor Cyan
Write-Host "🌐 URLs DE ACCESO" -ForegroundColor Cyan
Write-Host "=" * 60 -ForegroundColor Cyan
Write-Host "API:        http://localhost:8080/health" -ForegroundColor White
Write-Host "Swagger:    http://localhost:8080/swagger" -ForegroundColor White
Write-Host "Grafana:    http://localhost:3000 (admin/admin)" -ForegroundColor White
Write-Host "Prometheus: http://localhost:9090" -ForegroundColor White
Write-Host "Seq:        http://localhost:5341" -ForegroundColor White
Write-Host ""

if ($apiReady) {
    Write-Host "✅ Stack completo iniciado correctamente!" -ForegroundColor Green
    Write-Host ""
    Write-Host "💡 Para generar métricas, ejecuta pruebas de k6:" -ForegroundColor Cyan
    Write-Host "   cd performance-tests" -ForegroundColor Gray
    Write-Host "   k6 run scenarios/01-baseline.js" -ForegroundColor Gray
} else {
    Write-Host "⚠️  La API tuvo problemas al iniciar. Verifica los logs con:" -ForegroundColor Yellow
    Write-Host "   docker logs espectaculos_web -f" -ForegroundColor Gray
}

Write-Host ""
