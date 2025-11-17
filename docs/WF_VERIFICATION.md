# Guía rápida: Verificar que los workflows funcionan

Objetivo: pasos mínimos para comprobar que CI, CD-Image, CD-Infra y CD-Testing se ejecutan correctamente.

1) Comprobaciones locales rápidas
- Build & tests (backend):

```bat
cd BACKEND\LabNet\src\Espectaculos.WebApi
dotnet restore
dotnet build
dotnet test
```

- Si todo build/ tests pasan, continúas a CI.

2) Forzar ejecución del CI (pr/branch)
- Push una rama corta y crea PR hacia `develop` o `main`.
- En GitHub → Actions → seleccionar `CI` → verificar ejecución.
- Qué revisar:
  - Job `build` -> solución compilada
  - Job `test` -> tests OK (o ver fallos)
  - Artefactos/coverage subidos (si aplica)

3) Verificar CD Image (build & push)
- Trigger: push a `main` o ejecutar manualmente el workflow `CD Image` desde Actions.
- Revisa:
  - Paso `Build and push` completa (imagen subida a ECR)
  - Paso `Trivy` no reporta falla crítica
  - Obtener image URI en logs (output variable)

Comando rápido para comprobar imagen en AWS (local):

```bat
REM Mostrar imágenes en ECR
aws ecr list-images --repository-name espectaculos-api --region us-east-1
```

4) Verificar CD Infra (Terraform)
- Crear PR con cambios en `INFRA/` para ver job `plan`.
- Para apply, push a `main` o ejecutar manualmente (requiere permisos protegidos).
- Revisa artefacto `tfplan` y logs de `tfsec`.

5) Verificar CD Testing (despliegue a ECS)
- El workflow `CD Testing` se dispara después de `CD Image` o puede correr manualmente.
- Revisa pasos:
  - `Register Task Definition`
  - `Update Service` (tarea reemplazada con nueva imagen)
  - `Wait for service stable`
  - `Smoke tests` (endpoint `/health` o pruebas básicas)
- Logs CloudWatch (comando):

```bat
aws logs tail /ecs/espectaculos-api-testing --follow --region us-east-1
```

6) Qué hacer si algo falla (rápido)
- CI falla: abrir PR → añadir correcciones → rerun jobs.
- CD Image falla: revisar Dockerfile y runner logs (memoria/timeouts). Re-run workflow manually.
- Terraform plan falla: revisar variables y backend S3 permissions.
- ECS deploy falla: revisar task definition, permisos IAM y logs CloudWatch.

7) Checklist mínimo (OK = ✅)
- [ ] Build local ✅
- [ ] Tests local ✅
- [ ] CI run en PR ✅
- [ ] Imagen subida a ECR ✅
- [ ] Terraform plan correcto ✅
- [ ] CD Testing despliegue estable ✅

Si quieres, puedo ejecutar una lectura automática de los workflows para listar los secrets referenciados y generar un pequeño checklist de secrets pendientes en el repo. ¿Lo hago ahora?
