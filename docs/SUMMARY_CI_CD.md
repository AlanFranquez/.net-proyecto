# Resumen rápido CI/CD

Propósito: pipeline automatizado para build, test, análisis de calidad y despliegue a AWS (ECR + ECS + Terraform).

Workflows principales:
- `CI` (.github/workflows/ci.yml): build .NET, tests, coverage, Sonar (opcional), Trivy (scan).
- `CD Image` (.github/workflows/cd-image.yml): build Docker (Buildx), push a ECR, Trivy image scan.
- `CD Infra` (.github/workflows/cd-infra.yml): Terraform plan/validate (PR), tfsec, apply (main).
- `CD Testing` (.github/workflows/cd-testing.yml): despliegue a ECS testing, health checks y smoke tests.

Triggers (resumen):
- CI: push/PR a ramas de desarrollo y feature.
- CD Image: push a `main` o tag de release (o manual run).
- CD Infra: PR a `main` (plan), push a `main` (apply).
- CD Testing: auto después de CD Image o ejecución manual.

Secrets y variables críticas (debe definir en GitHub):
- `AWS_ACCOUNT_ID` (required)
- `AWS_ROLE_TO_ASSUME` (recommended, with OIDC)
- `TF_STATE_BUCKET` (S3 bucket para Terraform state)
- `SONAR_TOKEN` (opcional, si se usa SonarQube)
- `SLACK_WEBHOOK_URL` (opcional)

Comandos rápidos (Windows `cmd.exe`):

```bat
cd BACKEND\LabNet\src\Espectaculos.WebApi
dotnet restore
dotnet build
dotnet test
```

Terraform (desde repo root):

```bat
cd INFRA
terraform init
terraform validate
terraform plan -out=tfplan
```

Verificar despliegue (rápido):
- GitHub Actions → seleccionar workflow → revisar logs.
- AWS Console → ECR (ver imagen), ECS (tareas y servicios), CloudWatch logs.

Notas:
- Sonar y otros pasos marcados como opcionales no fallan la pipeline si no están configurados (continue-on-error).
- OIDC recomendado: no uses AWS access keys en Secrets cuando sea posible.

Última actualización: Noviembre 2025
