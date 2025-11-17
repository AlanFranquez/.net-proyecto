# 📚 Documentación CI/CD y Despliegue

> Índice de documentación completo para setup local, CI/CD automation y despliegue en AWS

---

## 📖 Documentos Principales

### ⚡ Resumen Corto

- 🔹 [SUMMARY_CI_CD.md](./SUMMARY_CI_CD.md) — Guía condensada (2–3 min).
- 🔹 [WF_VERIFICATION.md](./WF_VERIFICATION.md) — Pasos rápidos para verificar workflows.

### 🚀 [SETUP_COMPLETO.md](./SETUP_COMPLETO.md)
**Guía principal para setup local y remoto**

- ✅ Requisitos previos
- ✅ Configuración local (Windows/Mac/Linux)
- ✅ Ejecución del Backend (.NET)
- ✅ Ejecución del Frontend (React)
- ✅ Pipeline CI/CD explicado
- ✅ Despliegue en AWS
- ✅ Monitoreo y logs
- ✅ Troubleshooting

**Duración lectura:** ~30 minutos

**Para quién:** Desarrolladores, DevOps, QA

---

### 🔐 [AWS_GITHUB_ACTIONS_SETUP.md](./AWS_GITHUB_ACTIONS_SETUP.md)
**Configuración segura con GitHub Actions + AWS OIDC**

- ✅ Qué es OIDC y por qué es importante
- ✅ Crear IAM Role con confianza OIDC
- ✅ Configurar Secrets en GitHub
- ✅ Preparar infraestructura base (S3, DynamoDB, ECR)
- ✅ Verificación y troubleshooting
- ✅ Best practices de seguridad

**Duración lectura:** ~20 minutos

**Para quién:** DevOps, Security Engineers

---

### 🏗️ [AWS_IAM_SETUP.md](./AWS_IAM_SETUP.md)
**Script automático para setup de IAM**

- ✅ Script Bash (Linux/Mac)
- ✅ Script PowerShell (Windows)
- ✅ Pasos manuales en AWS Console
- ✅ Checklist de verificación

**Duración lectura:** ~10 minutos

**Para quién:** DevOps, System Administrators

---

### 📋 [PIPELINE_SPECIFICATION.md](./PIPELINE_SPECIFICATION.md)
**Especificación técnica de todos los workflows**

- ✅ Arquitectura general del pipeline
- ✅ CI Workflow detallado
- ✅ CD Image Workflow (Docker)
- ✅ CD Infra Workflow (Terraform)
- ✅ CD Testing Workflow (ECS Deployment)
- ✅ Autenticación y permisos
- ✅ Monitoreo
- ✅ Troubleshooting técnico

**Duración lectura:** ~40 minutos

**Para quién:** DevOps Engineers, Architects

---

## 🎯 Guías Rápidas por Rol

### 👨‍💻 Desarrollador Backend/Frontend

**Necesitas:**
1. Leer [SETUP_COMPLETO.md](./SETUP_COMPLETO.md) → Configuración Local
2. Entender CI Workflow en [PIPELINE_SPECIFICATION.md](./PIPELINE_SPECIFICATION.md)
3. Configurar IDE/Editor local

**Tiempo:** ~1 hora

```bash
# Checklist rápido
✅ Fork/Clone repositorio
✅ .NET 8 SDK instalado
✅ Node.js 18+ instalado
✅ PostgreSQL corriendo (Docker)
✅ Backend: dotnet run (http://localhost:8080)
✅ Frontend: npm run dev (http://localhost:5173)
✅ Tests pasando
```

---

### 🔧 DevOps / SRE

**Necesitas:**
1. Leer [AWS_GITHUB_ACTIONS_SETUP.md](./AWS_GITHUB_ACTIONS_SETUP.md)
2. Leer [AWS_IAM_SETUP.md](./AWS_IAM_SETUP.md)
3. Leer [SETUP_COMPLETO.md](./SETUP_COMPLETO.md) → Despliegue en AWS
4. Leer [PIPELINE_SPECIFICATION.md](./PIPELINE_SPECIFICATION.md)
5. Configurar AWS Infrastructure

**Tiempo:** ~3-4 horas

```bash
# Checklist rápido
✅ AWS CLI configurado
✅ IAM Role GitHubActionsRole creado
✅ OIDC Provider en AWS configurado
✅ S3 state bucket para Terraform
✅ DynamoDB table para locks
✅ ECR repository creado
✅ GitHub Secrets configurados
✅ Primer despliegue exitoso
✅ Monitoreo en CloudWatch
```

---

### 🏗️ Architect / Tech Lead

**Necesitas:**
1. Entender arquitectura completa en [SETUP_COMPLETO.md](./SETUP_COMPLETO.md)
2. Revisar especificación técnica en [PIPELINE_SPECIFICATION.md](./PIPELINE_SPECIFICATION.md)
3. Evaluar seguridad en [AWS_GITHUB_ACTIONS_SETUP.md](./AWS_GITHUB_ACTIONS_SETUP.md)
4. Revisar workflows en `.github/workflows/`

**Tiempo:** ~2 horas

---

### 🧪 QA / Tester

**Necesitas:**
1. Leer [SETUP_COMPLETO.md](./SETUP_COMPLETO.md) → Setup Local
2. Entender CI Tests en [PIPELINE_SPECIFICATION.md](./PIPELINE_SPECIFICATION.md)
3. Verificar health check en [SETUP_COMPLETO.md](./SETUP_COMPLETO.md) → Monitoreo

**Tiempo:** ~1 hora

---

## 🔄 Workflows en el Repositorio

### Ubicación: `.github/workflows/`

```
.github/workflows/
├── ci.yml                 # Build, Test, Code Quality
├── cd-image.yml           # Build Docker, Push to ECR
├── cd-infra.yml           # Terraform Plan & Apply
└── cd-testing.yml         # Deploy to Testing Environment
```

### Matriz de Triggers

| Workflow | Trigger | Rama | Condición |
|----------|---------|------|----------|
| **CI** | Push, PR | develop, feature/*, main | Siempre |
| **CD Image** | Push | main | `BACKEND/LabNet/**` |
| **CD Infra** | Push, PR | main, develop | `INFRA/**` |
| **CD Testing** | Auto (CD Image), Manual | main | Siempre |

---

## 🌐 Arquitectura AWS Desplegada

```
┌─────────────────────────────────────────────────────────────┐
│                      AWS (us-east-1)                        │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ CloudFront → S3 (Frontend)                          │  │
│  └──────────────────────────────────────────────────────┘  │
│                         │ HTTPS                             │
│                         ▼                                   │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ ALB (Application Load Balancer)                     │  │
│  └──────────────────────────────────────────────────────┘  │
│              │              │                               │
│              ▼              ▼                               │
│  ┌────────────────────┐  ┌────────────────────┐           │
│  │ ECS Fargate Prod   │  │ ECS Fargate Testing│           │
│  │ (espectaculos-api) │  │ (espectaculos-api) │           │
│  └────────────────────┘  └────────────────────┘           │
│              │              │                               │
│              └──────────┬───┘                               │
│                         ▼                                   │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ RDS PostgreSQL (Multi-AZ)                           │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                             │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ ECR (Elastic Container Registry)                    │  │
│  │ - espectaculos-api:latest                          │  │
│  │ - espectaculos-api:<commit-sha>                    │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                             │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ Logging & Monitoring                                │  │
│  │ - CloudWatch Logs                                   │  │
│  │ - CloudWatch Alarms                                 │  │
│  │ - X-Ray Tracing                                     │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## 📊 Flujo de un Deployment Completo

### 1️⃣ Desarrollador hace push

```bash
git commit -m \"feat: new feature\"
git push origin feature/my-feature
```

### 2️⃣ CI Workflow se ejecuta (automático)

```
✅ Build .NET
✅ Run Tests
✅ Code Quality Analysis
✅ Security Scan (Trivy)
```

**Si falla:** PR bloqueado, requiere fix

### 3️⃣ PR Review & Merge

```
Aprox: 1-2 horas
- Code review
- QA testing
- Merge a main
```

### 4️⃣ CD Workflows se ejecutan (automático)

#### En paralelo:
```
CD Image                        CD Infra
├─ Build Docker              ├─ Terraform Plan
├─ Push to ECR               ├─ Security scan (tfsec)
└─ Scan image                └─ (PR) Terraform Apply
    │
    ▼
CD Testing (Automático)
├─ Deploy to ECS Testing
├─ Wait for service
├─ Health check
├─ Smoke tests
└─ Notifications
```

**Duración total:** ~40-50 minutos

### 5️⃣ Production Deployment (Manual)

```
Cuando esté listo para producción:
1. Crear release branch
2. Terraform Apply con prod resources
3. Actualizar ECS Production Service
4. Verificar health & logs
```

---

## 🔍 Monitoreo Post-Deployment

### CloudWatch
```bash
aws logs tail /ecs/espectaculos-api-testing --follow
```

### GitHub Actions
- Ir a **Actions** → Filtrar workflow
- Ver logs detallados
- Descargar artifacts

### AWS Console
- **ECS** → Services → Ver tasks
- **ECR** → Repositories → Ver imágenes
- **CloudWatch** → Logs → Ver logs en tiempo real

---

## 🛠️ Troubleshooting Rápido

### \"Build falla\"
→ Ver [SETUP_COMPLETO.md](./SETUP_COMPLETO.md#troubleshooting)

### \"AWS credentials error\"
→ Ver [AWS_GITHUB_ACTIONS_SETUP.md](./AWS_GITHUB_ACTIONS_SETUP.md#verificación)

### \"ECS deployment falla\"
→ Ver [PIPELINE_SPECIFICATION.md](./PIPELINE_SPECIFICATION.md#cd-testing---deploy-testing)

### \"Terraform apply error\"
→ Ver [PIPELINE_SPECIFICATION.md](./PIPELINE_SPECIFICATION.md#cd-infra---terraform)

---

## 📞 Soporte

### Por Issue:
1. Buscar en troubleshooting guides
2. Revisar logs en GitHub Actions
3. Revisar CloudWatch si es AWS
4. Abrir issue en GitHub con contexto

### Por Equipo:
- **Backend Issues:** @backend-team
- **DevOps Issues:** @devops-team
- **Frontend Issues:** @frontend-team

---

## 🔐 Seguridad Checklist

- [x] OIDC en lugar de Access Keys
- [x] Secretos nunca en código
- [x] Trivy escanea imágenes
- [x] tfsec valida Terraform
- [x] Permisos IAM limitados
- [x] State bucket encriptado
- [x] ECR images privadas
- [x] Database en subnet privada
- [x] ALB con HTTPS
- [x] Logging completo en CloudWatch

---

## 📈 KPIs del Pipeline

| Métrica | Target | Actual |
|---------|--------|--------|
| Build Time | < 10 min | ~8 min |
| Test Coverage | >= 70% | ~75% |
| Deployment Time | < 15 min | ~12 min |
| MTTR (Mean Time To Recovery) | < 30 min | TBD |
| Uptime | >= 99.5% | TBD |

---

## 📚 Recursos Externos

- [GitHub Actions Docs](https://docs.github.com/en/actions)
- [AWS ECS Documentation](https://docs.aws.amazon.com/ecs/)
- [Terraform AWS Provider](https://registry.terraform.io/providers/hashicorp/aws/latest/docs)
- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)
- [OWASP CI/CD Security](https://owasp.org/www-project-devsecops-guideline/)

---

**📝 Última actualización:** Noviembre 2024
**✅ Status:** Production Ready
**📊 Versión:** 1.0.0
"