# 🚀 Guía Rápida de Ejecución - Pruebas de Rendimiento

## ⚡ Inicio Rápido (5 minutos)

### 1. **Instalar k6**

**Windows (PowerShell como Administrador):**
```powershell
choco install k6
```

**macOS:**
```bash
brew install k6
```

**Linux (Ubuntu/Debian):**
```bash
sudo gpg -k
sudo gpg --no-default-keyring --keyring /usr/share/keyrings/k6-archive-keyring.gpg --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys C5AD17C747E3415A3642D57D77C6C491D6AC1D69
echo "deb [signed-by=/usr/share/keyrings/k6-archive-keyring.gpg] https://dl.k6.io/deb stable main" | sudo tee /etc/apt/sources.list.d/k6.list
sudo apt-get update
sudo apt-get install k6
```

Verificar instalación:
```bash
k6 version
```

---

### 2. **Levantar el entorno de pruebas**

Desde la raíz del proyecto LabNet:

```powershell
# Windows
cd e:\DOTNET\.net-proyecto\BACKEND\LabNet
pwsh .\scripts\dev-up.ps1 -Seed
```

```bash
# Linux/macOS
cd /path/to/BACKEND/LabNet
./scripts/up.ps1 -Seed
```

Verificar que la API responde:
```bash
curl http://localhost:8080/health
```

---

### 3. **Ejecutar Pruebas**

#### 🎯 **Opción A: Pruebas Rápidas (10-15 minutos)**

```powershell
# Windows
cd performance-tests
.\run-all.ps1 -Quick
```

```bash
# Linux/macOS
cd performance-tests
chmod +x run-all.sh
./run-all.sh --quick
```

Esto ejecuta solo:
- ✅ Baseline (5 min)
- ✅ Peak Load (10 min)

---

#### 🔥 **Opción B: Prueba Individual**

**Baseline:**
```bash
k6 run scenarios/01-baseline.js
```

**Peak Load:**
```bash
k6 run scenarios/02-peak-load.js
```

**Stress Test:**
```bash
k6 run scenarios/03-stress-test.js
```

**Spike Test:**
```bash
k6 run scenarios/05-spike-test.js
```

---

#### 🏆 **Opción C: Suite Completa (⚠️ ~2 horas)**

```powershell
# Windows
.\run-all.ps1
```

```bash
# Linux/macOS
./run-all.sh
```

Incluye todos los escenarios, incluyendo el Soak Test de 1 hora.

---

### 4. **Ver Resultados**

Los resultados se guardan en:
```
performance-tests/results/
├── baseline-YYYY-MM-DD_HH-mm-ss.json
├── peak-load-YYYY-MM-DD_HH-mm-ss.json
├── stress-test-YYYY-MM-DD_HH-mm-ss.json
├── spike-test-YYYY-MM-DD_HH-mm-ss.json
├── soak-test-YYYY-MM-DD_HH-mm-ss.json
└── test-suite-summary-YYYY-MM-DD_HH-mm-ss.txt
```

**En consola, verás:**
```
✓ status is 200
✓ response time < 500ms

checks.........................: 99.80% ✓ 14970    ✗ 30
data_received..................: 4.2 MB 70 kB/s
http_req_duration..............: avg=145ms    p(95)=280ms p(99)=450ms
http_reqs......................: 15000  250/s
```

---

### 5. **Interpretar Resultados**

#### ✅ **Prueba Exitosa**
- `checks` > 99%
- `p(95)` < 300ms
- `http_req_failed` < 0.5%
- Sin errores 5xx

#### ⚠️ **Con Alertas**
- `p(95)` entre 300-500ms
- `http_req_failed` entre 0.5-1%

#### ❌ **Prueba Fallida**
- `p(95)` > 500ms
- `http_req_failed` > 1%
- Errores 5xx frecuentes

---

## 📊 Monitoreo en Tiempo Real

Mientras corren las pruebas, puedes monitorear:

1. **Logs de la API (Seq):**
   ```
   http://localhost:5341
   ```

2. **Métricas (Grafana):**
   ```
   http://localhost:3000
   Usuario: admin
   Password: admin
   ```

3. **Prometheus:**
   ```
   http://localhost:9090
   ```

---

## 🎯 Escenarios Disponibles

| Escenario | VUs | Duración | Propósito |
|-----------|-----|----------|-----------|
| **Baseline** | 10 | 5 min | Establecer métricas base |
| **Peak Load** | 100 | 10 min | Simular horas pico |
| **Stress** | 10→500 | 15 min | Encontrar límites |
| **Soak** | 50 | 1 hora | Detectar memory leaks |
| **Spike** | 10↔200 | 5 min | Validar recuperación |

---

## 🔧 Personalización

### Cambiar URL de la API:
```bash
BASE_URL=http://api.ejemplo.com:8080 k6 run scenarios/01-baseline.js
```

### Ajustar VUs o duración:
Editar directamente los archivos en `scenarios/` o usar variables de entorno.

### Exportar a formato HTML:
```bash
npm install -g k6-to-html
k6 run --out json=results.json scenarios/01-baseline.js
k6-to-html results.json > report.html
```

---

## ❓ Troubleshooting

**Error: "API no disponible"**
- Verificar que la API está ejecutándose: `curl http://localhost:8080/health`
- Verificar puertos en docker-compose.yml

**k6 no encontrado**
- Windows: Instalar con `choco install k6`
- Linux/macOS: Instalar con `brew install k6` o desde el sitio oficial

**Timeouts o errores 503**
- Aumentar recursos de Docker (CPU/RAM)
- Reducir número de VUs en los escenarios
- Verificar configuración de PostgreSQL (pool de conexiones)

---

## 📚 Recursos

- [Documentación completa](README.md)
- [k6 Documentation](https://k6.io/docs/)
- [Best Practices](https://k6.io/docs/testing-guides/api-load-testing/)

---

**¡Listo para ejecutar pruebas de carga profesionales! 🚀**
