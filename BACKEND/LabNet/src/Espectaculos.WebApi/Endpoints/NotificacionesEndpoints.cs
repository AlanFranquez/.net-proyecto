// Espectaculos.WebApi/Endpoints/NotificacionesEndpoints.cs
using System.Security.Claims;
using Espectaculos.Application.Notificaciones.Commands.CreateNotificacion;
using Espectaculos.Application.Notificaciones.Commands.PublishNotificacion;
using Espectaculos.Application.Notificaciones.Queries.ListNotificaciones;
using Espectaculos.Application.Notificaciones.Queries.GetNotificacionById;
using Espectaculos.Application.Notificaciones.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace Espectaculos.WebApi.Endpoints;

public static class NotificacionesEndpoints
{
    public static void MapNotificacionesEndpoints(this IEndpointRouteBuilder api)
    {
        // --- NOTIFICACIONES DEL USUARIO ---
        // Permite pasar usuarioId (query), y si no viene, se toma del token.
        api.MapGet("/notificaciones/mias", async (
                bool onlyActive,
                Guid? usuarioId,
                ClaimsPrincipal user,
                IMediator mediator) =>
        {
            Guid effectiveUsuarioId;

            if (usuarioId.HasValue && usuarioId.Value != Guid.Empty)
            {
                // 1) Si viene por query, usamos ese (útil para Swagger/testing)
                effectiveUsuarioId = usuarioId.Value;
            }
            else
            {
                // 2) Si NO viene, intentamos obtenerlo del token
                var usuarioIdStr =
                    user.FindFirst("custom:UsuarioId")?.Value
                    ?? user.FindFirst("usuarioId")?.Value
                    ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrWhiteSpace(usuarioIdStr) ||
                    !Guid.TryParse(usuarioIdStr, out effectiveUsuarioId))
                {
                    return Results.Unauthorized();
                }
            }

            var items = await mediator.Send(
                new ListNotificacionesQuery(onlyActive, effectiveUsuarioId));

            return Results.Ok(items);
        }).RequireAuthorization();

        // --- LISTA GLOBAL (para backoffice/admin) ---
        api.MapGet("/notificaciones", async (bool onlyActive, IMediator mediator) =>
        {
            var items = await mediator.Send(new ListNotificacionesQuery(onlyActive, null));
            return Results.Ok(items);
        });

        api.MapGet("/notificaciones/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var item = await mediator.Send(new GetNotificacionByIdQuery(id));
            return item is null ? Results.NotFound() : Results.Ok(item);
        });

        api.MapPost("/notificaciones", async (CreateNotificacionDto dto, IMediator mediator) =>
        {
            var cmd = new CreateNotificacionCommand(
                dto.Tipo, dto.Titulo, dto.Cuerpo, dto.ProgramadaParaUtc, dto.Audiencia);
            var id = await mediator.Send(cmd);
            return Results.Created($"/notificaciones/{id}", new { id });
        });

        api.MapPost("/notificaciones/{id:guid}/publicar", async (Guid id, DateTime? programadaParaUtc, IMediator mediator) =>
        {
            var ok = await mediator.Send(new PublishNotificacionCommand(id, programadaParaUtc));
            return ok ? Results.Ok(new { id, published = true }) : Results.NotFound();
        });
    }
}
