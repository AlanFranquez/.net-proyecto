# 🔗 Integración k6 con Prometheus/Grafana

## 🎯 Objetivo

Integrar las pruebas de carga de k6 con el stack de observabilidad existente (Prometheus + Grafana) para visualizar métricas en tiempo real mientras se ejecutan las pruebas.

---

## 📊 Arquitectura de Integración

```
┌─────────┐      ┌──────────────┐      ┌────────────┐      ┌─────────┐
│   k6    │─────>│ Remote Write │─────>│ Prometheus │─────>│ Grafana │
│ (tests) │      │  (Endpoint)  │      │  (storage) │      │  (viz)  │
└─────────┘      └──────────────┘      └────────────┘      └─────────┘
     │                                        │
     │                                        │
     └────────────────────────────────────────┘
                API bajo prueba
```

---

## 🚀 Opción 1: Output JSON + Visualización en Grafana (Ya Implementado)

**Estado:** ✅ Ya funciona con tu setup actual

Las pruebas de k6 ya generan archivos JSON con todas las métricas. Puedes:

1. **Durante la prueba:** Ver métricas del sistema en Grafana
2. **Después:** Analizar resultados de k6 en los archivos JSON

**Ventaja:** No requiere configuración adicional

**Uso:**
```bash
# Terminal 1: Levantar observabilidad
pwsh .\scripts\observability.ps1 -Open

# Terminal 2: Levantar API
pwsh .\scripts\dev-up.ps1

# Terminal 3: Ejecutar k6
cd performance-tests
k6 run scenarios/01-baseline.js

# Terminal 4: Ver Grafana
# http://localhost:3000 (admin/admin)
```

---

## 🚀 Opción 2: k6 → Prometheus (Integración Nativa)

**Estado:** Requiere extensión `xk6-output-prometheus-remote`

### Ventajas
- Métricas de k6 directamente en Prometheus
- Dashboards de Grafana en tiempo real
- Correlación perfecta entre carga y sistema

### Instalación

```powershell
# Instalar xk6 (builder de k6 con extensiones)
go install go.k6.io/xk6/cmd/xk6@latest

# Compilar k6 con soporte Prometheus
xk6 build --with github.com/grafana/xk6-output-prometheus-remote

# Esto genera k6.exe con soporte Prometheus
```

### Configuración

**1. Habilitar Remote Write en Prometheus:**

Editar `docker/prometheus.yml`:
```yaml
global:
  scrape_interval: 15s

scrape_configs:
  - job_name: 'espectaculos-api'
    static_configs:
      - targets: ['host.docker.internal:8080']

# Agregar esto:
remote_write:
  - url: http://localhost:9090/api/v1/write
    queue_config:
      capacity: 10000
      max_shards: 50
```

**2. Ejecutar k6 con output Prometheus:**

```bash
K6_PROMETHEUS_RW_SERVER_URL=http://localhost:9090/api/v1/write \
k6 run -o experimental-prometheus-rw scenarios/01-baseline.js
```

---

## 🚀 Opción 3: k6 Cloud (Grafana Cloud - Gratis)

**Estado:** Requiere cuenta gratuita de Grafana Cloud

### Ventajas
- Sin configuración local
- Dashboards profesionales prediseñados
- Comparación histórica de pruebas
- Colaboración en equipo

### Setup

1. Crear cuenta gratuita: https://grafana.com/auth/sign-up/create-user?pg=k6
2. Obtener token de k6 Cloud
3. Ejecutar:

```bash
k6 login cloud --token YOUR_TOKEN
k6 cloud scenarios/01-baseline.js
```

**Resultado:** URL con dashboard en vivo en Grafana Cloud

---

## 🎯 Recomendación: Setup Híbrido

### Para tu caso (LabNet):

**Configuración Actual (Opción 1) - ✅ Recomendada**

```powershell
# 1. Levantar observabilidad
pwsh .\scripts\observability.ps1

# 2. Levantar API
pwsh .\scripts\dev-up.ps1 -Seed

# 3. Ejecutar k6 (genera JSON)
cd performance-tests
k6 run scenarios/01-baseline.js

# 4. Monitorear en paralelo:
# - Grafana (localhost:3000): Métricas del sistema
# - Seq (localhost:5341): Logs de la API
# - k6 console: Resultados de las pruebas
```

**Lo que ves en tiempo real:**

| Herramienta | Qué Muestra | URL |
|-------------|-------------|-----|
| **k6 (consola)** | P95, throughput, errores de la prueba | Terminal |
| **Grafana** | CPU, memoria, latencia de la API | http://localhost:3000 |
| **Seq** | Logs estructurados, errores | http://localhost:5341 |
| **Prometheus** | Métricas raw del sistema | http://localhost:9090 |

---

## 📊 Dashboard Sugerido en Grafana

Crear un dashboard que muestre:

### Panel 1: Métricas de k6 (Manual)
- Requests/segundo (calcular de Prometheus)
- Latencia P95 de la API
- Tasa de errores HTTP

### Panel 2: Métricas del Sistema
- CPU Usage (API container)
- Memory Usage
- PostgreSQL connections
- HTTP request duration

### Panel 3: Correlación
- Graph con 2 ejes Y:
  - Eje izquierdo: VUs de k6 (manual/anotación)
  - Eje derecho: Latencia P95

**Query Prometheus de ejemplo:**
```promql
# Latencia P95 de la API
histogram_quantile(0.95, 
  rate(http_request_duration_seconds_bucket[1m])
)

# Throughput
rate(http_requests_total[1m])

# CPU de contenedor API
container_cpu_usage_seconds_total{container="espectaculos_web"}
```

---

## 🎯 Workflow Completo: Pruebas con Observabilidad

### Paso a Paso

```powershell
# 1. Levantar stack completo
cd e:\DOTNET\.net-proyecto\BACKEND\LabNet
pwsh .\scripts\dev-up.ps1 -Seed

# Esperar a que todo esté listo (30-60 segundos)

# 2. Abrir Grafana en el navegador
Start-Process "http://localhost:3000"
# Login: admin/admin

# 3. En Grafana, crear dashboard con:
# - Panel de CPU (del container espectaculos_web)
# - Panel de memoria
# - Panel de HTTP duration P95

# 4. Ejecutar prueba de k6
cd performance-tests
k6 run scenarios/01-baseline.js

# 5. Mientras corre:
# - Observar Grafana: ¿Cómo responde el sistema?
# - Observar Seq: ¿Hay errores en logs?
# - Observar k6: ¿Se cumplen los thresholds?

# 6. Después:
# - Analizar JSON de k6 (results/)
# - Revisar gráficas de Grafana (exportar screenshots)
# - Revisar logs de Seq
# - Documentar en REPORT-TEMPLATE.md
```

---

## 🎓 Ventajas de Usar Ambos

### k6 Solo
❌ No ves qué pasa dentro del sistema
❌ No sabes si hay memory leaks
❌ No correlacionas con logs

### Prometheus/Grafana Solo
❌ No generas carga controlada
❌ No validas SLOs específicos
❌ No comparas rendimiento entre versiones

### k6 + Prometheus/Grafana ✅
✅ Ves carga simulada Y respuesta del sistema
✅ Detectas bottlenecks (CPU, DB, memoria)
✅ Correlacionas errores con carga
✅ Validación objetiva + observabilidad profunda

---

## 📝 Ejemplo: Análisis Completo

**Ejecutando baseline:**

```
k6 console:
  http_req_duration: p(95)=280ms ✅
  http_req_failed: 0.2% ✅

Grafana:
  CPU: 45% (normal)
  Memory: 1.2GB (estable)
  DB Connections: 15/100 (ok)
  
Seq:
  [14:30:15] INFO: Request GET /api/espacios completed in 145ms
  [14:30:16] INFO: Request GET /api/espacios completed in 132ms
  ❌ [14:30:20] ERROR: Timeout en query de reglas de acceso
```

**Conclusión:** 
- ✅ Rendimiento general bueno
- ⚠️ Hay timeouts ocasionales en reglas → Optimizar query

---

## 🎯 Resumen

### Tu Situación Actual

**Tienes:**
- ✅ Prometheus + Grafana + Seq (observabilidad)
- ✅ k6 instalado (pruebas de carga)
- ✅ Scripts automatizados

**Mejor Enfoque:**

1. **Usar k6 como está** (Output JSON) ← Más simple
2. **Monitorear en Grafana** mientras corren las pruebas
3. **Revisar Seq** para logs/errores
4. **Analizar resultados** de ambas fuentes

**Beneficio:** Visión 360° del rendimiento

---

## 🚀 Siguiente Paso Recomendado

Ejecuta una prueba completa observando todo:

```powershell
# Terminal 1
pwsh .\scripts\dev-up.ps1 -Seed

# Terminal 2 (esperar 30s)
cd performance-tests
k6 run scenarios/01-baseline.js

# Browser: http://localhost:3000 (Grafana)
# Browser: http://localhost:5341 (Seq)
```

**Observa cómo se correlacionan:**
- Picos de latencia en k6 → Picos de CPU en Grafana
- Errores en k6 → Errores en Seq
- Carga sostenida → Memoria estable o creciente

---

**📌 Conclusión:** No necesitas elegir. k6 y Prometheus/Grafana son **complementarios**. Úsalos juntos para validación completa. 🎯
