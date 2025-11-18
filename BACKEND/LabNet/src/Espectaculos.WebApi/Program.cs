using System.Reflection;
using System.Text.Json.Serialization;
using Amazon.CognitoIdentityProvider;
using DotNetEnv;
using Amazon.Runtime;

using Espectaculos.Application.Abstractions;
using Espectaculos.Application.Abstractions.Repositories;
using Espectaculos.Application.Credenciales.Commands.CreateCredencial;
using Espectaculos.Application.Credenciales.Commands.DeleteCredencial;
using Espectaculos.Application.Credenciales.Commands.UpdateCredencial;
using Espectaculos.Application.Dispositivos.Commands.CreateDispositivo;
using Espectaculos.Application.Dispositivos.Commands.DeleteDispositivo;
using Espectaculos.Application.Dispositivos.Commands.UpdateDispositivo;
using Espectaculos.Application.Espacios.Commands.CreateEspacio;
using Espectaculos.Application.Espacios.Commands.DeleteEspacio;
using Espectaculos.Application.Espacios.Commands.UpdateEspacio;
using Espectaculos.Application.EventoAcceso.Commands.CreateEvento;
using Espectaculos.Application.EventoAcceso.Commands.DeleteEvento;
using Espectaculos.Application.EventoAcceso.Commands.UpdateEvento;
using Espectaculos.Application.ReglaDeAcceso.Commands.CreateReglaDeAcceso;
using Espectaculos.Application.ReglaDeAcceso.Commands.DeleteReglaDeAcceso;
using Espectaculos.Application.ReglaDeAcceso.Commands.UpdateReglaDeAcceso;
using Espectaculos.Application.Roles.Commands.CreateRol;
using Espectaculos.Application.Roles.Commands.DeleteRol;
using Espectaculos.Application.Roles.Commands.UpdateRol;
using Espectaculos.Application.Settings;
using Espectaculos.Application.Sincronizaciones.Commands.CreateSincronizacion;
using Espectaculos.Application.Sincronizaciones.Commands.DeleteSincronizacion;
using Espectaculos.Application.Sincronizaciones.Commands.UpdateSincronizacion;
using Espectaculos.Application.Usuarios.Commands.CreateUsuario;
using Espectaculos.Application.Usuarios.Commands.DeleteUsuario;
using Espectaculos.Application.Usuarios.Commands.UpdateUsuario;
using Espectaculos.Application.Services;      
using Espectaculos.Infrastructure.Persistence;
using Espectaculos.Infrastructure.Persistence.Interceptors;
using Espectaculos.Infrastructure.Persistence.Seed;
using Espectaculos.Infrastructure.RealTime;    
using Espectaculos.Infrastructure.Repositories;
using Espectaculos.WebApi.Endpoints;
using Espectaculos.WebApi.Health;
using Espectaculos.WebApi.Options;
using Espectaculos.WebApi.Security;
using Espectaculos.WebApi.SerilogConfig;
using Espectaculos.WebApi.Utils;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

var envPath = Path.Combine(Directory.GetCurrentDirectory(), "../../.env");

Console.WriteLine($"[AWS DEBUG] Path: {envPath}");
if (File.Exists(envPath))
{
    Env.Load(envPath);
}
else
{
    Env.Load();
}

var builder = WebApplication.CreateBuilder(args);

// ---------- Logging (Serilog) ----------
builder.AddSerilogLogging();

// ---------- Configuración base ----------
var config = builder.Configuration;

builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables("APP__")
    .AddEnvironmentVariables();

// ---------- Connection string ----------
string connectionString =
    config.GetConnectionString("Default")
    ?? config["ConnectionStrings__Default"]
    ?? "Host=localhost;Port=5432;Database=espectaculosdb;Username=postgres;Password=postgres";

Console.WriteLine($"[DB DEBUG] Path: {connectionString}");

// ---------- AWS Cognito ----------
builder.Services.Configure<AwsCognitoSettings>(builder.Configuration.GetSection("AWS:Cognito"));
var cognitoSection = builder.Configuration.GetSection("AWS:Cognito");
var cognitoSettings = cognitoSection.Get<AwsCognitoSettings>();

if (cognitoSettings == null)
    throw new InvalidOperationException("Falta la sección AWS:Cognito en la configuración.");

if (string.IsNullOrWhiteSpace(cognitoSettings.Region) ||
    string.IsNullOrWhiteSpace(cognitoSettings.UserPoolId) ||
    string.IsNullOrWhiteSpace(cognitoSettings.ClientId))
{
    throw new InvalidOperationException("AWS:Cognito Region, UserPoolId y ClientId deben estar configurados.");
}

var authority = $"https://cognito-idp.{cognitoSettings.Region}.amazonaws.com/{cognitoSettings.UserPoolId}";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = authority;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = authority,
            ValidateAudience = true,
            ValidAudience = cognitoSettings.ClientId,
            ValidateLifetime = true
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                if (string.IsNullOrEmpty(ctx.Request.Headers["Authorization"]))
                {
                    if (ctx.Request.Cookies.TryGetValue("espectaculos_session", out var tokenFromCookie))
                    {
                        ctx.Token = tokenFromCookie;
                    }
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Log de diagnóstico en Development
if (builder.Environment.IsDevelopment())
{
    var secretPresent = string.IsNullOrWhiteSpace(builder.Configuration["ValidationTokens:Secret"]) ? "absent" : "present";
    Log.Information("Startup diagnostic: ValidationTokens:Secret {Status} in configuration (Development).", secretPresent);
}

// ---------- Servicios básicos ----------
builder.Services.AddEndpointsApiExplorer();

// ---------- OpenTelemetry ----------
var serviceName = "Espectaculos.WebApi";
var serviceVersion = "1.0.0";

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(serviceName: serviceName, serviceVersion: serviceVersion))
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation(o =>
        {
            o.RecordException = true;
            o.EnrichWithHttpRequest = (activity, httpReq) =>
            {
                if (httpReq.HttpContext.Items.TryGetValue("CorrelationId", out var cid) && cid is not null)
                {
                    activity.SetTag("correlation.id", cid.ToString());
                }
                else if (httpReq.Headers.TryGetValue(CorrelationIdMiddleware.HeaderName, out var v))
                {
                    activity.SetTag("correlation.id", v.ToString());
                }
            };
            o.Filter = httpContext =>
                !httpContext.Request.Path.StartsWithSegments("/health") &&
                !httpContext.Request.Path.StartsWithSegments("/swagger");
        })
        .AddHttpClientInstrumentation()
        .AddOtlpExporter())
    .WithMetrics(m => m
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddOtlpExporter());

// ---------- Swagger ----------
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Espectáculos - Demo API",
        Version = "v1",
        Description = "API pública para la demo."
    });

    o.TagActionsBy(apiDesc =>
    {
        var relativePath = apiDesc.RelativePath ?? string.Empty;
        var segments = relativePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length >= 2 && segments[0].Equals("api", StringComparison.OrdinalIgnoreCase))
        {
            var category = segments[1];
            return new[] { char.ToUpper(category[0]) + category[1..] };
        }

        if (segments.Length >= 1)
            return new[] { segments[0] };

        return new[] { "General" };
    });

    o.UseInlineDefinitionsForEnums();
});

// ---------- JSON / enums ----------
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(opts =>
{
    opts.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    opts.SerializerOptions.Converters.Add(new Espectaculos.WebApi.Json.CaseInsensitiveEnumConverterFactory());
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// ---------- ValidationTokens (options) ----------
var validationSection = builder.Configuration.GetSection("ValidationTokens");
builder.Services
    .AddOptions<ValidationTokenOptions>()
    .Bind(validationSection)
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

// ---------- EF Core + Npgsql ----------
builder.Services.AddSingleton<AuditableEntitySaveChangesInterceptor>();

var npgsqlDataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
npgsqlDataSourceBuilder.EnableDynamicJson();
var npgsqlDataSource = npgsqlDataSourceBuilder.Build();

builder.Services.AddDbContext<EspectaculosDbContext>(options =>
{
    options.UseNpgsql(npgsqlDataSource);
});

// ---------- Health checks ----------
builder.Services.AddHealthChecks();
builder.Services.AddPostgresHealthChecks(connectionString);

// ---------- CORS (dev) ----------
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
             .AllowCredentials());
    });
}

// ---------- SignalR ----------
builder.Services.AddSignalR();

// ---------- FluentValidation (validators explícitos) ----------
builder.Services.AddScoped<IValidator<CreateEspacioCommand>, CreateEspacioValidator>();
builder.Services.AddScoped<IValidator<UpdateEspacioCommand>, UpdateEspacioValidator>();
builder.Services.AddScoped<IValidator<DeleteEspacioCommand>, DeleteEspacioValidator>();
builder.Services.AddScoped<IValidator<CreateReglaCommand>, CreateReglaValidator>();
builder.Services.AddScoped<IValidator<UpdateReglaCommand>, UpdateReglaValidator>();
builder.Services.AddScoped<IValidator<DeleteReglaCommand>, DeleteReglaValidator>();
builder.Services.AddScoped<IValidator<CreateEventoCommand>, CreateEventoValidator>();
builder.Services.AddScoped<IValidator<UpdateEventoCommand>, UpdateEventoValidator>();
builder.Services.AddScoped<IValidator<DeleteEventoCommand>, DeleteEventoValidator>();
builder.Services.AddScoped<IValidator<CreateCredencialCommand>, CreateCredencialValidator>();
builder.Services.AddScoped<IValidator<UpdateCredencialCommand>, UpdateCredencialValidator>();
builder.Services.AddScoped<IValidator<DeleteCredencialCommand>, DeleteCredencialValidator>();
builder.Services.AddScoped<IValidator<CreateRolCommand>, CreateRolValidator>();
builder.Services.AddScoped<IValidator<UpdateRolCommand>, UpdateRolValidator>();
builder.Services.AddScoped<IValidator<DeleteRolCommand>, DeleteRolValidator>();
builder.Services.AddScoped<IValidator<CreateUsuarioCommand>, CreateUsuarioValidator>();
builder.Services.AddScoped<IValidator<UpdateUsuarioCommand>, UpdateUsuarioValidator>();
builder.Services.AddScoped<IValidator<DeleteUsuarioCommand>, DeleteUsuarioValidator>();
builder.Services.AddScoped<IValidator<CreateSincronizacionCommand>, CreateSincronizacionValidator>();
builder.Services.AddScoped<IValidator<UpdateSincronizacionCommand>, UpdateSincronizacionValidator>();
builder.Services.AddScoped<IValidator<DeleteSincronizacionCommand>, DeleteSincronizacionValidator>();
builder.Services.AddScoped<IValidator<CreateDispositivoCommand>, CreateDispositivoValidator>();
builder.Services.AddScoped<IValidator<UpdateDispositivoCommand>, UpdateDispositivoValidator>();
builder.Services.AddScoped<IValidator<DeleteDispositivoCommand>, DeleteDispositivoValidator>();


builder.Services.AddValidatorsFromAssembly(Assembly.Load("Espectaculos.Application"));

// ---------- MediatR ----------
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.Load("Espectaculos.Application"));
});

// ---------- Repositorios + UnitOfWork ----------
builder.Services.AddScoped<IEspacioRepository, EspacioRepository>();
builder.Services.AddScoped<IReglaDeAccesoRepository, ReglaDeAccesoRepository>();
builder.Services.AddScoped<IBeneficioRepository, BeneficioRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IBeneficioUsuarioRepository, BeneficioUsuarioRepository>();
builder.Services.AddScoped<IBeneficioEspacioRepository, BeneficioEspacioRepository>();
builder.Services.AddScoped<ICanjeRepository, CanjeRepository>();
builder.Services.AddScoped<IEventoAccesoRepository, EventoAccesoRepository>();
builder.Services.AddScoped<ICredencialRepository, CredencialRepository>();
builder.Services.AddScoped<INotificacionRepository, NotificacionRepository>();
builder.Services.AddScoped<IRolRepository, RolRepository>();
builder.Services.AddScoped<ISincronizacionRepository, SincronizacionRepository>();
builder.Services.AddScoped<IDispositivoRepository, DispositivoRepository>();
builder.Services.AddScoped<INovedadRepository, NovedadRepository>();

// Notificador realtime de accesos (SignalR)
builder.Services.AddSingleton<IAccesosRealtimeNotifier, AccesosSignalRNotifier>();

builder.Services.AddSingleton<INotificationSender, Espectaculos.Infrastructure.Notifications.LoggingNotificationSender>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ---------- Amazon Cognito client + servicio ----------

builder.Services.AddSingleton<IAmazonCognitoIdentityProvider>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var opts = sp.GetRequiredService<IOptions<AwsCognitoSettings>>().Value;
    var logger = sp.GetRequiredService<ILogger<Program>>();

    var regionName = string.IsNullOrWhiteSpace(opts.Region) ? "us-east-1" : opts.Region;
    var regionEndpoint = Amazon.RegionEndpoint.GetBySystemName(regionName);

    // Leer claves desde config o env
    var accessKey = cfg["AWS:AccessKeyId"] ?? Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
    var secretKey = cfg["AWS:SecretAccessKey"] ?? Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");

    logger.LogInformation("AWS DEBUG: Region={Region}, AccessKeyPresent={HasAccessKey}",
        regionName,
        !string.IsNullOrEmpty(accessKey));

    if (!string.IsNullOrEmpty(accessKey) && !string.IsNullOrEmpty(secretKey))
    {
        // ✅ IAM User “fijo”: sin session token
        var creds = new BasicAWSCredentials(accessKey, secretKey);
        return new AmazonCognitoIdentityProviderClient(creds, new AmazonCognitoIdentityProviderConfig
        {
            RegionEndpoint = regionEndpoint
        });
    }

    // Fallback: cadena por defecto (si algún día querés usar SSO/perfil)
    logger.LogWarning("Using fallback AWS credential chain (no explicit access/secret key found).");
    return new AmazonCognitoIdentityProviderClient(regionEndpoint);
});

builder.Services.AddScoped<ICognitoService, CognitoService>();

// ---------- Seeder, routing, métricas ----------
builder.Services.AddScoped<DbSeeder>();
builder.Services.AddRouting();
builder.Services.AddHostedService<SincronizacionesMetrics>();

var app = builder.Build();

// ---------- Archivos estáticos ----------
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".dat"] = "application/octet-stream";
provider.Mappings[".wasm"] = "application/wasm";
provider.Mappings[".br"] = "application/octet-stream";

app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = provider });

// ---------- Middleware base ----------
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();

if (isDev)
    app.UseCors("DevCors");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Espectaculos API v1");
    c.RoutePrefix = "swagger";
});

// ---------- Auth ----------
app.UseAuthentication();
app.UseAuthorization();

// ---------- API bajo /api ----------
var api = app.MapGroup("/api");

api.MapEspaciosEndpoints();
api.MapReglasDeAccesoEndpoints();
api.MapBeneficiosEndpoints();
api.MapCanjesEndpoints();
api.MapEventosAccesosEndpoints();
api.MapNotificacionesEndpoints();
api.MapCredencialesEndpoints();
api.MapRolesEndpoints();
api.MapUsuariosEndpoints();
api.MapSincronizacionEndpoints();
api.MapDispositivosEndpoints();

// ---------- SignalR hubs ----------
app.MapHub<AccesosHub>("/hubs/accesos");

// ---------- Health ----------
app.MapHealthChecks("/health");
api.MapHealthChecks("/health");

// ---------- Flags admin / seed ----------
var enableAdmin = (Environment.GetEnvironmentVariable("DEMO_ENABLE_ADMIN") ?? config["DEMO_ENABLE_ADMIN"] ?? "false")
    .Equals("true", StringComparison.OrdinalIgnoreCase);

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

        logger.LogInformation("Aplicando migraciones...");
        await db.Database.MigrateAsync();
        logger.LogInformation("Migraciones aplicadas.");

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

app.Run();
