using Espectaculos.Application.Notificaciones.Commands.CreateNotificacion;
using Espectaculos.Application.Notificaciones.Commands.PublishNotificacion;
using Espectaculos.Application.Notificaciones.Queries.ListNotificaciones;
using Espectaculos.Application.Notificaciones.Queries.GetNotificacionById;
using Espectaculos.Application.Notificaciones.Dtos;
using Espectaculos.Application.Abstractions;
using MediatR;

namespace Espectaculos.WebApi.Endpoints;

public static class NotificacionesEndpoints
{
    public static void MapNotificacionesEndpoints(this IEndpointRouteBuilder api)
    {
        api.MapGet("/notificaciones", async (bool onlyActive, IMediator mediator) =>
        {
            var items = await mediator.Send(new ListNotificacionesQuery(onlyActive));
            return Results.Ok(items);
        });

        api.MapGet("/notificaciones/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var item = await mediator.Send(new GetNotificacionByIdQuery(id));
            return item is null ? Results.NotFound() : Results.Ok(item);
        });

        api.MapPost("/notificaciones", async (CreateNotificacionDto dto, IMediator mediator) =>
        {
            var cmd = new CreateNotificacionCommand(dto.Tipo, dto.Titulo, dto.Cuerpo, dto.ProgramadaParaUtc, dto.Audiencia);
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
