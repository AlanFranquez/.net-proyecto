# 📊 Informe de Pruebas de Rendimiento - LabNet API

**Fecha de Ejecución:** [FECHA]  
**Ejecutado por:** [NOMBRE]  
**Versión de la API:** [VERSION]  
**Entorno:** [Desarrollo/Staging/Pre-producción]

---

## 📋 Resumen Ejecutivo

### Objetivo
Validar el rendimiento, latencia, throughput y estabilidad de los endpoints críticos de la API LabNet bajo diferentes condiciones de carga para garantizar que cumple con los SLOs definidos.

### Resultado General
- ✅ / ⚠️ / ❌ **[ESTADO GENERAL]**
- **Pruebas ejecutadas:** X/5
- **Pruebas aprobadas:** X
- **Pruebas con alertas:** X
- **Pruebas fallidas:** X

### Hallazgos Clave
1. [Hallazgo 1]
2. [Hallazgo 2]
3. [Hallazgo 3]

---

## 🏗️ Configuración del Entorno de Prueba

### Infraestructura
- **API:**
  - CPU: 2 cores
  - RAM: 4GB
  - .NET: 8.0
  - Contenedor: Docker

- **Base de Datos (PostgreSQL):**
  - CPU: 2 cores
  - RAM: 4GB
  - Versión: PostgreSQL 17
  - Pool de conexiones: [TAMAÑO]

- **Red:**
  - Latencia: < 5ms (local)
  - Ancho de banda: 1Gbps

### Datos de Prueba
- **Usuarios en BD:** [CANTIDAD]
- **Espacios:** [CANTIDAD]
- **Credenciales:** [CANTIDAD]
- **Registros históricos:** [CANTIDAD]

---

## 📊 Resultados por Escenario

### 1️⃣ Escenario 1: BASELINE (Carga Normal)

**Configuración:**
- VUs: 10 usuarios concurrentes
- Duración: 5 minutos
- Objetivo: Establecer métricas base

**Métricas Obtenidas:**

| Métrica | Objetivo | Obtenido | Estado |
|---------|----------|----------|--------|
| P50 (mediana) | < 100ms | [X]ms | ✅/❌ |
| P95 | < 300ms | [X]ms | ✅/❌ |
| P99 | < 500ms | [X]ms | ✅/❌ |
| Throughput | > 100 rps | [X] rps | ✅/❌ |
| Tasa de error | < 0.5% | [X]% | ✅/❌ |
| Checks exitosos | > 99% | [X]% | ✅/❌ |

**Análisis:**
[Descripción de los resultados, comportamiento observado, etc.]

**Endpoints más lentos:**
1. `POST /api/canjes` - [X]ms p95
2. `POST /api/eventos-accesos` - [X]ms p95
3. `GET /api/espacios` - [X]ms p95

---

### 2️⃣ Escenario 2: PEAK LOAD (Carga Pico)

**Configuración:**
- VUs: 100 usuarios concurrentes
- Duración: 10 minutos
- Objetivo: Simular horas pico (comedor, entrada/salida)

**Métricas Obtenidas:**

| Métrica | Objetivo | Obtenido | Estado |
|---------|----------|----------|--------|
| P50 | < 200ms | [X]ms | ✅/❌ |
| P95 | < 500ms | [X]ms | ✅/❌ |
| P99 | < 800ms | [X]ms | ✅/❌ |
| Throughput | > 500 rps | [X] rps | ✅/❌ |
| Tasa de error | < 1% | [X]% | ✅/❌ |
| Checks exitosos | > 99% | [X]% | ✅/❌ |

**Análisis:**
[Descripción del comportamiento bajo carga pico]

**Operaciones Críticas:**
- **Canjes exitosos:** [X]
- **Canjes fallidos:** [X]
- **Tasa de éxito de canjes:** [X]%

---

### 3️⃣ Escenario 3: STRESS TEST (Prueba de Estrés)

**Configuración:**
- VUs: 10 → 500 (incremental)
- Duración: 15 minutos
- Objetivo: Encontrar límites del sistema

**Métricas Obtenidas:**

| Métrica | Objetivo | Obtenido | Estado |
|---------|----------|----------|--------|
| Máximo VUs soportados | - | [X] VUs | - |
| Tasa de error (total) | < 10% | [X]% | ✅/❌ |
| Timeouts | < 5% | [X]% | ✅/❌ |
| Errores 5xx | < 1% | [X]% | ✅/❌ |
| P95 (en pico) | - | [X]ms | - |

**Análisis:**
[Descripción de cómo se comportó el sistema al aumentar la carga]

**Punto de quiebre detectado:**
- **VUs:** [X] usuarios concurrentes
- **Síntomas:** [Timeouts, errores 503, degradación de latencia, etc.]

---

### 4️⃣ Escenario 4: SOAK TEST (Prueba de Resistencia)

**Configuración:**
- VUs: 50 usuarios concurrentes
- Duración: 1 hora
- Objetivo: Detectar memory leaks y degradación

**Métricas Obtenidas:**

| Métrica | Objetivo | Obtenido | Estado |
|---------|----------|----------|--------|
| P95 (promedio) | < 300ms | [X]ms | ✅/❌ |
| Tasa de error | < 0.5% | [X]% | ✅/❌ |
| Requests lentos (>1s) | < 5% | [X]% | ✅/❌ |
| Degradación detectada | No | Sí/No | ✅/❌ |

**Análisis de Estabilidad:**
- **Latencia al inicio:** [X]ms (p95)
- **Latencia a los 30 min:** [X]ms (p95)
- **Latencia al final (60 min):** [X]ms (p95)
- **Tendencia:** [Estable / Creciente / Decreciente]

**Indicadores de Memory Leak:**
[Análisis basado en métricas de memoria, conexiones DB, etc.]

---

### 5️⃣ Escenario 5: SPIKE TEST (Prueba de Picos)

**Configuración:**
- VUs: 10 ↔ 200 (3 spikes súbitos)
- Duración: 5 minutos
- Objetivo: Validar recuperación ante picos

**Métricas Obtenidas:**

| Métrica | Objetivo | Obtenido | Estado |
|---------|----------|----------|--------|
| P95 (durante spike) | < 800ms | [X]ms | ✅/❌ |
| Tasa de error | < 2% | [X]% | ✅/❌ |
| Timeouts | < 10% | [X]% | ✅/❌ |
| Tiempo de recuperación | < 10s | [X]s | ✅/❌ |

**Análisis de Resiliencia:**
- **Spike 1:** [Comportamiento observado]
- **Spike 2:** [Comportamiento observado]
- **Spike 3:** [Comportamiento observado]

**Recuperación post-spike:**
[Descripción de cómo se recuperó el sistema]

---

## 🔍 Análisis Detallado

### Endpoints Críticos

#### POST `/api/canjes` (Canje de Acceso)
- **P95:** [X]ms
- **P99:** [X]ms
- **Throughput:** [X] rps
- **Tasa de error:** [X]%
- **Estado:** ✅/⚠️/❌
- **Observaciones:** [Descripción]

#### POST `/api/eventos-accesos` (Registro de Acceso)
- **P95:** [X]ms
- **P99:** [X]ms
- **Throughput:** [X] rps
- **Tasa de error:** [X]%
- **Estado:** ✅/⚠️/❌
- **Observaciones:** [Descripción]

#### GET `/api/espacios` (Lista de Espacios)
- **P95:** [X]ms
- **P99:** [X]ms
- **Throughput:** [X] rps
- **Tasa de error:** [X]%
- **Estado:** ✅/⚠️/❌
- **Observaciones:** [Descripción]

---

## 🎯 Cumplimiento de SLOs

### Latencia
| SLO | Objetivo | Resultado | ✅/❌ |
|-----|----------|-----------|-------|
| P50 < 100ms | 100ms | [X]ms | [Estado] |
| P95 < 300ms | 300ms | [X]ms | [Estado] |
| P99 < 500ms | 500ms | [X]ms | [Estado] |

### Disponibilidad
| SLO | Objetivo | Resultado | ✅/❌ |
|-----|----------|-----------|-------|
| Tasa de éxito | > 99.5% | [X]% | [Estado] |
| Errores 5xx | < 0.1% | [X]% | [Estado] |
| Errores 4xx | < 1% | [X]% | [Estado] |

### Throughput
| SLO | Objetivo | Resultado | ✅/❌ |
|-----|----------|-----------|-------|
| Carga normal | > 100 rps | [X] rps | [Estado] |
| Carga pico | > 500 rps | [X] rps | [Estado] |

---

## ⚠️ Issues Detectados

### Críticos 🔴
1. **[Issue 1]**
   - **Descripción:** [...]
   - **Impacto:** Alto
   - **Escenario afectado:** [...]
   - **Acción recomendada:** [...]

### Medios 🟡
1. **[Issue 1]**
   - **Descripción:** [...]
   - **Impacto:** Medio
   - **Escenario afectado:** [...]
   - **Acción recomendada:** [...]

### Bajos 🟢
1. **[Issue 1]**
   - **Descripción:** [...]
   - **Impacto:** Bajo
   - **Escenario afectado:** [...]
   - **Acción recomendada:** [...]

---

## 💡 Recomendaciones

### Inmediatas (Alta Prioridad)
1. **[Recomendación 1]**
   - Impacto esperado: [...]
   - Esfuerzo: [Alto/Medio/Bajo]

### Corto Plazo (Media Prioridad)
1. **[Recomendación 1]**
   - Impacto esperado: [...]
   - Esfuerzo: [Alto/Medio/Bajo]

### Largo Plazo (Baja Prioridad)
1. **[Recomendación 1]**
   - Impacto esperado: [...]
   - Esfuerzo: [Alto/Medio/Bajo]

---

## 📈 Comparación con Ejecuciones Anteriores

| Métrica | [Fecha Anterior] | [Fecha Actual] | Cambio |
|---------|------------------|----------------|--------|
| P95 Latencia | [X]ms | [X]ms | +/-X% |
| Throughput | [X] rps | [X] rps | +/-X% |
| Tasa de error | [X]% | [X]% | +/-X% |

**Tendencia:** [Mejora / Estable / Degradación]

---

## 📎 Anexos

### A. Configuración de k6
- Versión de k6: [VERSION]
- Scripts utilizados: `performance-tests/scenarios/`

### B. Archivos de Resultados
- Baseline: `results/baseline-[TIMESTAMP].json`
- Peak Load: `results/peak-load-[TIMESTAMP].json`
- Stress Test: `results/stress-test-[TIMESTAMP].json`
- Soak Test: `results/soak-test-[TIMESTAMP].json`
- Spike Test: `results/spike-test-[TIMESTAMP].json`

### C. Logs y Monitoreo
- Logs de Serilog/Seq: [URL]
- Métricas de Prometheus: [URL]
- Dashboards de Grafana: [URL]

### D. Screenshots
[Agregar capturas de pantalla relevantes de Grafana, k6 output, etc.]

---

## ✅ Conclusiones

[Resumen general de los resultados, cumplimiento de objetivos, estado de la API]

**Veredicto Final:** ✅ APROBADO / ⚠️ APROBADO CON OBSERVACIONES / ❌ NO APROBADO

---

**Preparado por:** [NOMBRE]  
**Revisado por:** [NOMBRE]  
**Fecha:** [FECHA]  
**Versión del documento:** 1.0
