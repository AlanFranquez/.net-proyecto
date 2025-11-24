using System.Security.Claims;
using System.Text;
using Amazon.CognitoIdentityProvider.Model;
using Espectaculos.Application.Espacios.Commands.CreateEspacio;
using Espectaculos.Application.Espacios.Commands.DeleteEspacio;
using Espectaculos.Application.Espacios.Commands.UpdateEspacio;
using Espectaculos.Application.Espacios.Queries.ListarEspacios;
using Espectaculos.Application.Services;
using Espectaculos.Application.Usuarios.Commands.CreateUsuario;
using Espectaculos.Application.Usuarios.Commands.DeleteUsuario;
using Espectaculos.Application.Usuarios.Commands.LoginUsuario;
using Espectaculos.Application.Usuarios.Commands.UpdateUsuario;
using Espectaculos.Application.Usuarios.Queries.GetUsuarioByEmail;
using Espectaculos.Application.Usuarios.Queries.ListarUsuarios;
using Espectaculos.Application.Dispositivos.Queries.ListarDispositivos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Espectaculos.WebApi.Services;

namespace Espectaculos.WebApi.Endpoints;

public class CreateUsuarioDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public IEnumerable<Guid>? RolesIDs { get; set; }
}

// 👇 NUEVO DTO para cambio de contraseña
public class CambiarPasswordDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public static class UsuariosEndpoints
{
    public static IEndpointRouteBuilder MapUsuariosEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("usuarios");

        group.MapGet("/", async ([FromServices] IMediator mediator) =>
        {
            var query = new ListarUsuariosQuery();
            var usuarios = await mediator.Send(query);

            return Results.Ok(usuarios);
        }).WithName("ListarUsuarios").WithTags("Usuarios").WithOpenApi();
        
        
        group.MapDelete("/", async ([FromBody] DeleteUsuarioCommand command, [FromServices]IMediator mediator) =>
        {
            await mediator.Send(command);
            return Results.NoContent();
        }).WithName("EliminarUsuario").WithTags("Usuarios");

        group.MapPost("/registro", async (
    [FromBody] CreateUsuarioDto dto,
    HttpContext http,
    IMediator mediator,
    ICognitoService cognito) =>
{
    Serilog.Log.Information("DTO recibido: {@Dto}", dto);

    var cmd = new CreateUsuarioCommand
    {
        Nombre = dto.Nombre,
        Apellido = dto.Apellido,
        Documento = dto.Documento,
        Email = dto.Email,
        Password = dto.Password,
        RolesIDs = dto.RolesIDs?.ToList()
    };

    try
    {
        var id = await mediator.Send(cmd);
        var idToken = await cognito.LoginAsync(dto.Email, dto.Password);

        
        http.Response.Cookies.Append("espectaculos_session", idToken,
            new CookieOptions
            {
                Nombre = dto.Nombre,
                Apellido = dto.Apellido,
                Documento = dto.Documento,
                Email = dto.Email,
                Password = dto.Password,
                RolesIDs = dto.RolesIDs?.ToList()
            };

            try
            {
                var id = await mediator.Send(cmd);
                var idToken = await cognito.LoginAsync(dto.Email, dto.Password);

                // Publicar un mensaje en RabbitMQ (deshabilitado)
                // rabbitMqService.SendMessage($"Usuario registrado: {dto.Email}");

                http.Response.Cookies.Append("espectaculos_session", idToken,
                    new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = false,
                        SameSite = SameSiteMode.Lax,
                        Expires = DateTimeOffset.UtcNow.AddHours(8)
                    });

                return Results.Ok(id);
            }
            catch (FluentValidation.ValidationException vf)
            {
                var errors = vf.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                // rabbitMqService.SendMessage($"Error al registrar usuario: {dto.Email}. Detalle: {vf.Message}");
                return Results.BadRequest(new { message = "Validation failed", errors });

            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error en registro");
                // rabbitMqService.SendMessage($"Error al registrar usuario: {dto.Email}. Detalle: {ex.Message}");
        
                return Results.StatusCode(500);
            }
        })
        .WithName("CrearUsuario")
        .WithTags("Usuarios");

group.MapPost("/test-dlq", async ([FromServices] RabbitMqService rabbit) =>
{

    return Results.Ok(new
    {
        status = "Mensaje enviado a la DLQ",
        cola = "usuarios-dlq"
    });
})
.WithName("EnviarDLQ")
.WithTags("Usuarios");

        group.MapPost("/login", async (HttpContext http, LoginUsuarioCommand command, ICognitoService cognito) =>
        {
            try
            {
                var idToken = await cognito.LoginAsync(command.Email, command.Password, CancellationToken.None);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddHours(8)
                };

                http.Response.Cookies.Append("espectaculos_session", idToken, cookieOptions);
                return Results.Ok(new { message = "ok" });
            }
            catch (ArgumentNullException) { return Results.BadRequest(); }
            catch (NotAuthorizedException) { return Results.Unauthorized(); }
            catch (UserNotFoundException) { return Results.NotFound(); }
            catch (Exception ex) { Serilog.Log.Error(ex, "Error en login"); return Results.StatusCode(500); }
            
            //var id = await mediator.Send(command);
            //return Results.Ok(id);
        }).WithName("IniciarSesion").WithTags("Usuarios");
        
        group.MapGet("/me", [Authorize] async (HttpContext http, ClaimsPrincipal user, IMediator mediator, CancellationToken ct) =>
            {
                var email = user.FindFirstValue(ClaimTypes.Email);
                if (string.IsNullOrEmpty(email))
                    return Results.Unauthorized();

                var query = new GetUsuarioByEmailQuery(email);
                var usuario = await mediator.Send(query, ct);

                if (usuario is null)
                    return Results.NotFound("Usuario no encontrado en base de datos local.");

                // ✅ validar dispositivo actual por huella
                var browserId = http.Request.Headers["X-Device-Id"].FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(browserId))
                {
                    var dispositivos = await mediator.Send(new ListarDispositivosQuery(), ct);

                    // usuario.UsuarioId es Guid seguro (DTO o entidad)
                    var usuarioId = usuario.UsuarioId;
                    var current = dispositivos.FirstOrDefault(d =>
                        d.UsuarioId == usuarioId &&
                        d.HuellaDispositivo == browserId
                    );

                    if (current != null)
                    {
                        var estadoStr = current.Estado.ToString().ToLowerInvariant();

                        var revoked =
                            estadoStr.Contains("revoc") ||
                            estadoStr.Contains("baja") ||
                            estadoStr.Contains("inactiv");

                        if (revoked)
                        {
                            http.Response.Cookies.Delete("espectaculos_session");
                            return Results.Unauthorized();
                        }
                    }
                }

                return Results.Ok(usuario);
            })
            .WithName("GetUsuarioActual")
            .WithTags("Usuarios")
            .WithOpenApi();


       
        
        group.MapPut("/{id:guid}", async (Guid id, UpdateUsuarioCommand cmd, IMediator mediator) =>
        {
            cmd.UsuarioId = id; // Asegura que el id de la ruta se copie al comando
            var updatedId = await mediator.Send(cmd);
            return Results.Ok(updatedId);
        })
        .WithName("EditarUsuario")
        .WithTags("Usuarios");

        group.MapPost("/logout", (HttpContext http) =>
        {
            http.Response.Cookies.Delete("espectaculos_session");
            return Results.Ok(new { message = "logged_out" });
        })
        .WithName("LogoutUsuario")
        .WithTags("Usuarios");

        // 👇 NUEVO: endpoint para cambiar contraseña
        group.MapPost("/cambiar-password", [Authorize] async (
                ClaimsPrincipal user,
                [FromBody] CambiarPasswordDto dto,
                ICognitoService cognito,
                CancellationToken ct) =>
        {
            var email = user.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrWhiteSpace(email))
                return Results.Unauthorized();

            if (string.IsNullOrWhiteSpace(dto.CurrentPassword) ||
                string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                return Results.BadRequest(new { error = "CurrentPassword y NewPassword son obligatorios." });
            }

            try
            {
                await cognito.ChangePasswordAsync(email, dto.CurrentPassword, dto.NewPassword, ct);
                return Results.NoContent();
            }
            catch (NotAuthorizedException)
            {
                return Results.BadRequest(new { error = "Contraseña actual incorrecta o no autorizado." });
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error en cambiar-password");
                return Results.StatusCode(500);
            }
        })
        .WithName("CambiarPassword")
        .WithTags("Usuarios");

        group.MapPut("/bucleInfinito", async ([FromServices] RabbitMqService rabbitMqService) =>
        {
            int requestCount = 0;
            const int maxRequests = 200;

            while (true)
            {
                if (requestCount >= maxRequests)
                {
                    rabbitMqService.SendToDlq("usuarios-dlq", "Se alcanzó el límite de peticiones en el bucle infinito.");
                    Console.WriteLine("Mensaje enviado a la DLQ después de alcanzar el límite de peticiones.");
                    break;
                }

                rabbitMqService.SendMessage("usuario", $"Mensaje {requestCount + 1} enviado desde el bucle infinito.");
                Console.WriteLine($"Publicando mensaje: Mensaje {requestCount + 1}");

                requestCount++;
            }

            return Results.Ok(new { status = "Bucle terminado", totalRequests = requestCount });
        })
        .WithName("BucleInfinito")
        .WithTags("Usuarios");
        

#if DEMO_ENABLE_ADMIN
        group.MapPost("", async (CreateEventoCommand command, IUnitOfWork uow, IValidator<CreateEventoCommand> validator) =>
        {
            var handler = new CreateEventoHandler(uow, validator);
            var id = await handler.HandleAsync(command);
            return Results.Created($"/api/eventos/{id}", new { id });
        })
        .WithName("CreateEvento")
        .WithOpenApi();
#endif

#if DEMO_ENABLE_ADMIN
        // Entradas - Validación y Canje (se registran aquí para evitar tocar Program.cs)
        var entradasGroup = endpoints.MapGroup("entradas");

        entradasGroup.MapGet("/validar", async (string token, IConfiguration config, IUnitOfWork uow, CancellationToken ct) =>
        {
            var secret = config["ValidationTokens:Secret"];
            var validation = ValidationTokenHelper.ValidateToken(token, secret);
            if (validation.Status == TokenValidationStatus.InvalidToken)
                return Results.BadRequest(new { status = "invalid_token", detail = validation.Detail });
            if (validation.Status == TokenValidationStatus.Expired)
                return Results.BadRequest(new { status = "expired", detail = validation.Detail });

            var orderId = validation.OrderId!.Value;
            // Repositorio debe incluir Items
            var orden = await uow.Ordenes.GetByIdAsync(orderId, ct);
            if (orden is null)
                return Results.NotFound(new { status = "not_found" });

            if (orden.RedeemedAtUtc.HasValue)
                return Results.Conflict(new { status = "used" });

            // Validar estado de eventos asociados
            var eventoIds = orden.Items.Select(i => i.EventoId).Distinct().ToList();
            foreach (var eid in eventoIds)
            {
                var ev = await uow.Eventos.GetByIdAsync(eid, ct);
                var stockTotal = (ev?.Entradas?.Sum(x => (int?)x.StockDisponible) ?? 0);
                if (ev is null || ev.Fecha <= DateTime.UtcNow || stockTotal <= 0)
                    return Results.Conflict(new { status = "event_closed" });
            }

            var cantidadTotal = orden.Items.Sum(i => i.Cantidad);
            return Results.Ok(new
            {
                status = "ok",
                orderId = orden.Id,
                emailComprador = orden.EmailComprador,
                eventoIds,
                cantidadTotal
            });
        })
        .WithName("ValidarEntrada")
        .WithOpenApi();

        entradasGroup.MapPost("/canjear", async (HttpContext http, string token, IConfiguration config, IUnitOfWork uow, CancellationToken ct) =>
        {
            var secret = config["ValidationTokens:Secret"];
            var validation = ValidationTokenHelper.ValidateToken(token, secret);
            if (validation.Status == TokenValidationStatus.InvalidToken)
                return Results.BadRequest(new { status = "invalid_token", detail = validation.Detail });
            if (validation.Status == TokenValidationStatus.Expired)
                return Results.BadRequest(new { status = "expired", detail = validation.Detail });

            var orderId = validation.OrderId!.Value;
            var orden = await uow.Ordenes.GetByIdAsync(orderId, ct);
            if (orden is null)
                return Results.NotFound(new { status = "not_found" });

            if (orden.RedeemedAtUtc.HasValue)
                return Results.Conflict(new { status = "used" });

            // Validar estado de eventos asociados
            var eventoIds = orden.Items.Select(i => i.EventoId).Distinct().ToList();
            foreach (var eid in eventoIds)
            {
                var ev = await uow.Eventos.GetByIdAsync(eid, ct);
                var stockTotal = (ev?.Entradas?.Sum(x => (int?)x.StockDisponible) ?? 0);
                if (ev is null || ev.Fecha <= DateTime.UtcNow || stockTotal <= 0)
                    return Results.Conflict(new { status = "event_closed" });
            }

            // Marcar canje
            orden.RedeemedAtUtc = DateTime.UtcNow;
            if (http.Request.Headers.TryGetValue("X-Device-Id", out var deviceVals))
            {
                var dev = deviceVals.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(dev))
                    orden.RedeemedBy = dev;
            }

            uow.Ordenes.Update(orden);
            await uow.SaveChangesAsync(ct);

            return Results.Ok(new { status = "redeemed" });
        })
        .WithName("CanjearEntrada")
        .WithOpenApi();
#endif

        return endpoints;
    }
}
