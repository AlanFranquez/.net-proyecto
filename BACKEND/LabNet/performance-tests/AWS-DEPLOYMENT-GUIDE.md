# 🌐 Guía de Pruebas de Rendimiento en AWS

## 📋 Índice

1. [Requisitos Previos](#requisitos-previos)
2. [Configuración Inicial](#configuración-inicial)
3. [Ejecución de Pruebas](#ejecución-de-pruebas)
4. [Monitoreo y Observabilidad](#monitoreo-y-observabilidad)
5. [Troubleshooting](#troubleshooting)
6. [Mejores Prácticas](#mejores-prácticas)

---

## 1. Requisitos Previos

### Software Necesario

- ✅ **k6** v0.40+ instalado localmente
- ✅ **Terraform** configurado (para obtener outputs)
- ✅ **AWS CLI** configurado (opcional, para verificar recursos)
- ✅ **PowerShell** 5.1+ o PowerShell Core 7+

### Infraestructura AWS Desplegada

Asegúrate de que tu infraestructura esté completamente desplegada:

```powershell
# Verificar estado de Terraform
cd E:\DOTNET\.net-proyecto\INFRA
terraform plan

# Ver outputs (incluye ALB DNS)
terraform output
```

**Outputs esperados:**
```
alb_dns_name = "mi-alb-123456.us-east-1.elb.amazonaws.com"
rds_endpoint = "postgres.xyz.us-east-1.rds.amazonaws.com:5432"
user_pool_id = "us-east-1_XXXXXXXXX"
```

---

## 2. Configuración Inicial

### Opción 1: Script Automático (Recomendado)

El script `run-aws.ps1` obtiene automáticamente el DNS del ALB:

```powershell
cd E:\DOTNET\.net-proyecto\BACKEND\LabNet\performance-tests

# Ejecutar con detección automática
.\run-aws.ps1

# O especificar ALB manualmente
.\run-aws.ps1 -AlbDns "mi-alb-123456.us-east-1.elb.amazonaws.com"
```

### Opción 2: Variables de Entorno Manuales

Configura las variables antes de ejecutar k6:

```powershell
# PowerShell
$env:BASE_URL = "http://mi-alb-123456.us-east-1.elb.amazonaws.com"
k6 run .\scenarios\01-baseline.js

# O en una sola línea
$env:BASE_URL="http://mi-alb-123.us-east-1.elb.amazonaws.com"; k6 run .\scenarios\01-baseline.js
```

### Opción 3: Archivo .env

1. Copiar `.env.example` a `.env`:
   ```powershell
   Copy-Item .env.example .env
   ```

2. Editar `.env`:
   ```bash
   BASE_URL=http://mi-alb-123456.us-east-1.elb.amazonaws.com
   ```

3. Ejecutar k6 (cargará automáticamente el .env):
   ```powershell
   k6 run .\scenarios\01-baseline.js
   ```

---

## 3. Ejecución de Pruebas

### 3.1 Verificar Conectividad

Antes de ejecutar pruebas, verifica que el ALB esté accesible:

```powershell
# PowerShell
$albDns = "mi-alb-123456.us-east-1.elb.amazonaws.com"
curl "http://$albDns/health"

# Debería retornar: "Healthy"
```

### 3.2 Ejecutar Todos los Escenarios

```powershell
.\run-aws.ps1
```

**Duración total:** ~1 hora  
**Escenarios:** Baseline → Peak Load → Stress → Soak → Spike

### 3.3 Ejecutar Escenarios Rápidos

Para pruebas rápidas (ideal para CI/CD):

```powershell
.\run-aws.ps1 -Quick
```

**Duración total:** ~15 minutos  
**Escenarios:** Baseline + Peak Load

### 3.4 Ejecutar Escenario Individual

```powershell
# Baseline (5 min)
.\run-aws.ps1 -Scenario baseline

# Peak Load (10 min)
.\run-aws.ps1 -Scenario peak-load

# Stress Test (11 min)
.\run-aws.ps1 -Scenario stress

# Soak Test (33 min - detecta memory leaks)
.\run-aws.ps1 -Scenario soak

# Spike Test (4 min - picos repentinos)
.\run-aws.ps1 -Scenario spike
```

---

## 4. Monitoreo y Observabilidad

### 4.1 CloudWatch Metrics

Durante las pruebas, monitorea en AWS Console:

**ALB Metrics (Application Load Balancer):**
- `TargetResponseTime` → Latencia P95/P99
- `RequestCount` → Throughput (req/s)
- `HTTPCode_Target_4XX_Count` → Errores de cliente
- `HTTPCode_Target_5XX_Count` → Errores de servidor
- `HealthyHostCount` → Instancias saludables

**RDS Metrics (PostgreSQL):**
- `CPUUtilization` → Uso de CPU
- `DatabaseConnections` → Conexiones activas
- `ReadLatency` / `WriteLatency` → Latencia de DB
- `FreeableMemory` → Memoria disponible

**ECS/Fargate Metrics (si aplica):**
- `CPUUtilization` → Uso de CPU de contenedores
- `MemoryUtilization` → Uso de memoria
- `RunningTaskCount` → Tareas en ejecución

### 4.2 Logs en CloudWatch Logs

Filtra logs durante las pruebas:

```bash
# AWS CLI
aws logs filter-log-events \
  --log-group-name /ecs/espectaculos-api \
  --start-time $(date -d '10 minutes ago' +%s)000 \
  --filter-pattern "ERROR"
```

### 4.3 Integración con Prometheus/Grafana (Opcional)

Si tienes Prometheus desplegado en AWS:

```bash
# Exportar métricas de k6 a Prometheus
export K6_PROMETHEUS_RW_SERVER_URL=http://prometheus.miapp.com:9090/api/v1/write
k6 run --out experimental-prometheus-rw .\scenarios\01-baseline.js
```

---

## 5. Troubleshooting

### Problema 1: "No se puede conectar al ALB"

**Error:**
```
❌ ERROR: No se puede conectar a http://mi-alb-123.us-east-1.elb.amazonaws.com/health
```

**Soluciones:**

1. **Verificar Security Groups del ALB:**
   ```bash
   # AWS CLI
   aws elbv2 describe-load-balancers --names mi-alb
   aws ec2 describe-security-groups --group-ids sg-XXXXXXXXX
   ```
   
   Asegúrate de que permita tráfico HTTP (puerto 80) desde tu IP:
   ```
   Inbound Rules:
   - Type: HTTP (80)
   - Source: 0.0.0.0/0 (o tu IP específica)
   ```

2. **Verificar Target Group Health:**
   ```bash
   aws elbv2 describe-target-health --target-group-arn arn:aws:...
   ```
   
   Debe mostrar `State: healthy`

3. **Verificar que ECS/Fargate esté corriendo:**
   ```bash
   aws ecs list-tasks --cluster mi-cluster
   aws ecs describe-tasks --cluster mi-cluster --tasks task-id
   ```

### Problema 2: "Terraform output no encuentra alb_dns_name"

**Error:**
```
⚠️  No se encontró 'alb_dns_name' en outputs de Terraform
```

**Solución:**

```powershell
cd E:\DOTNET\.net-proyecto\INFRA

# Opción 1: Obtener manualmente
terraform output alb_dns_name

# Opción 2: Aplicar cambios si no existe el output
terraform apply

# Opción 3: Usar AWS CLI
aws elbv2 describe-load-balancers --query "LoadBalancers[0].DNSName" --output text
```

### Problema 3: "Tasa de errores alta (>1%)"

**Síntomas:**
```
✗ http_req_failed: rate=0.05  (5% de requests fallan)
```

**Diagnóstico:**

1. **Ver errores específicos en CloudWatch Logs:**
   ```bash
   aws logs tail /ecs/espectaculos-api --follow --filter-pattern "ERROR"
   ```

2. **Verificar límites de conexiones a RDS:**
   ```sql
   -- Conectarse a RDS
   SELECT count(*) FROM pg_stat_activity;
   SELECT * FROM pg_stat_activity WHERE state = 'active';
   ```

3. **Escalar capacidad (si es necesario):**
   - Aumentar instancias de ECS/Fargate
   - Aumentar clase de RDS (más CPU/RAM)
   - Habilitar Auto Scaling

### Problema 4: "Latencia P95 > 500ms"

**Síntomas:**
```
✗ http_req_duration: p(95)=823ms  (excede SLO de 500ms)
```

**Soluciones:**

1. **Optimizar queries SQL:**
   - Agregar índices faltantes
   - Optimizar queries N+1
   - Usar eager loading

2. **Implementar caché (Redis/ElastiCache):**
   - Cachear respuestas de endpoints frecuentes
   - Configurar TTL adecuado

3. **Escalar horizontalmente:**
   - Aumentar número de tasks de ECS
   - Configurar Auto Scaling basado en CPU

4. **Revisar cold starts (si usas Lambda):**
   - Aumentar reserved concurrency
   - Usar provisioned concurrency

---

## 6. Mejores Prácticas

### 6.1 Antes de Ejecutar Pruebas

- ✅ **Notificar al equipo:** Avisa que se ejecutarán pruebas de carga
- ✅ **Verificar horario:** Ejecuta en horarios de bajo tráfico
- ✅ **Backup de DB:** Asegúrate de tener backup reciente
- ✅ **Revisar límites:** Verifica límites de AWS (Service Quotas)
- ✅ **Monitoreo activo:** Ten CloudWatch/Grafana abierto

### 6.2 Durante las Pruebas

- ✅ **Monitorear métricas:** CloudWatch, Grafana, k6 output
- ✅ **Estar disponible:** Listo para detener pruebas si hay problemas
- ✅ **Anotar anomalías:** Timestamps de picos o errores

### 6.3 Después de las Pruebas

- ✅ **Analizar resultados:** Comparar con SLOs
- ✅ **Revisar logs:** Buscar errores o warnings
- ✅ **Verificar costos:** Revisar AWS Cost Explorer
- ✅ **Documentar findings:** Crear issues para mejoras

### 6.4 CI/CD Integration

Integra pruebas en tu pipeline:

```yaml
# .github/workflows/performance-tests.yml
name: Performance Tests

on:
  schedule:
    - cron: '0 2 * * *'  # Cada día a las 2 AM
  workflow_dispatch:

jobs:
  performance:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Install k6
        run: |
          sudo gpg -k
          sudo gpg --no-default-keyring --keyring /usr/share/keyrings/k6-archive-keyring.gpg --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys C5AD17C747E3415A3642D57D77C6C491D6AC1D69
          echo "deb [signed-by=/usr/share/keyrings/k6-archive-keyring.gpg] https://dl.k6.io/deb stable main" | sudo tee /etc/apt/sources.list.d/k6.list
          sudo apt-get update
          sudo apt-get install k6
      
      - name: Run performance tests
        env:
          BASE_URL: ${{ secrets.AWS_ALB_URL }}
        run: |
          cd BACKEND/LabNet/performance-tests
          k6 run scenarios/01-baseline.js
      
      - name: Upload results
        uses: actions/upload-artifact@v3
        with:
          name: k6-results
          path: performance-tests/results/
```

---

## 7. Comparación Local vs AWS

| Aspecto | Local (Docker) | AWS (ALB + ECS) |
|---------|----------------|-----------------|
| **URL** | `http://localhost:8080` | `http://alb-123.elb.amazonaws.com` |
| **Latencia** | 10-50ms | 50-200ms (incluye red) |
| **Throughput** | Limitado por hardware local | Escalable (hasta límites AWS) |
| **Costos** | Gratis | $$ (depende de uso) |
| **Observabilidad** | Prometheus + Grafana local | CloudWatch + (opcional) Grafana |
| **Escalabilidad** | Manual (cambiar resources) | Auto Scaling |
| **Red** | Localhost (sin latencia red) | Internet/VPN (latencia real) |

---

## 8. Métricas Objetivo (SLOs)

### Entorno Local

| Métrica | SLO |
|---------|-----|
| P95 Latencia | < 300ms |
| P99 Latencia | < 500ms |
| Error Rate | < 0.5% |
| Throughput | > 10 req/s |

### Entorno AWS

| Métrica | SLO |
|---------|-----|
| P95 Latencia | < 500ms (más permisivo por latencia de red) |
| P99 Latencia | < 800ms |
| Error Rate | < 1% |
| Throughput | > 50 req/s |
| ALB TargetResponseTime P95 | < 400ms |
| RDS CPU | < 80% |

---

## 9. Comandos Útiles

```powershell
# Obtener DNS del ALB desde Terraform
cd E:\DOTNET\.net-proyecto\INFRA
terraform output -json | ConvertFrom-Json | Select-Object -ExpandProperty alb_dns_name | Select-Object -ExpandProperty value

# Ejecutar prueba rápida (1 minuto)
$env:BASE_URL="http://mi-alb-123.elb.amazonaws.com"; k6 run --duration 60s --vus 10 .\scenarios\01-baseline.js

# Ver métricas de ALB en tiempo real (AWS CLI)
aws cloudwatch get-metric-statistics \
  --namespace AWS/ApplicationELB \
  --metric-name TargetResponseTime \
  --dimensions Name=LoadBalancer,Value=app/mi-alb/1234567890 \
  --start-time $(date -u -d '10 minutes ago' +%Y-%m-%dT%H:%M:%S) \
  --end-time $(date -u +%Y-%m-%dT%H:%M:%S) \
  --period 60 \
  --statistics Average

# Ver logs en tiempo real
aws logs tail /ecs/espectaculos-api --follow
```

---

## 10. Recursos Adicionales

- **Documentación k6:** https://k6.io/docs/
- **AWS ALB Monitoring:** https://docs.aws.amazon.com/elasticloadbalancing/latest/application/load-balancer-cloudwatch-metrics.html
- **RDS Performance Insights:** https://docs.aws.amazon.com/AmazonRDS/latest/UserGuide/USER_PerfInsights.html

---

**Última actualización:** Noviembre 2025  
**Versión:** 1.0
