#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Detecta automáticamente el backend (local o AWS) para pruebas k6

.DESCRIPTION
    Script auxiliar que detecta si hay un ALB de AWS desplegado via Terraform
    o si debe usar la API local en localhost:8080

.OUTPUTS
    URL del backend detectado

.EXAMPLE
    $backend = .\detect-backend.ps1
    k6 run -e BASE_URL=$backend scenarios/01-baseline.js
#>

$ErrorActionPreference = "Stop"

function Write-ColorOutput {
    param([string]$Message, [string]$Color = "White")
    Write-Host $Message -ForegroundColor $Color
}

# 1. Si existe variable de entorno, usarla
if ($env:BASE_URL) {
    Write-ColorOutput "🎯 BASE_URL ya definida: $env:BASE_URL" "Cyan"
    return $env:BASE_URL
}

# 2. Intentar obtener de Terraform
$terraformDir = Resolve-Path "$PSScriptRoot\..\..\..\INFRA" -ErrorAction SilentlyContinue

if ($terraformDir -and (Test-Path "$terraformDir\terraform.tfstate")) {
    Write-ColorOutput "🔍 Buscando backend en Terraform state..." "Yellow"
    
    Push-Location $terraformDir
    try {
        $tfOutput = terraform output -json 2>$null | ConvertFrom-Json
        
        if ($tfOutput.alb_dns_name -and $tfOutput.alb_dns_name.value) {
            $backend = "http://$($tfOutput.alb_dns_name.value)"
            Write-ColorOutput "✅ Backend AWS detectado: $backend" "Green"
            Pop-Location
            return $backend
        }
    } catch {
        Write-ColorOutput "⚠️  Error leyendo Terraform outputs" "Yellow"
    } finally {
        Pop-Location
    }
}

# 3. Fallback a localhost
$backend = "http://localhost:8080"
Write-ColorOutput "🏠 Usando backend local: $backend" "Cyan"
return $backend
