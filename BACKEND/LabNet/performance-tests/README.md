# 🚀 Pruebas de Rendimiento - LabNet API

## 📋 Objetivo

Validar el rendimiento, latencia, throughput y estabilidad de los endpoints críticos de la API bajo condiciones de carga representativas del entorno productivo.

---

## 🛠️ Herramienta: k6

**k6** es una herramienta moderna de pruebas de carga open-source escrita en Go, con scripting en JavaScript.

### Instalación

**Windows (PowerShell):**
```powershell
# Con Chocolatey
choco install k6

# O descargar desde: https://k6.io/docs/get-started/installation/
```

**Linux/macOS:**
```bash
# Linux (Debian/Ubuntu)
sudo gpg -k
sudo gpg --no-default-keyring --keyring /usr/share/keyrings/k6-archive-keyring.gpg --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys C5AD17C747E3415A3642D57D77C6C491D6AC1D69
echo "deb [signed-by=/usr/share/keyrings/k6-archive-keyring.gpg] https://dl.k6.io/deb stable main" | sudo tee /etc/apt/sources.list.d/k6.list
sudo apt-get update
sudo apt-get install k6

# macOS
brew install k6
```

Verificar instalación:
```bash
k6 version
```

---

## 📊 Plan de Pruebas

### **Endpoints Críticos Identificados**

| Endpoint | Método | Criticidad | Uso Esperado |
|----------|--------|------------|--------------|
| `/api/espacios` | GET | Alta | Consulta frecuente por apps móviles |
| `/api/espacios/{id}` | GET | Alta | Detalle de espacio individual |
| `/api/canjes` | POST | Crítica | Validación de acceso en tiempo real |
| `/api/usuarios` | POST | Media | Registro de nuevos usuarios |
| `/api/credenciales/{id}` | GET | Alta | Validación de credenciales |
| `/api/eventos-accesos` | POST | Crítica | Registro de eventos de acceso |
| `/health` | GET | Alta | Health checks de monitoring |

---

## 🎯 Escenarios de Prueba

### **Escenario 1: Carga Normal (Baseline)**
- **Objetivo**: Establecer métricas base en condiciones normales
- **Usuarios virtuales (VUs)**: 10 usuarios concurrentes
- **Duración**: 5 minutos
- **Ramp-up**: 30 segundos
- **Distribución de requests**:
  - 40% GET `/api/espacios`
  - 30% GET `/api/espacios/{id}`
  - 20% POST `/api/canjes`
  - 10% GET `/api/credenciales/{id}`

### **Escenario 2: Carga Pico (Peak Load)**
- **Objetivo**: Simular horas pico (entrada/salida del comedor)
- **Usuarios virtuales**: 100 usuarios concurrentes
- **Duración**: 10 minutos
- **Ramp-up**: 2 minutos
- **Distribución**: Similar al escenario 1

### **Escenario 3: Prueba de Estrés (Stress Test)**
- **Objetivo**: Encontrar límites del sistema
- **Usuarios virtuales**: 10 → 500 (incremental)
- **Duración**: 15 minutos
- **Ramp-up progresivo**: 
  - 10 VUs → 50 VUs (2 min)
  - 50 VUs → 200 VUs (3 min)
  - 200 VUs → 500 VUs (5 min)
  - Mantener 500 VUs (3 min)
  - Bajar a 0 (2 min)

### **Escenario 4: Prueba de Resistencia (Soak Test)**
- **Objetivo**: Validar estabilidad a largo plazo (memory leaks, degradación)
- **Usuarios virtuales**: 50 usuarios concurrentes
- **Duración**: 1 hora
- **Ramp-up**: 2 minutos

### **Escenario 5: Prueba de Spike**
- **Objetivo**: Validar recuperación ante picos súbitos
- **Pattern**: 10 VUs → 200 VUs (inmediato) → 10 VUs
- **Duración**: 5 minutos con 3 spikes

---

## 📈 Métricas Objetivo (SLOs)

### **Latencia**
| Métrica | Objetivo | Crítico |
|---------|----------|---------|
| p50 (mediana) | < 100ms | < 200ms |
| p95 | < 300ms | < 500ms |
| p99 | < 500ms | < 1000ms |

### **Throughput**
- **Requests por segundo**: > 100 rps (carga normal)
- **Requests por segundo**: > 500 rps (carga pico)

### **Disponibilidad**
- **Tasa de éxito**: > 99.5%
- **Tasa de errores HTTP 5xx**: < 0.1%
- **Tasa de errores HTTP 4xx**: < 1% (excluir 401/403 esperados)

### **Recursos**
- **CPU**: < 70% en promedio
- **Memoria**: < 80% del disponible
- **Conexiones DB**: < 80% del pool

---

## 🏃 Ejecución de Pruebas

### **Pre-requisitos**
1. Levantar entorno de pruebas:
```powershell
cd e:\DOTNET\.net-proyecto\BACKEND\LabNet
pwsh .\scripts\dev-up.ps1 -Seed
```

2. Verificar que la API responde:
```powershell
curl http://localhost:8080/health
```

### **Ejecutar Escenarios**

**Escenario 1 - Baseline:**
```bash
k6 run --out json=results/baseline.json performance-tests/scenarios/01-baseline.js
```

**Escenario 2 - Peak Load:**
```bash
k6 run --out json=results/peak-load.json performance-tests/scenarios/02-peak-load.js
```

**Escenario 3 - Stress Test:**
```bash
k6 run --out json=results/stress-test.json performance-tests/scenarios/03-stress-test.js
```

**Escenario 4 - Soak Test:**
```bash
k6 run --out json=results/soak-test.json performance-tests/scenarios/04-soak-test.js
```

**Escenario 5 - Spike Test:**
```bash
k6 run --out json=results/spike-test.json performance-tests/scenarios/05-spike-test.js
```

### **Ejecutar Todas las Pruebas (Automatizado)**
```powershell
# Windows
pwsh .\performance-tests\run-all.ps1

# Linux/macOS
./performance-tests/run-all.sh
```

---

## 📊 Análisis de Resultados

### **Salida de k6 en Consola**
k6 genera un resumen automático al finalizar:
```
     ✓ status is 200
     ✓ response time < 500ms

     checks.........................: 99.80% ✓ 14970    ✗ 30
     data_received..................: 4.2 MB 70 kB/s
     data_sent......................: 1.8 MB 30 kB/s
     http_req_blocked...............: avg=1.2ms    min=0s     med=1ms    max=150ms  p(95)=2ms   p(99)=5ms
     http_req_duration..............: avg=145ms    min=50ms   med=130ms  max=800ms  p(95)=280ms p(99)=450ms
     http_reqs......................: 15000  250/s
     iteration_duration.............: avg=2.5s     min=1s     med=2.3s   max=5s     p(95)=3.2s  p(99)=4s
     iterations.....................: 5000   83.33/s
     vus............................: 100    min=0      max=100
     vus_max........................: 100    min=100    max=100
```

### **Análisis con Grafana k6**
Para visualización avanzada, exportar a InfluxDB o Prometheus:
```bash
k6 run --out influxdb=http://localhost:8086/k6 scenario.js
```

### **HTML Report (k6-reporter)**
```bash
npm install -g k6-to-junit
k6 run --out json=results.json scenario.js
k6-to-junit results.json > results.xml
```

---

## 🔍 Criterios de Aceptación

### ✅ **Prueba Exitosa**
- Todos los thresholds definidos en verde
- p95 de latencia < 300ms
- Tasa de errores < 0.5%
- Sin errores 5xx (excepto casos de límite de recursos esperados)

### ⚠️ **Prueba con Alertas**
- p95 entre 300-500ms
- Tasa de errores entre 0.5-1%
- CPU > 70%

### ❌ **Prueba Fallida**
- p95 > 500ms
- Tasa de errores > 1%
- Errores 5xx > 0.1%
- Timeouts > 5%

---

## 🏗️ Entorno de Prueba

### **Especificaciones Recomendadas**
- **Similar a producción** (o escalado proporcional)
- **Base de datos**: PostgreSQL con datos representativos (mínimo 10,000 usuarios, 50 espacios)
- **Red**: Latencia simulada si difiere de producción
- **Recursos**: 
  - API: 2 CPU cores, 4GB RAM (mínimo)
  - DB: 2 CPU cores, 4GB RAM (mínimo)

### **Configuración Actual (Docker)**
```yaml
# docker-compose.yml
web:
  deploy:
    resources:
      limits:
        cpus: '2'
        memory: 4G
```

---

## 📁 Estructura de Archivos

```
performance-tests/
├── README.md                    # Este archivo
├── config/
│   ├── common.js               # Configuración compartida
│   └── endpoints.js            # Definición de endpoints
├── scenarios/
│   ├── 01-baseline.js          # Carga normal
│   ├── 02-peak-load.js         # Carga pico
│   ├── 03-stress-test.js       # Prueba de estrés
│   ├── 04-soak-test.js         # Prueba de resistencia
│   └── 05-spike-test.js        # Prueba de spike
├── utils/
│   ├── auth.js                 # Helpers de autenticación
│   └── data-generators.js      # Generadores de datos
├── results/                     # Resultados de ejecución (gitignored)
├── run-all.ps1                 # Script para ejecutar todas las pruebas
└── run-all.sh                  # Script para Linux/macOS
```

---

## � Integración con Observabilidad

LabNet ya cuenta con un stack completo de observabilidad:
- **Prometheus** (http://localhost:9090) - Métricas del sistema
- **Grafana** (http://localhost:3000) - Visualización de métricas
- **Seq** (http://localhost:5341) - Logs estructurados
- **Tempo** - Distributed tracing

**Recomendación:** Ejecutar k6 mientras monitoras en Grafana y Seq para obtener visión 360°.

Ver [INTEGRATION-WITH-OBSERVABILITY.md](INTEGRATION-WITH-OBSERVABILITY.md) para setup completo.

---

## �📚 Referencias

- [k6 Documentation](https://k6.io/docs/)
- [k6 Thresholds](https://k6.io/docs/using-k6/thresholds/)
- [k6 Scenarios](https://k6.io/docs/using-k6/scenarios/)
- [Best Practices](https://k6.io/docs/testing-guides/api-load-testing/)
- [Integración con Observabilidad](INTEGRATION-WITH-OBSERVABILITY.md)

---

## 🎯 Próximos Pasos

1. ✅ Instalar k6
2. ✅ Configurar entorno de pruebas
3. ✅ Ejecutar escenario baseline
4. ✅ Analizar resultados y ajustar thresholds
5. ✅ Ejecutar todos los escenarios
6. ✅ Documentar hallazgos y optimizaciones
7. ✅ Integrar en CI/CD (opcional)

---

**Autor**: LabNet Team  
**Última actualización**: Noviembre 2025
