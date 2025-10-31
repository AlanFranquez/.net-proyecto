# 🎭 Espectáculos — Backend (ASP.NET Core Web API)

API backend para la gestión de eventos, entradas y órdenes de compra/canje.  
Construida con **.NET 8**, organizada con **Arquitectura Clean**, soportando **PostgreSQL**, **EF Core**, **MediatR**, **FluentValidation**, **HealthChecks** y **Serilog** para logging estructurado.

---

## 🗂️ Estructura de Carpetas

```
/src
  /Espectaculos.Domain        → Entidades de negocio (agregados, entidades, value objects)
  /Espectaculos.Application   → Casos de uso, CQRS (commands/queries) con MediatR
  /Espectaculos.Infrastructure→ Persistencia (EF Core, Repositorios, Migrations, PostgreSQL)
  /Espectaculos.WebApi        → Endpoints HTTP, configuración, middleware, swagger
/docker                       → Scripts y configuración para contenedores (db + api)
/scripts                      → Automatización (ej: up.ps1 para levantar entorno completo)
```

---

## 🧩 Arquitectura Clean

El backend sigue un esquema de **Clean Architecture**:

- **Domain**: Núcleo de negocio, sin dependencias externas.
- **Application**: Lógica de casos de uso (handlers de comandos/queries). Usa `MediatR`.
- **Infrastructure**: Implementaciones de persistencia (EF Core con PostgreSQL).
- **WebApi**: Capa de presentación, expone endpoints RESTful.

Esto garantiza **separación de responsabilidades**, **testabilidad** y **flexibilidad** para futuras integraciones.

---

## 🔑 Funcionalidades principales

- 📅 **Eventos**: creación, listado, detalle, gestión de entradas con stock.
- 🛒 **Órdenes**: checkout de entradas, cálculo de totales, confirmación de compra.
- 🎟️ **Tokens**: generación tipo JWT con expiración para validar/canjear órdenes.
- ✅ **Canje de órdenes**: endpoint `POST /api/ordenes/redeem` para validar el acceso en el punto de entrada.
- 📊 **Health checks**: disponibles en `/health` (incluye PostgreSQL).
- 📜 **Swagger/OpenAPI**: documentación en `/swagger`.

---

## ⚙️ Configuración y ejecución

### 🔧 Requisitos previos
- [.NET SDK 8+](https://dotnet.microsoft.com/)
- [PostgreSQL](https://www.postgresql.org/) (local o vía Docker)
- PowerShell 7+ (para scripts de automatización)
- Docker & Docker Compose (para levantar infraestructura completa)

### ▶️ Modos de ejecución

**1) Desarrollo local con PostgreSQL ya instalado**
```bash

cd src/Espectaculos.WebApi
dotnet restore
dotnet ef database update --project ../Espectaculos.Infrastructure --startup-project .
dotnet run
```

**2) Entorno completo con Docker (recomendado)**
```powershell
# API + DB + Observabilidad (Grafana, Prometheus, Tempo, Seq)
pwsh ./scripts/dev-up.ps1 -Seed

# Solo API + DB
pwsh ./scripts/up.ps1 -Seed

# Solo Observabilidad
pwsh ./scripts/observability.ps1
```
Esto levanta **API + PostgreSQL** (y opcionalmente Observabilidad) y ejecuta el seed inicial de datos.

**3) Verificación de salud**
```bash

curl http://localhost:8080/health
```

### 📈 Observabilidad

- Seq:       http://localhost:5341  (logs Serilog)
- Grafana:   http://localhost:3000  (admin/admin)
- Prometheus: http://localhost:9090
- OTel Metrics (Collector): http://localhost:9464/metrics

Para bajar todo:

```powershell
pwsh ./scripts/dev-down.ps1   # baja observabilidad y API; usa -KeepDbVolume para conservar datos
```

---

## 🗄️ Migraciones (EF Core)

Para generar un snapshot determinista de la base:
```bash

dotnet ef migrations add Init_2025_09   --project src/Espectaculos.Infrastructure   --startup-project src/Espectaculos.WebApi

dotnet ef database update   --project src/Espectaculos.Infrastructure   --startup-project src/Espectaculos.WebApi
```

---

## 🛠️ Stack técnico

- **.NET 8 + ASP.NET Core**
- **Entity Framework Core + PostgreSQL**
- **MediatR** para CQRS
- **FluentValidation** para validaciones
- **Swagger/OpenAPI** para documentación
- **HealthChecks** (incl. NpgSql)
- **Serilog** para logging estructurado

---

## 🚀 Endpoints principales (resumen)

- `GET /api/eventos` → lista eventos
- `GET /api/eventos/{id}` → detalle
- `POST /api/ordenes/crear` → crea orden
- `GET /api/ordenes/{id}` → detalle de orden
- `POST /api/ordenes/redeem` → canje de orden
- `GET /health` → health check

---

## 📦 Despliegue

- El backend puede correr standalone (`dotnet run`) o en Docker (`scripts/up.ps1`).
- Incluye `docker-compose.override.yml` para entornos locales.

---

## 🧑‍💻 Desarrollo y contribución

1. Crear rama feature:
   ```bash
   git checkout -b feature/nueva-funcionalidad
   ```
2. Hacer cambios siguiendo la arquitectura clean.
3. Ejecutar tests y verificar que swagger y health funcionen.
4. Commit y push con convención:
   ```bash
   git commit -m "feat(api): descripción breve"
   ```

---

✨ Proyecto educativo para demostrar arquitectura **Clean** + **.NET 8** + **Blazor/MudBlazor** (frontend desacoplado).  
