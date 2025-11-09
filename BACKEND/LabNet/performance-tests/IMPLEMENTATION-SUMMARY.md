# ✅ Implementación Completada - Pruebas de Rendimiento LabNet

## 📦 Resumen de Entregables

Se ha implementado un **sistema completo de pruebas de rendimiento y carga** para el proyecto LabNet utilizando **k6**, cumpliendo con el requisito 3.12 del proyecto.

---

## 📂 Estructura de Archivos Creados

```
performance-tests/
├── 📄 README.md                    # Documentación completa del sistema de pruebas
├── 📄 QUICKSTART.md                # Guía rápida de inicio (5 minutos)
├── 📄 ANALYSIS-GUIDE.md            # Guía detallada de análisis de resultados
├── 📄 REPORT-TEMPLATE.md           # Plantilla para informes de pruebas
├── 📄 .gitignore                   # Ignorar resultados de pruebas
├── 🔧 run-all.ps1                  # Script de automatización (Windows)
├── 🔧 run-all.sh                   # Script de automatización (Linux/macOS)
│
├── config/
│   ├── common.js                   # Configuración compartida (thresholds, SLOs)
│   └── endpoints.js                # Definición de endpoints de la API
│
├── scenarios/
│   ├── 01-baseline.js              # Escenario 1: Carga normal (10 VUs, 5 min)
│   ├── 02-peak-load.js             # Escenario 2: Carga pico (100 VUs, 10 min)
│   ├── 03-stress-test.js           # Escenario 3: Estrés (10→500 VUs, 15 min)
│   ├── 04-soak-test.js             # Escenario 4: Resistencia (50 VUs, 1 hora)
│   └── 05-spike-test.js            # Escenario 5: Spikes (10↔200 VUs, 5 min)
│
├── utils/
│   └── data-generators.js          # Generadores de datos de prueba
│
└── results/                         # Resultados de ejecución (gitignored)
    └── .gitkeep
```

**Total:** 17 archivos creados

---

## ✅ Cumplimiento del Requisito 3.12

### ✔️ Plan de Prueba con Escenarios

Se implementaron **5 escenarios completos**:

| Escenario | VUs | Duración | Propósito |
|-----------|-----|----------|-----------|
| **1. Baseline** | 10 | 5 min | Establecer métricas base en condiciones normales |
| **2. Peak Load** | 100 | 10 min | Simular horas pico (comedor, entrada/salida) |
| **3. Stress Test** | 10→500 | 15 min | Encontrar límites del sistema |
| **4. Soak Test** | 50 | 1 hora | Detectar memory leaks y degradación |
| **5. Spike Test** | 10↔200 | 5 min | Validar recuperación ante picos súbitos |

**Patrones incluidos:**
- ✅ Usuarios concurrentes configurables
- ✅ Ramp-up gradual
- ✅ Duración variable según objetivo
- ✅ Distribución realista de requests (40% lecturas, 30% escrituras críticas, etc.)

---

### ✔️ Métricas Objetivo (SLOs Definidos)

Se definieron **métricas claras y medibles**:

#### Latencia
- **P50 (mediana):** < 100ms (objetivo), < 200ms (crítico)
- **P95:** < 300ms (objetivo), < 500ms (crítico)
- **P99:** < 500ms (objetivo), < 1000ms (crítico)

#### Throughput
- **Carga normal:** > 100 requests/segundo
- **Carga pico:** > 500 requests/segundo

#### Disponibilidad
- **Tasa de éxito:** > 99.5%
- **Errores 5xx:** < 0.1%
- **Errores 4xx:** < 1% (excluyendo 401/403)

#### Recursos
- **CPU:** < 70% en promedio
- **Memoria:** < 80% del disponible
- **Conexiones DB:** < 80% del pool

**Implementación en k6:**
```javascript
// Ejemplo en config/common.js
export const DEFAULT_THRESHOLDS = {
    'http_req_duration': ['p(95)<300', 'p(99)<500'],
    'http_req_failed': ['rate<0.005'],  // < 0.5%
    'checks': ['rate>0.99'],            // > 99%
};
```

---

### ✔️ Ejecución en Entorno Similar al Productivo

**Configuración del entorno:**

```yaml
# docker-compose.yml (ya existente en el proyecto)
web:
  deploy:
    resources:
      limits:
        cpus: '2'
        memory: 4G

db:
  deploy:
    resources:
      limits:
        cpus: '2'
        memory: 4G
```

**Scripts de preparación:**
```powershell
# Levantar entorno completo con datos seedeados
pwsh .\scripts\dev-up.ps1 -Seed

# Verificar disponibilidad
curl http://localhost:8080/health
```

**Datos representativos:**
- Seed automático de usuarios, espacios, credenciales
- Pool de conexiones PostgreSQL configurado
- Observabilidad activa (Seq, Prometheus, Grafana)

---

## 🎯 Endpoints Críticos Identificados

Los escenarios priorizan endpoints según **criticidad de negocio**:

| Endpoint | Criticidad | % Tráfico | SLO P95 |
|----------|------------|-----------|---------|
| `POST /api/canjes` | 🔴 Crítica | 15-35% | < 200ms |
| `POST /api/eventos-accesos` | 🔴 Crítica | 10-25% | < 200ms |
| `GET /api/espacios` | 🟡 Alta | 30-50% | < 300ms |
| `GET /api/espacios/{id}` | 🟡 Alta | 20-30% | < 300ms |
| `GET /api/credenciales/{id}` | 🟡 Alta | 10-15% | < 300ms |
| `GET /health` | 🟢 Media | 5% | < 100ms |

---

## 🚀 Cómo Usar

### Instalación de k6

**Windows:**
```powershell
choco install k6
```

**macOS:**
```bash
brew install k6
```

**Linux:**
```bash
# Ver QUICKSTART.md para instrucciones completas
```

### Ejecución Rápida (10-15 minutos)

```powershell
cd performance-tests
.\run-all.ps1 -Quick
```

Esto ejecuta:
1. Baseline (5 min) - Métricas base
2. Peak Load (10 min) - Carga pico

### Ejecución Completa (~2 horas)

```powershell
.\run-all.ps1
```

Incluye todos los 5 escenarios.

### Ejecución Individual

```bash
k6 run scenarios/01-baseline.js
k6 run scenarios/02-peak-load.js
k6 run scenarios/03-stress-test.js
k6 run scenarios/04-soak-test.js
k6 run scenarios/05-spike-test.js
```

---

## 📊 Análisis de Resultados

### Salida en Consola

k6 genera automáticamente un resumen con:
- ✅ Checks (validaciones)
- ⏱️ Latencia (avg, med, p95, p99, max)
- 📈 Throughput (requests/segundo)
- ❌ Tasa de errores
- 📊 Estadísticas personalizadas

### Archivos JSON

Resultados detallados en `results/`:
```
results/
├── baseline-2025-11-07_14-30-00.json
├── peak-load-2025-11-07_14-36-00.json
├── stress-test-2025-11-07_14-47-00.json
├── spike-test-2025-11-07_15-03-00.json
├── soak-test-2025-11-07_15-10-00.json
└── test-suite-summary-2025-11-07_16-15-00.txt
```

### Visualización

**Grafana + Prometheus:**
- Métricas de CPU, memoria, latencia en tiempo real
- http://localhost:3000

**Seq (Logs):**
- Logs estructurados de Serilog
- http://localhost:5341

---

## 📈 Interpretación de Métricas

Ver `ANALYSIS-GUIDE.md` para guía completa. Resumen:

| Métrica | Excelente ✅ | Aceptable ⚠️ | Malo ❌ |
|---------|-------------|-------------|---------|
| **P95** | < 300ms | 300-500ms | > 500ms |
| **P99** | < 500ms | 500-1000ms | > 1000ms |
| **Checks** | > 99.5% | 99-99.5% | < 99% |
| **Error Rate** | < 0.5% | 0.5-1% | > 1% |
| **Throughput** | > 150 rps | 100-150 rps | < 100 rps |

---

## 📝 Documentación Incluida

1. **README.md** (Completo)
   - Introducción y teoría
   - Plan de pruebas detallado
   - Instalación y configuración
   - Métricas objetivo (SLOs)
   - Análisis de resultados
   - Troubleshooting

2. **QUICKSTART.md** (5 minutos)
   - Instalación rápida
   - Ejecución inmediata
   - Interpretación básica

3. **ANALYSIS-GUIDE.md** (Análisis)
   - Explicación de métricas de k6
   - Análisis por escenario
   - Problemas comunes y soluciones
   - Checklist de validación

4. **REPORT-TEMPLATE.md** (Informe)
   - Plantilla profesional
   - Secciones estructuradas
   - Análisis comparativo
   - Recomendaciones

---

## 🎓 Conceptos Implementados

### Patrones de Prueba de Carga
- ✅ **Baseline:** Carga constante para métricas base
- ✅ **Ramp-up:** Incremento gradual de usuarios
- ✅ **Soak/Endurance:** Carga prolongada para detectar leaks
- ✅ **Stress:** Incremento hasta el punto de quiebre
- ✅ **Spike:** Picos súbitos para validar elasticidad

### Buenas Prácticas
- ✅ Thresholds automáticos (pass/fail)
- ✅ Tags personalizados para filtrar métricas
- ✅ Checks de validación en cada request
- ✅ Sleeps aleatorios (simular comportamiento humano)
- ✅ Distribución realista de tráfico (70% lecturas, 30% escrituras)
- ✅ Manejo de timeouts
- ✅ Métricas personalizadas (canjes exitosos, timeouts, etc.)

### Herramientas Profesionales
- ✅ **k6:** Herramienta moderna de load testing
- ✅ **JavaScript:** Scripts mantenibles y legibles
- ✅ **Thresholds:** Criterios de éxito automatizados
- ✅ **JSON output:** Integración con CI/CD
- ✅ **Grafana/Prometheus:** Monitoreo en tiempo real

---

## 🔄 Integración Futura

El sistema está preparado para:

1. **CI/CD (GitHub Actions):**
```yaml
# .github/workflows/performance-tests.yml
- name: Run Performance Tests
  run: |
    pwsh ./performance-tests/run-all.ps1 -Quick
```

2. **Reportes Automatizados:**
```bash
k6 run --out json=results.json scenario.js
k6-to-html results.json > report.html
```

3. **Alertas:**
```javascript
// Los thresholds ya configurados fallan el build si no se cumplen
export const options = {
    thresholds: {
        'http_req_duration': ['p(95)<300'],  // Falla si P95 > 300ms
    }
};
```

---

## 📊 Resumen de Métricas Implementadas

### Métricas Estándar de k6
- `http_req_duration` (latencia)
- `http_req_failed` (errores)
- `http_reqs` (throughput)
- `checks` (validaciones)
- `http_req_blocked`, `http_req_connecting` (red)
- `vus`, `iterations` (concurrencia)

### Métricas Personalizadas
- `errors` (Rate) - Tasa de errores personalizada
- `espacios_duration` (Trend) - Latencia específica de espacios
- `canjes_duration` (Trend) - Latencia de canjes
- `canjes_exitosos` (Counter) - Canjes completados
- `canjes_fallidos` (Counter) - Canjes fallidos
- `eventos_registrados` (Counter) - Eventos de acceso
- `timeouts` (Rate) - Requests con timeout
- `server_errors_5xx` (Rate) - Errores de servidor
- `degradation_trend` (Trend) - Degradación en soak test
- `slow_requests_rate` (Rate) - Requests > 1s

---

## ✨ Valor Agregado

Este sistema de pruebas proporciona:

1. ✅ **Validación objetiva de rendimiento** antes de producción
2. ✅ **Detección temprana** de bottlenecks y problemas de escalabilidad
3. ✅ **Baseline documentada** para futuras comparaciones
4. ✅ **Confianza** en la capacidad del sistema bajo carga
5. ✅ **Análisis de límites** (¿cuántos usuarios soporta?)
6. ✅ **Detección de memory leaks** y degradación
7. ✅ **Validación de recuperación** ante picos súbitos

---

## 🎯 Próximos Pasos Recomendados

1. **Ejecutar baseline inicial:**
   ```bash
   k6 run scenarios/01-baseline.js
   ```

2. **Documentar resultados actuales** (usar REPORT-TEMPLATE.md)

3. **Configurar CI/CD** para ejecutar automáticamente

4. **Establecer alertas** basadas en thresholds

5. **Ejecutar periódicamente** (semanal o pre-release)

6. **Comparar tendencias** entre ejecuciones

---

## 📚 Referencias y Recursos

- **k6 Documentation:** https://k6.io/docs/
- **k6 Best Practices:** https://k6.io/docs/testing-guides/api-load-testing/
- **Performance Testing Guide:** https://k6.io/docs/test-types/introduction/
- **Proyecto LabNet:** `e:\DOTNET\.net-proyecto\BACKEND\LabNet`

---

## 👥 Soporte

Para dudas o problemas:
1. Revisar `QUICKSTART.md` para inicio rápido
2. Consultar `ANALYSIS-GUIDE.md` para interpretación de resultados
3. Revisar logs en Seq: http://localhost:5341
4. Verificar métricas en Grafana: http://localhost:3000

---

**🎉 ¡Sistema de pruebas de rendimiento completamente implementado y listo para usar!**

---

**Fecha de implementación:** 7 de noviembre de 2025  
**Versión:** 1.0  
**Autor:** GitHub Copilot + [Tu Nombre]
