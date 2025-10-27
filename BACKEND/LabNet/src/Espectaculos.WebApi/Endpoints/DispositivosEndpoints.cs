using Espectaculos.Application.Dispositivos.Commands.CreateDispositivo;
using Espectaculos.Application.Dispositivos.Commands.DeleteDispositivo;
using Espectaculos.Application.Dispositivos.Commands.UpdateDispositivo;
using Espectaculos.Application.Dispositivos.Queries.ListarDispositivos;
using Espectaculos.Application.Notificaciones.Queries.ListarNotificacionesPorDispositivo;
using Espectaculos.Application.Notificaciones.Commands.MarcarNotificacionVisto;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Espectaculos.WebApi.Endpoints;

public static class DispositivosEndpoints
{
    public static IEndpointRouteBuilder MapDispositivosEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("dispositivos");

        group.MapGet("/", async ([FromServices] IMediator mediator) =>
        {
            var query = new ListarDispositivosQuery();
            var dispositivos = await mediator.Send(query);

            return Results.Ok(dispositivos);
        }).WithName("ListarDispositivos").WithTags("Dispositivos").WithOpenApi();
        
        
        group.MapDelete("/", async ([FromBody] DeleteDispositivoCommand command, [FromServices]IMediator mediator) =>
        {
            await mediator.Send(command);
            return Results.NoContent();
        }).WithName("EliminarDispositivo").WithTags("Dispositivos");
        
        group.MapPost("/", async (CreateDispositivoCommand command, IMediator mediator) =>
        {
            var id = await mediator.Send(command);
            return Results.Ok(id);
        }).WithName("CrearDispositivo").WithTags("Dispositivos");
        
        group.MapPut("/{id:guid}", async (Guid id, UpdateDispositivoCommand cmd, IMediator mediator) =>
        {
            cmd.DispositivoId = id; // Asegura que el id de la ruta se copie al comando
            var updatedId = await mediator.Send(cmd);
            return Results.Ok(updatedId);
        })
        .WithName("EditarDispositivo")
        .WithTags("Dispositivos");

        // Notificaciones del dispositivo
        group.MapGet("/{id:guid}/notificaciones", async (Guid id, [FromQuery] Espectaculos.Domain.Enums.NotificacionLecturaEstado? lectura, [FromQuery] int? take, [FromQuery] int? skip, IMediator mediator) =>
        {
            var query = new ListarNotificacionesPorDispositivoQuery(id, lectura, take, skip);
            var items = await mediator.Send(query);
            return Results.Ok(items);
        }).WithName("ListarNotificacionesPorDispositivo").WithTags("Dispositivos");

        group.MapPost("/{id:guid}/notificaciones/{notificacionId:guid}/visto", async (Guid id, Guid notificacionId, IMediator mediator) =>
        {
            var ok = await mediator.Send(new MarcarNotificacionVistoCommand(id, notificacionId));
            return ok ? Results.NoContent() : Results.NotFound();
        }).WithName("MarcarNotificacionComoVisto").WithTags("Dispositivos");
        

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
