#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Helper script para gestionar k6 en EKS

.DESCRIPTION
    Script de utilidad para gestionar k6 en EKS con soporte para streaming

    Definiciones:
        VU: Virtual User
        RPS: Requests Per Second


    # Baseline agresivo (50 VUs - 6 minutos)
    .\k6-eks-helper.ps1 -Action run -Scenario baseline

    # Peak Load brutal (500 VUs - 12 minutos)
    .\k6-eks-helper.ps1 -Action run -Scenario peak-load

    # Stress insano (hasta 1000 VUs - 17 minutos)
    .\k6-eks-helper.ps1 -Action run -Scenario stress

    # Spike extremo (hasta 2000 VUs - 5 minutos)
    .\k6-eks-helper.ps1 -Action run -Scenario spike

    # Breakpoint test (encuentra el límite - 23 minutos)
    .\k6-eks-helper.ps1 -Action run -Scenario breakpoint

    # Constant rate (5000 RPS garantizados - 5 minutos)
    .\k6-eks-helper.ps1 -Action run -Scenario constant-rate


    Objetivo            Escenario       Duración    VUs     Max Descripción
    Validar SLOs        baseline        6 min       50      Carga típica sostenida
    Capacidad máxima    peak-load       12 min      500     Pico realista
    Romper el sistema   stress          17 min      1000    Hasta que falle
    Validar elasticidad spike           5 min       2000    Spikes súbitos
    Encontrar límite    breakpoint      23 min      3000    Incremento gradual
    Validar throughput  constant-rate   5 min       Auto    5000 RPS fijos
    Memory leaks        soak            34 min      200     Larga duración

#>

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet('deploy', 'run', 'status', 'logs', 'url', 'delete', 'scenarios')]
    [string]$Action,
    
    [Parameter(Mandatory=$false)]
    [ValidateSet('baseline', 'peak-load', 'stress', 'soak', 'spike', 'breakpoint', 'constant-rate')]
    [string]$Scenario,
    
    [Parameter(Mandatory=$false)]
    [string]$Namespace = 'default'
)

$ErrorActionPreference = "Stop"

function Write-ColorOutput {
    param([string]$Message, [string]$Color = "White")
    Write-Host $Message -ForegroundColor $Color
}

Write-ColorOutput "`n k6 EKS Helper" "Cyan"
Write-ColorOutput "==================================================" "Cyan"

try {
    kubectl version --client 2>&1 | Out-Null
} catch {
    Write-ColorOutput " ERROR: kubectl no esta instalado" "Red"
    exit 1
}

function Get-K6Url {
    Write-ColorOutput "`n Obteniendo URL del LoadBalancer..." "Yellow"
    
    $maxRetries = 30
    $retryCount = 0
    
    while ($retryCount -lt $maxRetries) {
        $url = kubectl get svc k6-runner -n $Namespace -o jsonpath='{.status.loadBalancer.ingress[0].hostname}' 2>$null
        
        if ($url) {
            Write-ColorOutput " LoadBalancer disponible: http://$url" "Green"
            return "http://$url"
        }
        
        $retryCount++
        Write-ColorOutput "    Esperando LoadBalancer... ($retryCount/$maxRetries)" "Gray"
        Start-Sleep -Seconds 10
    }
    
    Write-ColorOutput " ERROR: LoadBalancer no obtuvo IP" "Red"
    return $null
}

function Test-K6Health {
    param([string]$Url)
    
    try {
        $response = Invoke-RestMethod -Uri "$Url/health" -Method GET -TimeoutSec 5
        if ($response.status -eq "healthy") {
            Write-ColorOutput " k6 API esta saludable" "Green"
            return $true
        }
    } catch {
        Write-ColorOutput "  k6 API no responde aun" "Yellow"
    }
    return $false
}

if ($Action -eq 'deploy') {
    Write-ColorOutput "`n Desplegando k6 en EKS..." "Cyan"
    
    $deployFile = Join-Path $PSScriptRoot "deployment.yaml"
    
    if (-not (Test-Path $deployFile)) {
        Write-ColorOutput " ERROR: No se encontro deployment.yaml" "Red"
        exit 1
    }
    
    Write-ColorOutput "  Aplicando configuracion..." "Yellow"
    kubectl apply -f $deployFile -n $Namespace
    
    if ($LASTEXITCODE -eq 0) {
        Write-ColorOutput " Deployment aplicado exitosamente" "Green"
        
        Write-ColorOutput "`n Esperando a que el pod este listo..." "Yellow"
        kubectl wait --for=condition=ready pod -l app=k6-runner -n $Namespace --timeout=120s
        
        $k6Url = Get-K6Url
        
        if ($k6Url) {
            Write-ColorOutput "`n k6 desplegado exitosamente!" "Green"
            Write-ColorOutput "   URL: $k6Url" "Cyan"
        }
    } else {
        Write-ColorOutput " ERROR al aplicar deployment" "Red"
        exit 1
    }
}

elseif ($Action -eq 'run') {
    if (-not $Scenario) {
        Write-ColorOutput " ERROR: Debes especificar un escenario con -Scenario" "Red"
        exit 1
    }
    
    Write-ColorOutput "`n Ejecutando escenario: $Scenario" "Cyan"
    
    $k6Url = Get-K6Url
    if (-not $k6Url) { exit 1 }
    
    if (-not (Test-K6Health -Url $k6Url)) {
        Write-ColorOutput " Esperando disponibilidad del API..." "Yellow"
        Start-Sleep -Seconds 30
        
        if (-not (Test-K6Health -Url $k6Url)) {
            Write-ColorOutput " ERROR: k6 API no responde" "Red"
            exit 1
        }
    }
    
    $scenarioMap = @{
        'baseline' = '01-baseline'
        'peak-load' = '02-peak-load'
        'stress' = '03-stress-test'
        'soak' = '04-soak-test'
        'spike' = '05-spike-test'
        'breakpoint' = '06-breakpoint-test'
        'constant-rate' = '07-constant-arrival-rate'
    }
    
    $scenarioFile = $scenarioMap[$Scenario]
    
    Write-ColorOutput "`n Iniciando test: $scenarioFile.js" "Green"
    Write-ColorOutput "  Este test puede tardar varios minutos..." "Yellow"
    Write-ColorOutput "   (Mostrando output en tiempo real)`n" "Gray"
    Write-ColorOutput "==================================================" "Cyan"
    
    try {
        $startTime = Get-Date
        
        # Usar curl para manejar Server-Sent Events
        $curlPath = Get-Command curl -ErrorAction SilentlyContinue
        
        if ($curlPath) {
            # Usar curl nativo para streaming
            $exitCode = 0
            $output = & curl.exe -N -X POST "$k6Url/run/$scenarioFile" -s -S 2>&1 | ForEach-Object {
                $line = $_.ToString()
                
                # Parsear eventos SSE
                if ($line -match '^data: (.+)$') {
                    try {
                        $data = $matches[1] | ConvertFrom-Json
                        
                        if ($data.status -eq 'started') {
                            Write-ColorOutput "  Test iniciado: $($data.scenario)" "Green"
                        }
                        elseif ($data.output) {
                            Write-Host $data.output
                        }
                        elseif ($data.heartbeat) {
                            # Heartbeat silencioso - solo actualizar timestamp
                            # Write-Host "." -NoNewline
                        }
                        elseif ($data.status -eq 'completed') {
                            if ($data.exit_code -eq 0) {
                                Write-ColorOutput "`n Test completado exitosamente" "Green"
                            } else {
                                Write-ColorOutput "`n  Test completado con errores (exit code: $($data.exit_code))" "Yellow"
                                $exitCode = $data.exit_code
                            }
                        }
                        elseif ($data.status -eq 'error') {
                            Write-ColorOutput "`n Error: $($data.error)" "Red"
                            $exitCode = 1
                        }
                    } catch {
                        # Linea no es JSON valido, ignorar
                    }
                }
            }
            
            $duration = ((Get-Date) - $startTime).TotalSeconds
            Write-ColorOutput "`n==================================================" "Cyan"
            Write-ColorOutput "  Duracion total: $([math]::Round($duration, 2)) segundos" "Cyan"
            
            if ($exitCode -ne 0) {
                exit $exitCode
            }
        }
        else {
            # Fallback: usar Invoke-WebRequest (sin streaming)
            Write-ColorOutput "  curl no disponible, usando metodo alternativo (sin streaming)" "Yellow"
            
            $response = Invoke-WebRequest `
                -Uri "$k6Url/run/$scenarioFile" `
                -Method POST `
                -TimeoutSec 7200
            
            $lines = $response.Content -split "`n"
            
            foreach ($line in $lines) {
                if ($line -match '^data: (.+)$') {
                    try {
                        $data = $matches[1] | ConvertFrom-Json
                        
                        if ($data.output) {
                            Write-Host $data.output
                        }
                        elseif ($data.status -eq 'completed') {
                            if ($data.exit_code -eq 0) {
                                Write-ColorOutput "`n Test completado" "Green"
                            } else {
                                Write-ColorOutput "`n  Exit code: $($data.exit_code)" "Yellow"
                            }
                        }
                    } catch {}
                }
            }
        }
        
    } catch {
        Write-ColorOutput "`n ERROR ejecutando test: $($_.Exception.Message)" "Red"
        
        if ($_.Exception.Message -like "*timeout*") {
            Write-ColorOutput "    El test excedio el timeout. Verifica los logs del pod." "Yellow"
            Write-ColorOutput "   kubectl logs -l app=k6-runner -f" "Gray"
        }
        
        exit 1
    }
}

elseif ($Action -eq 'status') {
    Write-ColorOutput "`n Estado del deployment k6:" "Cyan"
    Write-ColorOutput "==================================================" "Cyan"
    
    Write-ColorOutput "`n Deployment:" "Yellow"
    kubectl get deployment k6-runner -n $Namespace
    
    Write-ColorOutput "`n Pods:" "Yellow"
    kubectl get pods -l app=k6-runner -n $Namespace
    
    Write-ColorOutput "`n Service:" "Yellow"
    kubectl get svc k6-runner -n $Namespace
    
    Write-ColorOutput "`n ConfigMap:" "Yellow"
    kubectl get configmap k6-scripts -n $Namespace
    
    $k6Url = kubectl get svc k6-runner -n $Namespace -o jsonpath='{.status.loadBalancer.ingress[0].hostname}' 2>$null
    
    if ($k6Url) {
        Write-ColorOutput "`n URL Publica: http://$k6Url" "Green"
        
        if (Test-K6Health -Url "http://$k6Url") {
            Write-ColorOutput " API esta operativa" "Green"
        }
    } else {
        Write-ColorOutput "`n LoadBalancer aun no tiene IP externa" "Yellow"
    }
}

elseif ($Action -eq 'logs') {
    Write-ColorOutput "`n Logs del pod k6-runner:" "Cyan"
    Write-ColorOutput "==================================================" "Cyan"
    Write-ColorOutput "(Presiona Ctrl+C para salir)`n" "Gray"
    
    kubectl logs -l app=k6-runner -n $Namespace -f --tail=100
}

elseif ($Action -eq 'url') {
    $k6Url = Get-K6Url
    
    if ($k6Url) {
        Write-ColorOutput "`n Endpoints disponibles:" "Cyan"
        Write-ColorOutput "   Health Check:   $k6Url/health" "Gray"
        Write-ColorOutput "   Listar Tests:   $k6Url/scenarios" "Gray"
        Write-ColorOutput "   Ejecutar Test:  $k6Url/run/01-baseline (POST)" "Gray"
        
        Write-ColorOutput "`n Verificando conectividad..." "Yellow"
        
        try {
            $health = Invoke-RestMethod -Uri "$k6Url/health" -Method GET -TimeoutSec 5
            Write-ColorOutput " Status: $($health.status)" "Green"
            
            $scenarios = Invoke-RestMethod -Uri "$k6Url/scenarios" -Method GET -TimeoutSec 5
            Write-ColorOutput "`n Escenarios disponibles:" "Cyan"
            $scenarios.scenarios | ForEach-Object {
                Write-ColorOutput "   - $_" "Gray"
            }
        } catch {
            Write-ColorOutput "  No se pudo conectar al API" "Yellow"
        }
    }
}

elseif ($Action -eq 'delete') {
    Write-ColorOutput "`n  Eliminando k6 de EKS..." "Red"
    
    $confirm = Read-Host "¿Estas seguro? Esto eliminara los recursos de k6 (y/n)"
    
    if ($confirm -eq 'y' -or $confirm -eq 'Y') {
        Write-ColorOutput "Eliminando recursos..." "Yellow"
        
        kubectl delete deployment k6-runner -n $Namespace 2>$null
        kubectl delete svc k6-runner -n $Namespace 2>$null
        kubectl delete configmap k6-scripts -n $Namespace 2>$null
        kubectl delete cronjob k6-baseline-daily -n $Namespace 2>$null
        
        Write-ColorOutput " Recursos eliminados" "Green"
    } else {
        Write-ColorOutput " Cancelado" "Yellow"
    }
}

elseif ($Action -eq 'scenarios') {
    Write-ColorOutput "`n Escenarios de k6 disponibles:" "Cyan"
    Write-ColorOutput "==================================================" "Cyan"
    
    $k6Url = kubectl get svc k6-runner -n $Namespace -o jsonpath='{.status.loadBalancer.ingress[0].hostname}' 2>$null
    
    if ($k6Url) {
        try {
            $scenarios = Invoke-RestMethod -Uri "http://$k6Url/scenarios" -Method GET -TimeoutSec 5
            
            Write-ColorOutput "`n Escenarios en el cluster:" "Green"
            $scenarios.scenarios | ForEach-Object {
                Write-ColorOutput "    $_" "White"
            }
            
            Write-ColorOutput "`n Para ejecutar un escenario:" "Yellow"
            Write-ColorOutput "   .\k6-eks-helper.ps1 -Action run -Scenario baseline" "Gray"
            
        } catch {
            Write-ColorOutput " No se pudo obtener la lista de escenarios" "Red"
        }
    } else {
        Write-ColorOutput "  LoadBalancer no disponible aun" "Yellow"
        Write-ColorOutput "   Ejecuta: .\k6-eks-helper.ps1 -Action status" "Gray"
    }
}

Write-ColorOutput "`n Operacion completada`n" "Green"