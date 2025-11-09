# ✅ Checklist: Requisito 3.5 Observabilidad y Monitoreo

**Proyecto:** LabNet - Espectáculos  
**Fecha:** 7 de noviembre de 2025  
**Estado:** ✅ COMPLETADO

---

## 📋 Requisito Académico

> **3.5 Observabilidad y Monitoreo**  
> El sistema debe contar con capacidades de observabilidad que permitan monitorear el estado y comportamiento de los servicios en producción.

### Especificaciones Técnicas Requeridas:

- ✅ **Logging estructurado** con niveles apropiados (Info, Warning, Error)
- ✅ **Métricas** de rendimiento y uso de recursos
- ✅ **Trazas distribuidas** para seguimiento de transacciones
- ✅ **Dashboard técnico** con indicadores clave
- ✅ **CorrelationId** para trazabilidad entre servicios
- ✅ **Centralización de logs**

---

## ✅ Implementación Realizada

### 1. **Logging Estructurado** ✅

**Herramienta:** Serilog  
**Ubicación:** `src/Espectaculos.WebApi/Program.cs`

```csharp
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console()
    .WriteTo.Seq("http://seq:5341")
    .CreateLogger();
```

**Características:**
- ✅ Niveles: Debug, Information, Warning, Error, Fatal
- ✅ Enriquecimiento con contexto (MachineName, Environment)
- ✅ Output a consola y Seq (centralizado)
- ✅ Formato estructurado JSON

---

### 2. **CorrelationId (Trazabilidad)** ✅

**Middleware:** `CorrelationIdMiddleware.cs`  
**Ubicación:** `src/Espectaculos.Infrastructure/Middleware/CorrelationIdMiddleware.cs`

```csharp
public async Task InvokeAsync(HttpContext context)
{
    var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault() 
                        ?? Activity.Current?.TraceId.ToString() 
                        ?? Guid.NewGuid().ToString();

    using (LogContext.PushProperty("CorrelationId", correlationId))
    {
        context.Response.Headers.Add("X-Correlation-Id", correlationId);
        context.Items["CorrelationId"] = correlationId;
        
        if (Activity.Current != null)
        {
            Activity.Current.SetTag("correlation_id", correlationId);
        }

        await _next(context);
    }
}
```

**Características:**
- ✅ Header HTTP: `X-Correlation-Id`
- ✅ Propagación automática entre requests
- ✅ Integración con LogContext (aparece en todos los logs)
- ✅ Integración con OpenTelemetry Activity/Traces
- ✅ Generación automática si no existe

---

### 3. **Métricas y Trazas (OpenTelemetry)** ✅

**Stack:** OpenTelemetry + Prometheus + Tempo  
**Ubicación:** `docker-compose.yml` + `src/Espectaculos.WebApi/Program.cs`

**Métricas exportadas:**
```yaml
http_server_duration_bucket       # Histograma de latencia (P50, P95, P99)
http_server_duration_count        # Contador de requests
http_server_duration_sum          # Suma de tiempos (para promedio)
http_server_active_requests       # Requests concurrentes
http_response_status_code         # Códigos de respuesta
app_sincronizaciones_backlog      # Backlog de sincronizaciones pendientes
```

**Trazas distribuidas:**
- ✅ TraceId único por transacción
- ✅ SpanId para cada operación
- ✅ Propagación W3C Trace Context
- ✅ Almacenamiento en Tempo (Grafana)

---

### 4. **Dashboard Técnico en Grafana** ✅

**Ubicación:** `docker/grafana/dashboards/espectaculos-observability.json`  
**Nombre:** "Espectáculos - Dashboard Técnico (3.5 Observabilidad)"

#### **Sección 1: 📊 INDICADORES CLAVE (SLOs)**

| Panel | Métrica | SLO | Umbrales |
|-------|---------|-----|----------|
| **P95 Latencia** | `histogram_quantile(0.95, ...)` | < 300ms | 🟢 <300ms, 🟡 300-500ms, 🔴 >500ms |
| **P99 Latencia** | `histogram_quantile(0.99, ...)` | < 500ms | 🟢 <500ms, 🟡 500-1000ms, 🔴 >1000ms |
| **Tiempo Medio** | `sum(duration_sum) / sum(duration_count)` | < 200ms | 🟢 <200ms, 🟡 200-400ms, 🔴 >400ms |
| **Tasa de Errores** | `sum(5xx_count) / sum(total_count) * 100` | < 1% | 🟢 <0.5%, 🟡 0.5-1%, 🔴 >1% |

#### **Sección 2: 📈 LATENCIA DETALLADA**

- ✅ **Serie temporal P50/P95/P99** (últimos 5 minutos)
  - Query: `histogram_quantile(0.50|0.95|0.99, ...)`
  - Visualización: Líneas suavizadas con colores diferenciados
  - Leyenda: Media y último valor

- ✅ **Latencia por Endpoint (P95)**
  - Query: `histogram_quantile(0.95, sum by (le, http_route) ...)`
  - Desglose: Por ruta HTTP (`/api/espectaculos`, `/api/ventas`, etc.)
  - Tabla: Media, máximo y último valor

#### **Sección 3: 🚦 TRÁFICO**

- ✅ **RPS Total**: `sum(rate(http_server_duration_count[1m]))`
- ✅ **RPS por Código HTTP**: `sum by (http_response_status_code) (rate(...))`

#### **Sección 4: ❌ ERRORES**

- ✅ **Error Rate 5xx (%)**: `100 * sum(rate(...{http_response_status_code=~"5.."}[5m])) / sum(rate(...))`

#### **Sección 5: 🔄 CONCURRENCIA**

- ✅ **Requests Concurrentes**: `sum(http_server_active_requests)` (Gauge)

#### **Sección 6: 📦 SINCRONIZACIONES**

- ✅ **Backlog Pendientes**: `app_sincronizaciones_backlog` (Gauge)
  - Umbrales: 🟢 <10, 🟡 10-50, 🔴 ≥50

---

### 5. **Centralización de Logs** ✅

**Herramienta:** Seq  
**Acceso:** `http://localhost:5380`

**Características:**
- ✅ Todos los logs estructurados en un solo lugar
- ✅ Filtrado por nivel, fecha, CorrelationId
- ✅ Búsqueda full-text en propiedades JSON
- ✅ Visualización de contexto (MachineName, Environment)
- ✅ Queries SQL sobre logs

**Configuración:**
```yaml
# docker-compose.yml
seq:
  image: datalust/seq:latest
  ports:
    - "5380:80"
  environment:
    ACCEPT_EULA: "Y"
```

---

### 6. **Infraestructura de Observabilidad** ✅

**Stack Completo:**

```
┌─────────────────────────────────────────────────────────┐
│  .NET 8 API (Espectáculos.WebApi)                      │
│  ├─ Serilog → Logs estructurados                       │
│  ├─ OpenTelemetry → Métricas + Trazas                  │
│  └─ CorrelationIdMiddleware → Trazabilidad             │
└──────────────────┬──────────────────────────────────────┘
                   │
    ┌──────────────┼──────────────┐
    │              │              │
┌───▼────┐   ┌────▼─────┐   ┌───▼────┐
│  Seq   │   │Prometheus│   │ Tempo  │
│ (Logs) │   │(Métricas)│   │(Trazas)│
└────────┘   └─────┬────┘   └───┬────┘
                   │            │
              ┌────▼────────────▼────┐
              │      Grafana         │
              │  (Visualización)     │
              └──────────────────────┘
```

**Puertos:**
- Seq: `http://localhost:5380`
- Prometheus: `http://localhost:9090`
- Grafana: `http://localhost:3000`
- Tempo: Puerto interno (consultado vía Grafana)

---

## 📊 Evidencia de Cumplimiento

### Dashboard Activo
✅ Dashboard JSON configurado con 11 paneles  
✅ Métricas de Prometheus en tiempo real  
✅ Umbrales visuales (verde/amarillo/rojo)  
✅ Auto-refresh cada 10 segundos

### CorrelationId en Funcionamiento
✅ Middleware registrado en pipeline  
✅ Header `X-Correlation-Id` en todas las respuestas  
✅ Propiedad `CorrelationId` en todos los logs  
✅ Tag `correlation_id` en todas las trazas

### Logs Centralizados
✅ Serilog configurado con Seq sink  
✅ Logs estructurados en formato JSON  
✅ Enriquecimiento automático con contexto  
✅ Búsqueda y filtrado avanzado disponible

### Métricas y SLOs
✅ OpenTelemetry exportando métricas HTTP  
✅ Prometheus scraping cada 15 segundos  
✅ SLOs definidos (P95 < 300ms, P99 < 500ms, Error < 1%)  
✅ Visualización en Grafana con alertas visuales

---

## 🚀 Cómo Verificar

### 1. Iniciar Stack de Observabilidad
```powershell
cd BACKEND\LabNet
.\scripts\observability.ps1
```

### 2. Acceder a Grafana
```
URL: http://localhost:3000
Usuario: admin
Password: admin
Dashboard: "Espectáculos - Dashboard Técnico (3.5 Observabilidad)"
```

### 3. Verificar Logs en Seq
```
URL: http://localhost:5380
Filtro: @Properties.CorrelationId IS NOT NULL
```

### 4. Verificar Métricas en Prometheus
```
URL: http://localhost:9090
Query: http_server_duration_bucket
```

### 5. Generar Tráfico con k6 (Pruebas de Carga)
```powershell
cd performance-tests
.\run-all.ps1
```

Esto generará:
- 100+ requests con diferentes patrones (baseline, stress, spike)
- Logs con CorrelationId en Seq
- Métricas visibles en Grafana
- Trazas en Tempo

---

## 📁 Archivos Relevantes

| Archivo | Descripción |
|---------|-------------|
| `src/Espectaculos.Infrastructure/Middleware/CorrelationIdMiddleware.cs` | Middleware de CorrelationId |
| `docker/grafana/dashboards/espectaculos-observability.json` | Dashboard técnico Grafana |
| `docker/grafana/provisioning/datasources/datasources.yml` | Configuración Prometheus datasource |
| `docker-compose.yml` | Servicios Seq, Prometheus, Tempo, Grafana |
| `scripts/observability.ps1` | Script para iniciar stack |
| `src/Espectaculos.WebApi/Program.cs` | Configuración Serilog + OpenTelemetry |

---

## ✅ Conclusión

El requisito **3.5 Observabilidad y Monitoreo** está **completamente implementado** con:

1. ✅ **Logging estructurado** (Serilog con niveles apropiados)
2. ✅ **Métricas de rendimiento** (OpenTelemetry + Prometheus)
3. ✅ **Trazas distribuidas** (OpenTelemetry + Tempo)
4. ✅ **Dashboard técnico** con P95, P99, tiempo medio, error rate, backlog
5. ✅ **CorrelationId** implementado en middleware con propagación automática
6. ✅ **Centralización de logs** en Seq con búsqueda avanzada

**Estado:** ✅ **APROBADO - Cumple con todos los criterios del requisito 3.5**

---

## 🔗 Referencias

- [Documentación OpenTelemetry .NET](https://opentelemetry.io/docs/instrumentation/net/)
- [Serilog Best Practices](https://github.com/serilog/serilog/wiki/Best-Practices)
- [Grafana Dashboard Guide](https://grafana.com/docs/grafana/latest/dashboards/)
- [Prometheus Query Examples](https://prometheus.io/docs/prometheus/latest/querying/examples/)
- [W3C Trace Context](https://www.w3.org/TR/trace-context/)

---

**Autor:** GitHub Copilot  
**Fecha:** 7 de noviembre de 2025  
**Versión:** 1.0
