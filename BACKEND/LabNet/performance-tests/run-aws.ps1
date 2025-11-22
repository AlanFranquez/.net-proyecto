#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Ejecuta pruebas de rendimiento k6 contra el backend desplegado en AWS

.DESCRIPTION
    Script para ejecutar pruebas de carga contra la infraestructura de AWS.
    Obtiene automáticamente el DNS del ALB desde los outputs de Terraform.

.PARAMETER Scenario
    Escenario específico a ejecutar (baseline, peak-load, stress, soak, spike)
    Si no se especifica, ejecuta todos los escenarios.

.PARAMETER AlbDns
    DNS del Application Load Balancer de AWS. 
    Si no se proporciona, intenta obtenerlo de Terraform outputs.

.PARAMETER Quick
    Ejecuta solo los escenarios rápidos (baseline y peak-load)

.EXAMPLE
    .\run-aws.ps1
    Ejecuta todos los escenarios contra el ALB obtenido de Terraform

.EXAMPLE
    .\run-aws.ps1 -Scenario baseline
    Ejecuta solo el escenario baseline

.EXAMPLE
    .\run-aws.ps1 -AlbDns "mi-alb-123.us-east-1.elb.amazonaws.com"
    Ejecuta contra un ALB específico

.EXAMPLE
    .\run-aws.ps1 -Quick
    Ejecuta solo baseline y peak-load (15 minutos aprox)
#>

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet('baseline', 'peak-load', 'stress', 'soak', 'spike', 'all')]
    [string]$Scenario = 'all',
    
    [Parameter(Mandatory=$false)]
    [string]$AlbDns,
    
    [Parameter(Mandatory=$false)]
    [switch]$Quick
)

$ErrorActionPreference = "Stop"

# Colores
function Write-ColorOutput {
    param([string]$Message, [string]$Color = "White")
    Write-Host $Message -ForegroundColor $Color
}

Write-ColorOutput "`n🚀 Pruebas de Rendimiento k6 - AWS Backend" "Cyan"
Write-ColorOutput "============================================================" "Cyan"

# 1. Verificar k6
Write-ColorOutput "`n[1/4] 🔍 Verificando k6..." "Yellow"
try {
    $k6Version = k6 version 2>$null
    Write-ColorOutput "✅ k6 encontrado: $k6Version" "Green"
} catch {
    Write-ColorOutput "❌ ERROR: k6 no está instalado" "Red"
    Write-ColorOutput "Instalar con: winget install k6" "Yellow"
    exit 1
}

# 2. Obtener DNS del ALB
Write-ColorOutput "`n[2/4] 🌐 Obteniendo DNS del ALB..." "Yellow"

if ($AlbDns) {
    $albDnsName = $AlbDns
    Write-ColorOutput "✅ Usando ALB proporcionado: $albDnsName" "Green"
} else {
    # Intentar obtener de Terraform outputs
    $terraformDir = Resolve-Path "$PSScriptRoot\..\..\..\INFRA"
    
    if (Test-Path "$terraformDir\terraform.tfstate") {
        Write-ColorOutput "📂 Leyendo terraform.tfstate..." "Gray"
        
        Push-Location $terraformDir
        try {
            $tfOutput = terraform output -json 2>$null | ConvertFrom-Json
            
            if ($tfOutput.alb_dns_name) {
                $albDnsName = $tfOutput.alb_dns_name.value
                Write-ColorOutput "✅ ALB DNS obtenido de Terraform: $albDnsName" "Green"
            } else {
                Write-ColorOutput "⚠️  No se encontró 'alb_dns_name' en outputs de Terraform" "Yellow"
                $albDnsName = Read-Host "Ingresa el DNS del ALB manualmente"
            }
        } catch {
            Write-ColorOutput "⚠️  Error leyendo Terraform outputs: $_" "Yellow"
            $albDnsName = Read-Host "Ingresa el DNS del ALB manualmente"
        } finally {
            Pop-Location
        }
    } else {
        Write-ColorOutput "⚠️  No se encontró terraform.tfstate" "Yellow"
        $albDnsName = Read-Host "Ingresa el DNS del ALB manualmente"
    }
}

# Validar que no esté vacío
if ([string]::IsNullOrWhiteSpace($albDnsName)) {
    Write-ColorOutput "❌ ERROR: DNS del ALB no puede estar vacío" "Red"
    exit 1
}

# Construir URL base
$baseUrl = "http://$albDnsName"
Write-ColorOutput "🎯 URL objetivo: $baseUrl" "Cyan"

# 3. Verificar conectividad con el ALB
Write-ColorOutput "`n[3/4] 🏥 Verificando conectividad con AWS..." "Yellow"
try {
    $healthCheck = Invoke-WebRequest -Uri "$baseUrl/health" -TimeoutSec 10 -UseBasicParsing -ErrorAction Stop
    
    if ($healthCheck.StatusCode -eq 200) {
        Write-ColorOutput "✅ API AWS disponible (Status: 200)" "Green"
        Write-ColorOutput "   Response: $($healthCheck.Content)" "Gray"
    } else {
        Write-ColorOutput "⚠️  API respondió con status: $($healthCheck.StatusCode)" "Yellow"
    }
} catch {
    Write-ColorOutput "❌ ERROR: No se puede conectar a $baseUrl/health" "Red"
    Write-ColorOutput "   Detalles: $($_.Exception.Message)" "Red"
    
    $continue = Read-Host "¿Continuar de todos modos? (y/n)"
    if ($continue -ne 'y') {
        exit 1
    }
}

# 4. Ejecutar escenarios
Write-ColorOutput "`n[4/4] 🧪 Ejecutando escenarios de prueba..." "Yellow"
Write-ColorOutput "============================================================" "Yellow"

# Definir escenarios según parámetros
$scenariosToRun = @()

if ($Quick) {
    $scenariosToRun = @(
        @{ Name = "01-baseline"; DisplayName = "Baseline"; Duration = "5 min" },
        @{ Name = "02-peak-load"; DisplayName = "Peak Load"; Duration = "10 min" }
    )
    Write-ColorOutput "⚡ Modo QUICK: Ejecutando solo Baseline y Peak Load (~15 min)" "Cyan"
} elseif ($Scenario -eq 'all') {
    $scenariosToRun = @(
        @{ Name = "01-baseline"; DisplayName = "Baseline"; Duration = "5 min" },
        @{ Name = "02-peak-load"; DisplayName = "Peak Load"; Duration = "10 min" },
        @{ Name = "03-stress-test"; DisplayName = "Stress Test"; Duration = "11 min" },
        @{ Name = "04-soak-test"; DisplayName = "Soak Test"; Duration = "33 min" },
        @{ Name = "05-spike-test"; DisplayName = "Spike Test"; Duration = "4 min" }
    )
    Write-ColorOutput "📊 Ejecutando TODOS los escenarios (~1 hora)" "Cyan"
} else {
    # Mapeo de nombres a archivos
    $scenarioMap = @{
        'baseline' = @{ Name = "01-baseline"; DisplayName = "Baseline"; Duration = "5 min" }
        'peak-load' = @{ Name = "02-peak-load"; DisplayName = "Peak Load"; Duration = "10 min" }
        'stress' = @{ Name = "03-stress-test"; DisplayName = "Stress Test"; Duration = "11 min" }
        'soak' = @{ Name = "04-soak-test"; DisplayName = "Soak Test"; Duration = "33 min" }
        'spike' = @{ Name = "05-spike-test"; DisplayName = "Spike Test"; Duration = "4 min" }
    }
    
    if ($scenarioMap.ContainsKey($Scenario)) {
        $scenariosToRun = @($scenarioMap[$Scenario])
        Write-ColorOutput "🎯 Ejecutando escenario: $($scenarioMap[$Scenario].DisplayName)" "Cyan"
    } else {
        Write-ColorOutput "❌ ERROR: Escenario desconocido: $Scenario" "Red"
        exit 1
    }
}

Write-ColorOutput ""

# Ejecutar cada escenario
$results = @()
$startTime = Get-Date

foreach ($s in $scenariosToRun) {
    $scenarioFile = ".\scenarios\$($s.Name).js"
    
    Write-ColorOutput "`n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" "Cyan"
    Write-ColorOutput "📋 Escenario: $($s.DisplayName) ($($s.Duration))" "Cyan"
    Write-ColorOutput "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" "Cyan"
    Write-ColorOutput "⏱️  Inicio: $(Get-Date -Format 'HH:mm:ss')" "Gray"
    Write-ColorOutput ""
    
    # Ejecutar k6 con BASE_URL apuntando al ALB
    $env:BASE_URL = $baseUrl
    
    try {
        k6 run $scenarioFile
        $exitCode = $LASTEXITCODE
        
        if ($exitCode -eq 0) {
            Write-ColorOutput "`n✅ $($s.DisplayName) completado exitosamente" "Green"
            $results += @{ Scenario = $s.DisplayName; Status = "✅ PASS"; ExitCode = 0 }
        } else {
            Write-ColorOutput "`n⚠️  $($s.DisplayName) completado con warnings (exit code: $exitCode)" "Yellow"
            $results += @{ Scenario = $s.DisplayName; Status = "⚠️  FAIL"; ExitCode = $exitCode }
        }
    } catch {
        Write-ColorOutput "`n❌ ERROR ejecutando $($s.DisplayName): $_" "Red"
        $results += @{ Scenario = $s.DisplayName; Status = "❌ ERROR"; ExitCode = -1 }
    }
    
    Write-ColorOutput "⏱️  Fin: $(Get-Date -Format 'HH:mm:ss')" "Gray"
    
    # Pausa entre escenarios (excepto el último)
    if ($s -ne $scenariosToRun[-1]) {
        Write-ColorOutput "`n⏸️  Esperando 30 segundos antes del siguiente escenario..." "Gray"
        Start-Sleep -Seconds 30
    }
}

# Resumen final
$endTime = Get-Date
$duration = $endTime - $startTime

Write-ColorOutput "`n`n" "White"
Write-ColorOutput "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" "Green"
Write-ColorOutput "📊 RESUMEN DE PRUEBAS - AWS" "Green"
Write-ColorOutput "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" "Green"
Write-ColorOutput ""
Write-ColorOutput "🎯 Entorno:      AWS ALB" "Cyan"
Write-ColorOutput "🌐 URL:          $baseUrl" "Cyan"
Write-ColorOutput "⏱️  Duración:     $($duration.ToString('hh\:mm\:ss'))" "Cyan"
Write-ColorOutput "📅 Fecha:        $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" "Cyan"
Write-ColorOutput ""
Write-ColorOutput "📋 Resultados:" "Yellow"
Write-ColorOutput "============================================================" "Yellow"

foreach ($result in $results) {
    Write-ColorOutput "   $($result.Status)  $($result.Scenario)" "White"
}

Write-ColorOutput ""
Write-ColorOutput "📂 Resultados guardados en: .\results\" "Gray"
Write-ColorOutput ""

# Exit code: 0 si todos pasaron, 1 si hubo algún fallo
$failedCount = ($results | Where-Object { $_.ExitCode -ne 0 }).Count
if ($failedCount -eq 0) {
    Write-ColorOutput "✅ Todas las pruebas completadas exitosamente" "Green"
    exit 0
} else {
    Write-ColorOutput "⚠️  $failedCount escenario(s) fallaron" "Yellow"
    exit 1
}
