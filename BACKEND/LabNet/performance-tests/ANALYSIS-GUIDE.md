# 📊 Guía de Análisis de Resultados

## 🎯 Cómo Interpretar la Salida de k6

### Salida en Consola

Cuando ejecutas una prueba de k6, verás algo como esto:

```
     ✓ status is 200
     ✓ response time < 500ms

     checks.........................: 99.80% ✓ 14970    ✗ 30
     data_received..................: 4.2 MB 70 kB/s
     data_sent......................: 1.8 MB 30 kB/s
     http_req_blocked...............: avg=1.2ms    min=0s     med=1ms    max=150ms  p(95)=2ms   p(99)=5ms
     http_req_connecting............: avg=0.8ms    min=0s     med=0.7ms  max=50ms   p(95)=1.5ms p(99)=3ms
     http_req_duration..............: avg=145ms    min=50ms   med=130ms  max=800ms  p(95)=280ms p(99)=450ms
       { expected_response:true }...: avg=143ms    min=50ms   med=128ms  max=750ms  p(95)=275ms p(99)=440ms
     http_req_failed................: 0.20%  ✓ 30       ✗ 14970
     http_req_receiving.............: avg=0.5ms    min=0.1ms  med=0.4ms  max=15ms   p(95)=1ms   p(99)=2ms
     http_req_sending...............: avg=0.3ms    min=0.05ms med=0.2ms  max=10ms   p(95)=0.6ms p(99)=1.5ms
     http_req_tls_handshaking.......: avg=0ms      min=0s     med=0ms    max=0ms    p(95)=0ms   p(99)=0ms
     http_req_waiting...............: avg=144ms    min=49ms   med=129ms  max=795ms  p(95)=278ms p(99)=448ms
     http_reqs......................: 15000  250/s
     iteration_duration.............: avg=2.5s     min=1s     med=2.3s   max=5s     p(95)=3.2s  p(99)=4s
     iterations.....................: 5000   83.33/s
     vus............................: 100    min=0      max=100
     vus_max........................: 100    min=100    max=100
```

---

## 📈 Métricas Clave Explicadas

### 1. **checks** ✅
```
checks.........................: 99.80% ✓ 14970    ✗ 30
```
- **Qué es:** Porcentaje de validaciones (`check()`) que pasaron
- **Objetivo:** > 99%
- **Interpretación:**
  - ✅ 99-100%: Excelente
  - ⚠️ 95-99%: Aceptable, investigar
  - ❌ < 95%: Problemas serios

### 2. **http_req_duration** ⏱️
```
http_req_duration..............: avg=145ms    p(95)=280ms p(99)=450ms
```
- **Qué es:** Tiempo total de respuesta HTTP (sin red local)
- **Componentes:**
  - `avg`: Tiempo promedio
  - `med`: Mediana (valor del 50%)
  - `p(95)`: 95% de requests están por debajo de este tiempo
  - `p(99)`: 99% de requests están por debajo de este tiempo
  - `max`: Tiempo máximo registrado

**Análisis:**
- **P95 < 300ms:** ✅ Excelente rendimiento
- **P95 300-500ms:** ⚠️ Aceptable bajo carga
- **P95 > 500ms:** ❌ Lento, optimizar

**Por qué P95 es importante:**
- El promedio puede ser engañoso (distorsionado por outliers)
- P95 te dice "el 95% de tus usuarios experimentan esto o mejor"
- P99 es útil para detectar casos extremos

### 3. **http_req_failed** ❌
```
http_req_failed................: 0.20%  ✓ 30       ✗ 14970
```
- **Qué es:** Porcentaje de requests con status 4xx o 5xx (configurable)
- **Objetivo:** < 0.5%
- **Interpretación:**
  - ✅ < 0.5%: Sistema estable
  - ⚠️ 0.5-1%: Investigar errores
  - ❌ > 1%: Problemas graves

### 4. **http_reqs** 📊
```
http_reqs......................: 15000  250/s
```
- **Qué es:** Total de requests realizados y throughput (rps)
- **Objetivo:** 
  - Carga normal: > 100 rps
  - Carga pico: > 500 rps
- **Interpretación:** Cuánto tráfico puede manejar el sistema

### 5. **http_req_blocked** 🚧
```
http_req_blocked...............: avg=1.2ms    p(95)=2ms
```
- **Qué es:** Tiempo bloqueado antes de enviar request (DNS, TCP handshake)
- **Objetivo:** p(95) < 50ms
- **Si es alto:** Problemas de red o pool de conexiones

### 6. **http_req_connecting** 🔌
```
http_req_connecting............: avg=0.8ms    p(95)=1.5ms
```
- **Qué es:** Tiempo estableciendo conexión TCP
- **Objetivo:** p(95) < 100ms
- **Si es alto:** Pool de conexiones insuficiente

### 7. **vus** 👥
```
vus............................: 100    min=0      max=100
vus_max........................: 100    min=100    max=100
```
- **Qué es:** Virtual Users activos en cada momento
- **Interpretación:** Confirma que se alcanzó el número objetivo de usuarios

---

## 🔍 Análisis por Escenario

### Baseline (Carga Normal)
**Buscar:**
- P95 < 300ms ✅
- Tasa de error < 0.5% ✅
- Throughput estable > 100 rps ✅

**Red flags:**
- P95 > 500ms → API muy lenta
- Errores > 1% → Bugs o problemas de configuración

### Peak Load (Carga Pico)
**Buscar:**
- P95 < 500ms ⚠️ (más permisivo que baseline)
- Sistema se mantiene estable bajo presión
- Sin errores 5xx críticos

**Red flags:**
- Timeouts frecuentes
- Errores 503 (Service Unavailable)
- P99 > 2 segundos

### Stress Test (Estrés)
**Buscar:**
- Identificar punto de quiebre (VUs donde empieza a fallar)
- Degradación gradual (no abrupta)
- Sistema se recupera al reducir carga

**Red flags:**
- Crash total del sistema
- Errores 5xx > 10%
- No recuperación después del pico

### Soak Test (Resistencia)
**Buscar:**
- Latencia estable durante 1 hora
- Sin crecimiento de memoria (memory leaks)
- Tasa de error constante < 0.5%

**Red flags:**
- P95 crece con el tiempo (ej: 200ms → 500ms → 1s)
- Requests lentos aumentan progresivamente
- → Indica memory leak o leak de recursos

### Spike Test (Picos)
**Buscar:**
- Recuperación rápida después de spike (< 10s)
- Sin crashes durante picos súbitos
- Latencia vuelve a la normalidad

**Red flags:**
- Timeouts > 20% durante spike
- Sistema no se recupera
- Errores persisten después del spike

---

## 📊 Comparación con SLOs

### Tabla de Evaluación Rápida

| Métrica | Excelente ✅ | Aceptable ⚠️ | Malo ❌ |
|---------|-------------|-------------|---------|
| **P50** | < 100ms | 100-200ms | > 200ms |
| **P95** | < 300ms | 300-500ms | > 500ms |
| **P99** | < 500ms | 500-1000ms | > 1000ms |
| **Checks** | > 99.5% | 99-99.5% | < 99% |
| **Error Rate** | < 0.5% | 0.5-1% | > 1% |
| **Errores 5xx** | < 0.1% | 0.1-0.5% | > 0.5% |
| **Throughput (normal)** | > 150 rps | 100-150 rps | < 100 rps |
| **Throughput (pico)** | > 600 rps | 500-600 rps | < 500 rps |

---

## 🔧 Análisis de Problemas Comunes

### Problema: P95 muy alto (> 500ms)

**Posibles causas:**
1. **Queries de BD lentas:**
   - Revisar logs de Serilog/Seq
   - Analizar query plans en PostgreSQL
   - Agregar índices faltantes

2. **N+1 queries:**
   - Revisar uso de Entity Framework
   - Implementar `.Include()` para eager loading

3. **Sin caching:**
   - Implementar Redis para datos frecuentes
   - Cache in-memory para datos estáticos

**Acción:**
```sql
-- PostgreSQL: Encontrar queries lentas
SELECT query, mean_exec_time, calls
FROM pg_stat_statements
ORDER BY mean_exec_time DESC
LIMIT 10;
```

---

### Problema: Alta tasa de errores (> 1%)

**Posibles causas:**
1. **Timeouts de BD:**
   - Aumentar pool de conexiones
   - Optimizar queries

2. **Validaciones fallando:**
   - Revisar datos generados en `data-generators.js`
   - Ajustar modelos de prueba

3. **Rate limiting:**
   - Ajustar configuración de rate limiter
   - Aumentar capacidad

**Acción:**
- Filtrar logs por errores en Seq:
  ```
  @Level = 'Error' OR @Level = 'Fatal'
  ```

---

### Problema: Throughput bajo (< 100 rps)

**Posibles causas:**
1. **CPU saturada:**
   - Verificar en Grafana/Prometheus
   - Escalar horizontalmente

2. **Conexiones BD agotadas:**
   - Aumentar `MaxPoolSize` en connection string
   - Revisar leaks de conexiones

3. **Bloqueos (locks):**
   - Revisar transacciones largas
   - Optimizar nivel de aislamiento

**Acción:**
```csharp
// En appsettings.json, aumentar pool:
"ConnectionStrings": {
  "Default": "Host=db;Port=5432;Database=espectaculosdb;Username=postgres;Password=postgres;Maximum Pool Size=100"
}
```

---

### Problema: Memory leak detectado (Soak Test)

**Síntomas:**
- P95 crece linealmente con el tiempo
- Requests lentos (>1s) aumentan

**Posibles causas:**
1. **Conexiones no cerradas:**
   - Usar `using` statements
   - Revisar repositorios

2. **Event handlers no desregistrados:**
   - Memory leaks clásicos en .NET

3. **Caching sin límite:**
   - Implementar eviction policies

**Acción:**
```bash
# Monitorear memoria del contenedor:
docker stats espectaculos_web
```

---

## 📂 Análisis de Archivos JSON

Los resultados en `results/*.json` contienen datos detallados.

**Extraer P95 de todos los requests:**
```bash
cat results/baseline-*.json | jq '.metrics.http_req_duration.values["p(95)"]'
```

**Contar requests por endpoint:**
```bash
cat results/baseline-*.json | jq '.metrics | to_entries[] | select(.key | contains("http_req_duration{endpoint:")) | {endpoint: .key, p95: .value.values["p(95)"]}'
```

---

## 🎯 Checklist de Validación

Después de ejecutar las pruebas, verificar:

- [ ] ✅ P95 < 300ms en baseline
- [ ] ✅ P95 < 500ms en peak load
- [ ] ✅ Tasa de error < 0.5% en todos los escenarios
- [ ] ✅ Sin errores 5xx críticos
- [ ] ✅ Throughput > 100 rps (baseline)
- [ ] ✅ Throughput > 500 rps (peak)
- [ ] ✅ Sistema se recupera después de stress
- [ ] ✅ Sin degradación en soak test (1 hora)
- [ ] ✅ Recuperación < 10s después de spikes
- [ ] ✅ Checks > 99%
- [ ] ✅ Logs sin errores críticos en Seq
- [ ] ✅ Métricas estables en Grafana

---

## 📚 Recursos Adicionales

- **Documentación de k6:** https://k6.io/docs/
- **Métricas de k6:** https://k6.io/docs/using-k6/metrics/
- **Thresholds:** https://k6.io/docs/using-k6/thresholds/
- **Troubleshooting PostgreSQL:** https://www.postgresql.org/docs/current/monitoring-stats.html

---

**¡Analiza tus métricas y optimiza! 🚀**
