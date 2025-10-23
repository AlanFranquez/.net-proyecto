using System.Reflection;
using System.Text.Json.Serialization;
using Espectaculos.Application;
using Espectaculos.Application.Abstractions;
using Espectaculos.Application.Abstractions.Repositories;
using Espectaculos.Application.Commands.CrearOrden;
using Espectaculos.Application.Commands.CrearUsuario;
using Espectaculos.Application.Commands.CreateEvento;
using Espectaculos.Application.Commands.PublicarEvento;
using Espectaculos.Application.Espacios.Commands.CreateEspacio;
using Espectaculos.Application.Espacios.Commands.DeleteEspacio;
using Espectaculos.Application.Espacios.Commands.UpdateEspacio;
using Espectaculos.Application.Usuarios.Commands.CreateUsuario;
using Espectaculos.Infrastructure.Persistence;
using Espectaculos.Infrastructure.Persistence.Interceptors;
using Espectaculos.Infrastructure.Persistence.Seed;
using Espectaculos.Infrastructure.Repositories;
using Espectaculos.WebApi.Endpoints;
using Espectaculos.WebApi.Health;
using Espectaculos.WebApi.Options;
using Espectaculos.WebApi.Security;
using Espectaculos.WebApi.SerilogConfig;
using FluentValidation;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Serilog;
using Espectaculos.WebApi.Security;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ---- Logging (Serilog)
builder.AddSerilogLogging();

// ---- Configuración
var config = builder.Configuration;

// Asegurar orden de configuración: JSON base -> JSON por entorno -> Variables de entorno
// Nota: soportamos variables con y sin prefijo "APP__".
// - Si usás docker-compose con APP__VALIDATION_TOKENS__SECRET, el proveedor con prefijo lo recorta a VALIDATION_TOKENS__SECRET.
// - Si usás VALIDATION_TOKENS__SECRET directamente, el proveedor sin prefijo lo toma tal cual.
builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables("APP__") // quita el prefijo "APP__" si existe
    .AddEnvironmentVariables();       // sin prefijo (toma el resto)

// Log de diagnóstico en Development (no muestra el secreto)
if (builder.Environment.IsDevelopment())
{
    var secretPresent = string.IsNullOrWhiteSpace(builder.Configuration["ValidationTokens:Secret"]) ? "absent" : "present";
    Serilog.Log.Information("Startup diagnostic: ValidationTokens:Secret {Status} in configuration (Development).", secretPresent);
}

string connectionString =
    config.GetConnectionString("Default")
    ?? config["ConnectionStrings__Default"]
    ?? "Host=localhost;Port=5432;Database=espectaculosdb;Username=postgres;Password=postgres";

// ---- Servicios
builder.Services.AddEndpointsApiExplorer();

// ===== CONFIGURACIÓN CONSOLIDADA DE SWAGGER =====
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Espectáculos - Demo API",
        Version = "v1",
        Description = "API pública para la demo. Incluye endpoints de eventos y órdenes. Endpoints de administración permanecen ocultos por defecto."
    });

    // Agrupar endpoints por categorías basadas en el primer segmento después de "api"
    // Ejemplo: /api/espacios/{id} => categoría "Espacios"
    o.TagActionsBy(apiDesc =>
    {
        var relativePath = apiDesc.RelativePath ?? string.Empty;
        var segments = relativePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        
        // Si la ruta empieza con "api", tomamos el siguiente segmento
        if (segments.Length >= 2 && segments[0].Equals("api", StringComparison.OrdinalIgnoreCase))
        {
            // Capitalizar la primera letra para que se vea más prolijo
            var category = segments[1];
            return new[] { char.ToUpper(category[0]) + category.Substring(1) };
        }
        
        if (segments.Length >= 1)
            return new[] { segments[0] };
            
        return new[] { "General" };
    });

    // Mostrar enums como strings en lugar de números en Swagger UI
    o.UseInlineDefinitionsForEnums();
});

// Aceptar enums representados como strings en JSON (p.ej. "Comedor") y case-insensitive
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(opts =>
{
    opts.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    opts.SerializerOptions.Converters.Add(new Espectaculos.WebApi.Json.CaseInsensitiveEnumConverterFactory());
});

// Options: ValidationTokens (fail-fast)
var validationSection = builder.Configuration.GetSection("ValidationTokens");
builder.Services
    .AddOptions<ValidationTokenOptions>()
    .Bind(validationSection)
    // Fallbacks robustos: primero IConfiguration con ":", luego variables de entorno crudas (con y sin prefijo APP__)
    .PostConfigure(o =>
    {
        if (string.IsNullOrWhiteSpace(o.Secret))
        {
            var envSecret =
                builder.Configuration["ValidationTokens:Secret"]
                ?? Environment.GetEnvironmentVariable("VALIDATION_TOKENS__SECRET")
                ?? Environment.GetEnvironmentVariable("APP__VALIDATION_TOKENS__SECRET");
            if (!string.IsNullOrWhiteSpace(envSecret)) o.Secret = envSecret;
        }
        if (o.DefaultExpiryMinutes <= 0)
        {
            var envExpStr =
                builder.Configuration["ValidationTokens:DefaultExpiryMinutes"]
                ?? Environment.GetEnvironmentVariable("VALIDATION_TOKENS__DEFAULT_EXPIRY_MINUTES")
                ?? Environment.GetEnvironmentVariable("APP__VALIDATION_TOKENS__DEFAULT_EXPIRY_MINUTES");
            if (int.TryParse(envExpStr, out var mins) && mins > 0) o.DefaultExpiryMinutes = mins;
        }
    })
    .Validate(o => !string.IsNullOrWhiteSpace(o.Secret), "ValidationTokens:Secret es obligatorio (use env VALIDATION_TOKENS__SECRET).")
    .Validate(o => o.DefaultExpiryMinutes > 0 && o.DefaultExpiryMinutes <= 10080, "ValidationTokens:DefaultExpiryMinutes debe ser 1..10080.")
    .ValidateOnStart();

builder.Services.AddSingleton<IValidationTokenService, HmacValidationTokenService>();

// EF Core + Npgsql (simple y robusto)
builder.Services.AddSingleton<AuditableEntitySaveChangesInterceptor>();
builder.Services.AddDbContext<EspectaculosDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

// Health checks
builder.Services.AddHealthChecks();
builder.Services.AddPostgresHealthChecks(connectionString);

// CORS (solo dev) — orígenes permitidos por config/env, con defaults sensatos
var isDev = builder.Environment.IsDevelopment();
var devOrigins = (config["Cors:AllowedOrigins"]
                  ?? Environment.GetEnvironmentVariable("CORS_ORIGINS")
                  ?? "http://localhost:5262,http://localhost:5173")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

if (isDev)
{
    builder.Services.AddCors(o =>
    {
        o.AddPolicy("DevCors", p =>
            p.WithOrigins(devOrigins)
             .AllowAnyHeader()
             .AllowAnyMethod()
        );
    });
}

// ===== CONFIGURACIÓN GLOBAL DE JSON PARA ENUMS =====
// Permite enviar y recibir enums como strings en lugar de números
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Validators (Application)
builder.Services.AddScoped<IValidator<CreateEventoCommand>, CreateEventoValidator>();
builder.Services.AddScoped<IValidator<PublicarEventoCommand>, PublicarEventoValidator>();
builder.Services.AddScoped<IValidator<CrearOrdenCommand>, CrearOrdenValidator>();
builder.Services.AddScoped<IValidator<CreateEspacioCommand>, CreateEspacioValidator>();
builder.Services.AddScoped<IValidator<UpdateEspacioCommand>, UpdateEspacioValidator>();
builder.Services.AddScoped<IValidator<DeleteEspacioCommand>, DeleteEspacioValidator>();
builder.Services.AddScoped<CrearUsuarioHandler>();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateEspacioCommand).Assembly));
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblies(
        typeof(CreateUsuarioCommand).Assembly,
        Assembly.GetExecutingAssembly()
    )
);


// Repos + UoW: registrar repositorios primero, luego IUnitOfWork
builder.Services.AddScoped<IEventoRepository, EventoRepository>();
builder.Services.AddScoped<IEntradaRepository, EntradaRepository>();
builder.Services.AddScoped<IOrdenRepository, OrdenRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IEspacioRepository, EspacioRepository>();
builder.Services.AddScoped<IReglaDeAccesoRepository, ReglaDeAccesoRepository>();
builder.Services.AddScoped<IBeneficioRepository, BeneficioRepository>();
builder.Services.AddScoped<IBeneficioUsuarioRepository, BeneficioUsuarioRepository>();
builder.Services.AddScoped<IBeneficioEspacioRepository, BeneficioEspacioRepository>();
builder.Services.AddScoped<ICanjeRepository, CanjeRepository>();

// Finalmente el UnitOfWork (depende de los repos registrados arriba)
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
// Seeder
builder.Services.AddScoped<DbSeeder>();
builder.Services.AddRouting();

var app = builder.Build();

// ---------- 1) Archivos estáticos (sirven la SPA publicada) ----------
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".dat"]  = "application/octet-stream"; // ICU/native data
provider.Mappings[".wasm"] = "application/wasm";
provider.Mappings[".br"]   = "application/octet-stream";

app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = provider });

// ---------- 2) Logging, CORS, Swagger ----------
app.UseSerilogRequestLogging();
if (isDev) app.UseCors("DevCors");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Espectaculos API v1");
    c.RoutePrefix = "swagger";
});

// ---------- 3) API bajo /api ----------
var api = app.MapGroup("/api");

// Mapea tus endpoints SOBRE el grupo (usar rutas relativas en las extensiones)
api.MapEventosEndpoints();
api.MapUsuariosEndpoints();
api.MapEspaciosEndpoints();
api.MapOrdenesEndpoints();
api.MapBeneficiosEndpoints();
api.MapCanjesEndpoints();

// Health root para readiness checks fuera de /api
app.MapHealthChecks("/health");
api.MapHealthChecks("/health");

// === ADMIN: quick seed para poblar datos desde curl ===
// Uso: POST /admin/quick-seed?count=70&publish=true
var enableAdmin = (Environment.GetEnvironmentVariable("DEMO_ENABLE_ADMIN") ?? config["DEMO_ENABLE_ADMIN"] ?? "false")
    .Equals("true", StringComparison.OrdinalIgnoreCase);
if (enableAdmin)
{
    app.MapPost("/admin/quick-seed", async (int count, bool publish, EspectaculosDbContext db) =>
    {
        if (count <= 0) return Results.BadRequest("count debe ser > 0");

        var ahora = DateTime.UtcNow;
        var rnd = new Random();
        var nuevos = new List<Espectaculos.Domain.Entities.Evento>(count);

        for (int i = 0; i < count; i++)
        {
            var ev = new Espectaculos.Domain.Entities.Evento
            {
                Id = Guid.NewGuid(),
                Titulo = $"Festival Demo #{i + 1}",
                Descripcion = "Evento generado rápido para pruebas de front.",
                Fecha = ahora.AddDays(rnd.Next(3, 180)),
                Lugar = rnd.Next(0, 2) == 0 ? "Antel Arena, Montevideo" : "Luna Park, Buenos Aires",
                Publicado = publish,
                Entradas = new List<Espectaculos.Domain.Entities.Entrada>
                {
                    new() { Id = Guid.NewGuid(), Tipo = "General", Precio = rnd.Next(40,120),  StockTotal = 5000, StockDisponible = 5000 },
                    new() { Id = Guid.NewGuid(), Tipo = "Platea",  Precio = rnd.Next(120,240), StockTotal = 2000, StockDisponible = 2000 },
                    new() { Id = Guid.NewGuid(), Tipo = "VIP",     Precio = rnd.Next(240,480), StockTotal =  500, StockDisponible =  500 },
                }
            };

            foreach (var en in ev.Entradas) en.EventoId = ev.Id; // asegurar FK
            nuevos.Add(ev);
        }

        await db.Eventos.AddRangeAsync(nuevos);
        await db.SaveChangesAsync();

        return Results.Ok(new { inserted = nuevos.Count });
    });
}

// === ADMIN: publicar todos los eventos existentes ===
// Uso: POST /admin/publish-all
if (enableAdmin)
{
    app.MapPost("/admin/publish-all", async (EspectaculosDbContext db) =>
    {
        var todos = await db.Eventos.ToListAsync();
        foreach (var e in todos) e.Publicado = true;
        await db.SaveChangesAsync();
        return Results.Ok(new { updated = todos.Count });
    });
}

// ---------- 5) Migrar SIEMPRE + (opcional) SEED ----------
static bool GetFlag(IConfiguration cfg, string key, bool def = false)
    => (Environment.GetEnvironmentVariable(key)
        ?? cfg[key]
        ?? (def ? "true" : "false"))
       .Equals("true", StringComparison.OrdinalIgnoreCase);

async Task ApplyMigrationsAndSeedAsync()
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");

    try
    {
        var db = scope.ServiceProvider.GetRequiredService<EspectaculosDbContext>();

        // 1) Migraciones SIEMPRE
        logger.LogInformation("Aplicando migraciones...");
        await db.Database.MigrateAsync();
        logger.LogInformation("Migraciones aplicadas.");

        // 2) Seed sólo si lo pediste explícitamente (AUTO_SEED=true)
        var doSeed = GetFlag(config, "AUTO_SEED", false);
        if (doSeed)
        {
            logger.LogInformation("SEED solicitado → Reset + carga completa.");
            var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
            await seeder.SeedAsync(forceResetAndLoadAll: true);
            logger.LogInformation("SEED finalizado.");
        }
        else
        {
            logger.LogInformation("AUTO_SEED=false → seed omitido.");
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error durante migración/seed");
        throw;
    }
}

await ApplyMigrationsAndSeedAsync();

if (enableAdmin)
{
    app.MapGet("/admin/debug-event/{id:guid}", async (Guid id, IUnitOfWork uow) => {
        var ev = await uow.Eventos.GetByIdAsync(id);
        return Results.Ok(new { ev?.Id, ev?.Disponible, ev?.Fecha, Kind = ev?.Fecha.Kind.ToString() });
    });
}

app.Run();