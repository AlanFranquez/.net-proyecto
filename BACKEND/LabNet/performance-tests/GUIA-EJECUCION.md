# 🚀 Guía de Ejecución - Pruebas de Rendimiento

**Proyecto:** LabNet - Sistema de Espectáculos  
**Requisito:** 3.12 Pruebas de rendimiento o carga con herramientas automatizadas  
**Herramienta:** k6 v1.3.0  
**Fecha:** Noviembre 2025

---

## 📋 Índice

1. [Requisitos Previos](#requisitos-previos)
2. [Preparación del Entorno](#preparación-del-entorno)
3. [Ejecución de Pruebas](#ejecución-de-pruebas)
4. [Visualización de Resultados](#visualización-de-resultados)
5. [Interpretación de Métricas](#interpretación-de-métricas)
6. [Troubleshooting](#troubleshooting)

---

## 1. Requisitos Previos

### Software Necesario

| Software | Versión Mínima | Instalación |
|----------|----------------|-------------|
| **Docker Desktop** | 20.x+ | https://www.docker.com/products/docker-desktop/ |
| **k6** | 0.40+ | `winget install k6` |
| **PowerShell** | 5.1+ | Incluido en Windows |
| **Git** | 2.x+ | https://git-scm.com/ |

### Verificación de Instalación

```powershell
# Verificar Docker
docker --version
# Salida esperada: Docker version 24.x.x

# Verificar k6
k6 version
# Salida esperada: k6 v1.3.0

# Verificar PowerShell
$PSVersionTable.PSVersion
# Salida esperada: 5.1 o superior
```

---

## 2. Preparación del Entorno

### Paso 1: Clonar el Repositorio

```powershell
# Navegar al directorio de trabajo
cd E:\DOTNET

# Clonar el repositorio
git clone https://github.com/AlanFranquez/.net-proyecto.git
cd .net-proyecto\BACKEND\LabNet
```

### Paso 2: Iniciar el Stack Completo

El proyecto incluye un script automatizado que inicia todos los servicios necesarios:

```powershell
# Ejecutar script de inicio
.\start-full-stack.ps1
```

**¿Qué hace este script?**

1. ✅ Inicia PostgreSQL 17 y espera a que esté healthy
2. ✅ Inicia stack de observabilidad (Prometheus, Grafana, Seq, Tempo)
3. ✅ Inicia la API .NET 8 con migraciones y datos de prueba
4. ✅ Verifica que todos los servicios estén funcionando

**Salida esperada:**

```
🚀 Iniciando Stack Completo - LabNet Espectáculos
============================================================

[1/3] 🗄️  Iniciando PostgreSQL...
⏳ Esperando a que PostgreSQL esté lista...
✅ PostgreSQL lista

[2/3] 📊 Iniciando Stack de Observabilidad...
✅ Observabilidad iniciada

[3/3] 🌐 Iniciando API .NET...
⏳ Esperando a que la API esté lista...
✅ API lista y respondiendo

============================================================
📊 ESTADO FINAL
============================================================
[Tabla con estado de contenedores]

============================================================
🌐 URLs DE ACCESO
============================================================
API:        http://localhost:8080/health
Swagger:    http://localhost:8080/swagger
Grafana:    http://localhost:3000 (admin/admin)
Prometheus: http://localhost:9090
Seq:        http://localhost:5341

✅ Stack completo iniciado correctamente!
```

### Paso 3: Verificar que la API está Disponible

```powershell
# Verificar health endpoint
curl http://localhost:8080/health

# Salida esperada:
# Healthy
```

### Paso 4: Abrir Grafana (Opcional)

Para monitorear las métricas en tiempo real durante las pruebas:

```
URL: http://localhost:3000
Usuario: admin
Password: admin
```

Buscar dashboard: **"Espectáculos - Dashboard Técnico (3.5 Observabilidad)"**

---

## 3. Ejecución de Pruebas

### Opción A: Ejecutar Todas las Pruebas (Recomendado)

```powershell
cd performance-tests
.\run-all.ps1
```

**Duración total:** ~1 hora (todos los escenarios)

**Escenarios ejecutados:**
1. ✅ Baseline (5 min) - Línea base con 10 VUs
2. ✅ Peak Load (10 min) - Carga pico con 100 VUs
3. ✅ Stress Test (11 min) - Prueba de estrés hasta 100 VUs
4. ✅ Soak Test (33 min) - Resistencia con 20 VUs durante 30 min
5. ✅ Spike Test (4 min) - Picos repentinos de 10→100 VUs

---

### Opción B: Ejecutar Pruebas Rápidas

Para una demostración rápida (solo baseline y peak-load):

```powershell
cd performance-tests
.\run-all.ps1 -Quick
```

**Duración total:** ~15 minutos

---

### Opción C: Ejecutar Escenario Individual

Para ejecutar un solo escenario específico:

```powershell
cd performance-tests

# Escenario 1: Baseline (5 minutos)
k6 run .\scenarios\01-baseline.js

# Escenario 2: Peak Load (10 minutos)
k6 run .\scenarios\02-peak-load.js

# Escenario 3: Stress Test (11 minutos)
k6 run .\scenarios\03-stress-test.js

# Escenario 4: Soak Test (33 minutos)
k6 run .\scenarios\04-soak-test.js

# Escenario 5: Spike Test (4 minutos)
k6 run .\scenarios\05-spike-test.js
```

---

### Opción D: Prueba Rápida (1 minuto)

Para una verificación rápida sin alterar la configuración:

```powershell
cd performance-tests
k6 run --duration 60s --vus 5 .\scenarios\01-baseline.js
```

**Duración:** 1 minuto  
**Usuarios:** 5 VUs concurrentes

---

## 4. Visualización de Resultados

### 4.1 Salida en Consola (k6)

Durante la ejecución, k6 muestra métricas en tiempo real:

```
     ✓ espacios: status 200
     ✓ espacios: tiempo < 500ms
     ✓ health: status 200

     http_req_duration..............: avg=72.54ms   min=14.66ms med=53ms
     http_req_failed................: 0.00%  ✓ 0       ✗ 55
     http_reqs......................: 55     1.548/s
     vus............................: 5      min=5      max=5
```

**Métricas clave:**
- ✅ `http_req_duration` → Latencia (P50, P95, P99)
- ✅ `http_req_failed` → Tasa de errores (%)
- ✅ `http_reqs` → Throughput (req/s)
- ✅ `checks` → % de validaciones pasadas

---

### 4.2 Resumen Final (Consola)

Al finalizar, k6 muestra un resumen completo:

```
✓ http_req_duration...........: avg=245ms  min=12ms  med=198ms  max=1.2s  
                                 p(90)=412ms p(95)=523ms p(99)=892ms
✓ http_req_failed.............: 0.23%    ✓ 23 ✗ 9977
✓ http_reqs...................: 10000    (166 req/s)
✓ checks......................: 98.5%    ✓ 9850 ✗ 150
```

**Interpretación:**

| Métrica | Valor | Estado | Objetivo (SLO) |
|---------|-------|--------|----------------|
| P95 Latencia | 523ms | ⚠️ Fuera de SLO | < 300ms |
| P99 Latencia | 892ms | ❌ Fuera de SLO | < 500ms |
| Error Rate | 0.23% | ✅ Cumple | < 1% |
| Throughput | 166 req/s | ✅ Cumple | > 10 req/s |
| Checks Pass | 98.5% | ⚠️ Cerca del límite | > 99% |

---

### 4.3 Archivos de Resultados (JSON)

Los resultados se guardan automáticamente en:

```
performance-tests/
└── results/
    ├── baseline-2025-11-09_15-30-00.json
    ├── peak-load-2025-11-09_15-35-00.json
    ├── stress-2025-11-09_15-45-00.json
    ├── soak-2025-11-09_15-56-00.json
    ├── spike-2025-11-09_16-29-00.json
    └── test-suite-summary-2025-11-09_15-30-00.txt
```

**Contenido de los archivos JSON:**

- ✅ Métricas detalladas de cada request
- ✅ Timestamps de cada iteración
- ✅ Datos de checks (validaciones)
- ✅ Información de VUs activos
- ✅ Duración de cada fase

---

### 4.4 Dashboard en Grafana (Tiempo Real)

**Acceso:** http://localhost:3000

**Dashboard:** "Espectáculos - Dashboard Técnico (3.5 Observabilidad)"

**Paneles disponibles:**

| Panel | Métrica | Actualización |
|-------|---------|---------------|
| 🎯 **P95 Latencia** | Percentil 95 | Cada 10s |
| 🎯 **P99 Latencia** | Percentil 99 | Cada 10s |
| ⚡ **Tiempo Medio** | Promedio de respuesta | Cada 10s |
| ❌ **Error Rate** | % de errores 5xx | Cada 10s |
| 📈 **Latencia Detallada** | P50/P95/P99 en gráfico | Cada 10s |
| 🚦 **RPS Total** | Requests por segundo | Cada 10s |
| 📦 **Backlog Sync** | Cola de sincronizaciones | Cada 10s |

**Cómo usar Grafana durante las pruebas:**

1. Iniciar prueba de k6
2. Abrir Grafana en el navegador
3. Buscar dashboard "Observabilidad"
4. Observar métricas actualizándose en tiempo real
5. Anotar timestamps de picos o anomalías
6. Correlacionar con logs en Seq (http://localhost:5341)

---

### 4.5 Logs en Seq (Trazabilidad)

**Acceso:** http://localhost:5341

**Características:**
- ✅ Logs estructurados en JSON
- ✅ Cada request tiene un `CorrelationId` único
- ✅ Búsqueda full-text y por propiedades
- ✅ Filtrado por nivel (Debug, Info, Warning, Error)
- ✅ Visualización de contexto (request/response)

**Ejemplo de búsqueda:**

```
# Buscar logs de un CorrelationId específico
CorrelationId = "abc-123-def-456"

# Buscar errores durante las pruebas
@Level = 'Error' AND @Timestamp > now()-1h

# Buscar requests lentas
@Properties.Duration > 500
```

---

## 5. Interpretación de Métricas

### 5.1 Métricas Clave y SLOs

| Métrica | Descripción | SLO (Objetivo) | Crítico |
|---------|-------------|----------------|---------|
| **P50 (Mediana)** | 50% de requests más rápidos | < 150ms | No |
| **P95** | 95% de requests más rápidos | < 300ms | ✅ Sí |
| **P99** | 99% de requests más rápidos | < 500ms | ✅ Sí |
| **Error Rate** | % de requests fallidos | < 0.5% | ✅ Sí |
| **Throughput** | Requests por segundo | > 10 req/s | No |
| **Checks Pass** | % de validaciones exitosas | > 99% | Sí |

---

### 5.2 ¿Qué Hacer si Fallan los SLOs?

#### **Escenario 1: P95 > 300ms**

**Diagnóstico:**
- Ver en Grafana qué endpoints son lentos (panel "Latencia por Endpoint")
- Revisar logs en Seq para identificar queries SQL lentas
- Verificar concurrencia en PostgreSQL

**Acciones:**
1. Optimizar queries SQL (agregar índices)
2. Implementar caché (Redis)
3. Aumentar pool de conexiones a DB
4. Revisar `N+1` queries (eager loading)

---

#### **Escenario 2: Error Rate > 1%**

**Diagnóstico:**
- Ver códigos HTTP en panel "RPS por Código"
- Filtrar logs por nivel Error en Seq
- Revisar stack traces

**Acciones:**
1. Si 404: Endpoints no implementados o rutas incorrectas
2. Si 500: Errores de aplicación (verificar logs)
3. Si 503: Servicio saturado (aumentar recursos)
4. Si 401/403: Problemas de autenticación

---

#### **Escenario 3: Throughput < 10 req/s**

**Diagnóstico:**
- Verificar recursos del contenedor (CPU, RAM)
- Revisar si hay bottlenecks en DB
- Verificar latencia de red

**Acciones:**
1. Escalar horizontalmente (más instancias)
2. Aumentar límites de recursos en Docker
3. Optimizar queries lentas
4. Implementar load balancing

---

### 5.3 Criterios de Aprobación

Para que el proyecto **apruebe** el requisito 3.12:

| Criterio | Mínimo Aceptable | Estado |
|----------|------------------|--------|
| **Plan de prueba documentado** | 3+ escenarios | ✅ 5 escenarios |
| **Usuarios concurrentes** | 10+ VUs | ✅ 10-100 VUs |
| **Ramp-up definido** | Sí | ✅ Stages configurados |
| **Duración > 5 min** | Al menos 1 escenario | ✅ Todos > 5 min |
| **P95 definido** | Sí | ✅ < 300ms |
| **Error rate definido** | Sí | ✅ < 0.5% |
| **Entorno dockerizado** | Sí | ✅ Docker Compose |
| **Resultados exportados** | JSON o reporte | ✅ JSON + TXT |

---

## 6. Troubleshooting

### Problema 1: "API no disponible"

**Error:**
```
❌ ERROR: No se puede conectar a la API
```

**Solución:**
```powershell
# Verificar que los contenedores estén corriendo
docker ps

# Si espectaculos_web no está running:
docker-compose up -d

# Ver logs de la API
docker logs espectaculos_web --tail 50
```

---

### Problema 2: "k6 no está instalado"

**Error:**
```
❌ ERROR: k6 no está instalado
```

**Solución:**
```powershell
# Instalar k6 con winget
winget install k6

# O con Chocolatey
choco install k6

# Verificar instalación
k6 version
```

---

### Problema 3: "Grafana no muestra métricas"

**Síntomas:**
- Dashboard de Grafana vacío
- No hay datos en los paneles

**Solución:**
```powershell
# 1. Verificar que Prometheus está scrapeando
curl http://localhost:9090/api/v1/query?query=http_server_request_duration_seconds_count

# 2. Verificar que el Collector tiene métricas
curl http://localhost:9464/metrics | Select-String "http_server"

# 3. Reiniciar Grafana
cd docker
docker-compose -f docker-compose.observability.yml restart grafana

# 4. Esperar 30 segundos y refrescar el dashboard
```

---

### Problema 4: "Tests fallan con error 404"

**Error:**
```
✗ evento: status 200 o 201
  ↳  0% — ✓ 0 / ✗ 3
```

**Causa:** Endpoints no existen o tienen ruta incorrecta

**Solución:**
```powershell
# Ver endpoints disponibles
curl http://localhost:8080/swagger/v1/swagger.json | ConvertFrom-Json | Select-Object -ExpandProperty paths

# Actualizar rutas en config/endpoints.js
# Ejemplo: /api/eventos-accesos → /api/eventos
```

---

### Problema 5: "PowerShell dice 'Token ?? inesperado'"

**Error:**
```
Token '??' inesperado en la expresión
```

**Causa:** PowerShell < 7.0 no soporta operador `??`

**Solución:**
Ya está corregido en el script. Si persiste:
```powershell
# Actualizar PowerShell a 7.x
winget install Microsoft.PowerShell

# O usar el script con PowerShell 7
pwsh .\run-all.ps1
```

---

## 7. Comandos Rápidos (Cheat Sheet)

### Iniciar Todo
```powershell
cd E:\DOTNET\.net-proyecto\BACKEND\LabNet
.\start-full-stack.ps1
```

### Ejecutar Pruebas
```powershell
cd performance-tests
.\run-all.ps1 -Quick                      # 15 min
.\run-all.ps1                             # 1 hora (todas)
k6 run .\scenarios\01-baseline.js         # Solo baseline
```

### Verificar Estado
```powershell
docker ps                                  # Ver contenedores
curl http://localhost:8080/health          # API health
curl http://localhost:3000                 # Grafana
```

### Ver Logs
```powershell
docker logs espectaculos_web -f           # Logs de API
docker logs docker-grafana-1 --tail 50    # Logs de Grafana
```

### Detener Todo
```powershell
docker-compose down                        # Detener API + DB
cd docker
docker-compose -f docker-compose.observability.yml down  # Detener observabilidad
```

---

## 8. Checklist de Ejecución

Antes de ejecutar las pruebas, verificar:

- [ ] Docker Desktop está corriendo
- [ ] k6 está instalado (`k6 version`)
- [ ] Puerto 8080 está libre (API)
- [ ] Puerto 3000 está libre (Grafana)
- [ ] Puerto 5432 está libre (PostgreSQL)
- [ ] `start-full-stack.ps1` ejecutado con éxito
- [ ] `curl http://localhost:8080/health` devuelve "Healthy"
- [ ] Grafana accesible en http://localhost:3000

---

## 9. Resultados Esperados

### Escenario 1: Baseline (10 VUs, 5 min)

| Métrica | Valor Esperado | SLO |
|---------|----------------|-----|
| P95 Latencia | 150-300ms | < 300ms ✅ |
| P99 Latencia | 250-500ms | < 500ms ✅ |
| Error Rate | 0-1% | < 1% ✅ |
| Throughput | 15-30 req/s | > 10 req/s ✅ |

### Escenario 2: Peak Load (100 VUs, 10 min)

| Métrica | Valor Esperado | SLO |
|---------|----------------|-----|
| P95 Latencia | 300-500ms | < 500ms ✅ |
| P99 Latencia | 500-800ms | < 800ms ✅ |
| Error Rate | 0-2% | < 2% ✅ |
| Throughput | 80-150 req/s | > 20 req/s ✅ |

### Escenario 3: Stress Test (100 VUs, 11 min)

| Métrica | Valor Esperado | SLO |
|---------|----------------|-----|
| P95 Latencia | 500-1000ms | < 1000ms ⚠️ |
| P99 Latencia | 800-1500ms | < 1500ms ⚠️ |
| Error Rate | 1-5% | < 5% ⚠️ |
| Throughput | 50-100 req/s | > 30 req/s ✅ |

---

## 10. Contacto y Soporte

**Documentación:**
- README.md → Guía completa (35+ páginas)
- QUICKSTART.md → Inicio rápido (5 minutos)
- ANALYSIS-GUIDE.md → Interpretación de métricas

**Archivos de Configuración:**
- `config/common.js` → Thresholds y SLOs
- `config/endpoints.js` → URLs de la API
- `docker-compose.yml` → Configuración de servicios

**Scripts de Automatización:**
- `run-all.ps1` → Ejecutar todas las pruebas
- `start-full-stack.ps1` → Iniciar stack completo

---

**Autor:** Sistema de Pruebas k6 - LabNet  
**Versión:** 1.0  
**Fecha:** Noviembre 2025  
**Requisito:** 3.12 Pruebas de rendimiento o carga con herramientas automatizadas
