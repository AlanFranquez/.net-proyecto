# Script de automatización para ejecutar todas las pruebas de rendimiento
# Uso: .\run-all.ps1 [-SkipBaseline] [-SkipPeakLoad] [-SkipStress] [-SkipSoak] [-SkipSpike]

param(
    [switch]$SkipBaseline,
    [switch]$SkipPeakLoad,
    [switch]$SkipStress,
    [switch]$SkipSoak,
    [switch]$SkipSpike,
    [switch]$Quick  # Solo ejecuta baseline y peak-load
)

$ErrorActionPreference = "Stop"

# Colores
function Write-ColorOutput {
    param([string]$Message, [string]$Color = "White")
    Write-Host $Message -ForegroundColor $Color
}

# Banner
Write-ColorOutput "`n🚀 ===============================================" "Cyan"
Write-ColorOutput "   K6 PERFORMANCE TEST SUITE - LabNet API" "Cyan"
Write-ColorOutput "===============================================`n" "Cyan"

# Verificar que k6 está instalado
try {
    $k6Version = k6 version 2>&1
    Write-ColorOutput "✅ k6 encontrado: $k6Version" "Green"
} catch {
    Write-ColorOutput "❌ ERROR: k6 no está instalado" "Red"
    Write-ColorOutput "   Instalar con: choco install k6" "Yellow"
    Write-ColorOutput "   O descargar de: https://k6.io/docs/get-started/installation/" "Yellow"
    exit 1
}

# Detectar backend automáticamente
if ($env:BASE_URL) {
    $apiUrl = $env:BASE_URL
    Write-ColorOutput "`n🎯 Usando BASE_URL de variable de entorno: $apiUrl" "Cyan"
} else {
    # Intentar obtener ALB de Terraform
    $terraformDir = Resolve-Path "$PSScriptRoot\..\..\..\INFRA"
    
    if (Test-Path "$terraformDir\terraform.tfstate") {
        Write-ColorOutput "`n🔍 Buscando backend en Terraform..." "Yellow"
        
        Push-Location $terraformDir
        try {
            $tfOutput = terraform output -json 2>$null | ConvertFrom-Json
            
            if ($tfOutput.alb_dns_name -and $tfOutput.alb_dns_name.value) {
                $apiUrl = "http://$($tfOutput.alb_dns_name.value)"
                Write-ColorOutput "✅ Backend AWS detectado: $apiUrl" "Green"
            } else {
                $apiUrl = "http://localhost:8080"
                Write-ColorOutput "ℹ️  ALB no encontrado, usando local: $apiUrl" "Gray"
            }
        } catch {
            $apiUrl = "http://localhost:8080"
            Write-ColorOutput "ℹ️  Error leyendo Terraform, usando local: $apiUrl" "Gray"
        } finally {
            Pop-Location
        }
    } else {
        $apiUrl = "http://localhost:8080"
        Write-ColorOutput "`n🎯 Usando backend local: $apiUrl" "Cyan"
    }
}

Write-ColorOutput "`n🔍 Verificando disponibilidad de la API en $apiUrl..." "Cyan"

try {
    $response = Invoke-WebRequest -Uri "$apiUrl/health" -Method GET -TimeoutSec 5 -UseBasicParsing
    if ($response.StatusCode -eq 200) {
        Write-ColorOutput "✅ API disponible y saludable" "Green"
    } else {
        Write-ColorOutput "⚠️  API responde con código: $($response.StatusCode)" "Yellow"
    }
} catch {
    Write-ColorOutput "❌ ERROR: No se puede conectar a la API" "Red"
    Write-ColorOutput "   Asegúrate de que la API esté ejecutándose en $apiUrl" "Yellow"
    Write-ColorOutput "   Ejecutar: pwsh .\scripts\dev-up.ps1 -Seed" "Yellow"
    exit 1
}

# Crear directorio de resultados si no existe
$resultsDir = Join-Path $PSScriptRoot "results"
if (-not (Test-Path $resultsDir)) {
    New-Item -ItemType Directory -Path $resultsDir | Out-Null
    Write-ColorOutput "📁 Directorio de resultados creado: $resultsDir" "Gray"
}

# Timestamp para este conjunto de pruebas
$timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
$summaryFile = Join-Path $resultsDir "test-suite-summary-$timestamp.txt"

# Inicializar resumen
$summary = @()
$summary += "=" * 60
$summary += "  K6 PERFORMANCE TEST SUITE - RESUMEN"
$summary += "  Fecha: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
$summary += "  API: $apiUrl"
$summary += "=" * 60
$summary += ""

# Contador de pruebas
$totalTests = 0
$passedTests = 0
$failedTests = 0

# Función para ejecutar un escenario
function Run-Scenario {
    param(
        [string]$Name,
        [string]$ScriptPath,
        [string]$OutputFile,
        [string]$Description
    )
    
    Write-ColorOutput "`n$('=' * 60)" "Cyan"
    Write-ColorOutput "  EJECUTANDO: $Name" "Cyan"
    Write-ColorOutput "  $Description" "Gray"
    Write-ColorOutput "$('=' * 60)" "Cyan"
    
    $script:totalTests++
    $startTime = Get-Date
    
    try {
        # Ejecutar k6
        $k6Output = k6 run --out "json=$OutputFile" $ScriptPath 2>&1
        $exitCode = $LASTEXITCODE
        
        $endTime = Get-Date
        $duration = ($endTime - $startTime).TotalSeconds
        
        # Analizar resultado
        if ($exitCode -eq 0) {
            Write-ColorOutput "`n✅ $Name COMPLETADO (${duration}s)" "Green"
            $script:passedTests++
            $script:summary += "✅ $Name - APROBADO (${duration}s)"
        } else {
            Write-ColorOutput "`n❌ $Name FALLIDO (${duration}s)" "Red"
            $script:failedTests++
            $script:summary += "❌ $Name - FALLIDO (${duration}s)"
        }
        
    } catch {
        $endTime = Get-Date
        $duration = ($endTime - $startTime).TotalSeconds
        Write-ColorOutput "`n❌ $Name ERROR: $_" "Red"
        $script:failedTests++
        $script:summary += "❌ $Name - ERROR (${duration}s)"
    }
    
    Write-ColorOutput "" "Gray"
}

# === EJECUTAR ESCENARIOS ===

# Escenario 1: Baseline
if (-not $SkipBaseline) {
    Run-Scenario `
        -Name "Escenario 1: BASELINE" `
        -ScriptPath "scenarios/01-baseline.js" `
        -OutputFile (Join-Path $resultsDir "baseline-$timestamp.json") `
        -Description "Carga normal - 10 VUs durante 5 minutos"
} else {
    Write-ColorOutput "`n⏭️  Escenario 1: BASELINE omitido" "Yellow"
}

# Escenario 2: Peak Load
if (-not $SkipPeakLoad) {
    Run-Scenario `
        -Name "Escenario 2: PEAK LOAD" `
        -ScriptPath "scenarios/02-peak-load.js" `
        -OutputFile (Join-Path $resultsDir "peak-load-$timestamp.json") `
        -Description "Carga pico - 100 VUs durante 10 minutos"
} else {
    Write-ColorOutput "`n⏭️  Escenario 2: PEAK LOAD omitido" "Yellow"
}

# Si es modo Quick, saltar el resto
if ($Quick) {
    Write-ColorOutput "`n⚡ Modo Quick activado - omitiendo pruebas largas" "Yellow"
    $SkipStress = $true
    $SkipSoak = $true
    $SkipSpike = $true
}

# Escenario 3: Stress Test
if (-not $SkipStress) {
    Run-Scenario `
        -Name "Escenario 3: STRESS TEST" `
        -ScriptPath "scenarios/03-stress-test.js" `
        -OutputFile (Join-Path $resultsDir "stress-test-$timestamp.json") `
        -Description "Prueba de estrés - 10→500 VUs durante 15 minutos"
} else {
    Write-ColorOutput "`n⏭️  Escenario 3: STRESS TEST omitido" "Yellow"
}

# Escenario 4: Soak Test
if (-not $SkipSoak) {
    Write-ColorOutput "`n⚠️  ADVERTENCIA: El Soak Test durará aproximadamente 62 minutos" "Yellow"
    $continue = Read-Host "¿Continuar? (s/N)"
    if ($continue -eq "s" -or $continue -eq "S") {
        Run-Scenario `
            -Name "Escenario 4: SOAK TEST" `
            -ScriptPath "scenarios/04-soak-test.js" `
            -OutputFile (Join-Path $resultsDir "soak-test-$timestamp.json") `
            -Description "Prueba de resistencia - 50 VUs durante 1 hora"
    } else {
        Write-ColorOutput "⏭️  Escenario 4: SOAK TEST omitido por el usuario" "Yellow"
    }
} else {
    Write-ColorOutput "`n⏭️  Escenario 4: SOAK TEST omitido" "Yellow"
}

# Escenario 5: Spike Test
if (-not $SkipSpike) {
    Run-Scenario `
        -Name "Escenario 5: SPIKE TEST" `
        -ScriptPath "scenarios/05-spike-test.js" `
        -OutputFile (Join-Path $resultsDir "spike-test-$timestamp.json") `
        -Description "Prueba de spikes - 10↔200 VUs con 3 picos súbitos"
} else {
    Write-ColorOutput "`n⏭️  Escenario 5: SPIKE TEST omitido" "Yellow"
}

# === RESUMEN FINAL ===

Write-ColorOutput "`n`n$('=' * 60)" "Cyan"
Write-ColorOutput "  RESUMEN DE EJECUCIÓN" "Cyan"
Write-ColorOutput "$('=' * 60)" "Cyan"

$summary += ""
$summary += "=" * 60
$summary += "  ESTADÍSTICAS FINALES"
$summary += "=" * 60
$summary += "Total de pruebas ejecutadas: $totalTests"
$summary += "Pruebas aprobadas: $passedTests"
$summary += "Pruebas fallidas: $failedTests"

if ($failedTests -eq 0) {
    $summary += "`n✅ TODAS LAS PRUEBAS APROBADAS"
    Write-ColorOutput "`n✅ TODAS LAS PRUEBAS APROBADAS ($passedTests/$totalTests)" "Green"
} else {
    $summary += "`n⚠️  ALGUNAS PRUEBAS FALLARON"
    Write-ColorOutput "`n⚠️  $failedTests de $totalTests pruebas FALLARON" "Yellow"
}

# Guardar resumen en archivo
$summary | Out-File -FilePath $summaryFile -Encoding UTF8
Write-ColorOutput "`n📄 Resumen guardado en: $summaryFile" "Gray"

# Mostrar resumen en consola
Write-ColorOutput "`n$($summary -join "`n")" "White"

# Mostrar ubicación de resultados detallados
Write-ColorOutput "`n📊 Resultados detallados disponibles en:" "Cyan"
Write-ColorOutput "   $resultsDir" "Gray"

Write-ColorOutput "`n🎯 PRÓXIMOS PASOS:" "Cyan"
Write-ColorOutput "   1. Revisar métricas en los archivos JSON de results/" "Gray"
Write-ColorOutput "   2. Comparar con los SLOs definidos en el README" "Gray"
Write-ColorOutput "   3. Analizar logs en Seq: http://localhost:5341" "Gray"
Write-ColorOutput "   4. Revisar métricas en Grafana: http://localhost:3000" "Gray"

Write-ColorOutput "`n✨ Pruebas de rendimiento completadas!`n" "Green"

# Código de salida
if ($failedTests -gt 0) {
    exit 1
} else {
    exit 0
}
