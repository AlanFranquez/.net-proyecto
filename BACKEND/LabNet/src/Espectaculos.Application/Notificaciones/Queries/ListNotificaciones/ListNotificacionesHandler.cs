// Espectaculos.Application/Notificaciones/Queries/ListNotificaciones/ListNotificacionesHandler.cs
using Espectaculos.Application.Abstractions;
using Espectaculos.Application.Notificaciones.Dtos;
using Espectaculos.Domain.Enums;
using MediatR;

namespace Espectaculos.Application.Notificaciones.Queries.ListNotificaciones;

public class ListNotificacionesHandler 
    : IRequestHandler<ListNotificacionesQuery, IEnumerable<NotificacionDto>>
{
    private readonly IUnitOfWork _uow;

    public ListNotificacionesHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<NotificacionDto>> Handle(
        ListNotificacionesQuery request,
        CancellationToken cancellationToken)
    {
        var usuarioId = request.UsuarioId;

        IEnumerable<Domain.Entities.Notificacion> list;

        if (usuarioId.HasValue)
        {
            if (request.OnlyActive)
            {
                list = await _uow.Notificaciones.ListAsync(
                    n =>
                        n.UsuarioId == usuarioId.Value &&
                        (n.Estado == NotificacionEstado.Programada ||
                         n.Estado == NotificacionEstado.Publicada),
                    cancellationToken);
            }
            else
            {
                list = await _uow.Notificaciones.ListAsync(
                    n => n.UsuarioId == usuarioId.Value,
                    cancellationToken);
            }
        }
        else
        {
            list = request.OnlyActive
                ? await _uow.Notificaciones.ListAsync(
                    n => n.Estado == NotificacionEstado.Programada ||
                         n.Estado == NotificacionEstado.Publicada,
                    cancellationToken)
                : await _uow.Notificaciones.ListAsync(cancellationToken);
        }

        return list.Select(n => new NotificacionDto(
            n.NotificacionId,
            n.UsuarioId,              // ⬅⬅⬅ NUEVO
            n.Tipo,
            n.Titulo,
            n.Cuerpo,
            n.ProgramadaParaUtc,
            n.Estado.ToString(),
            n.Canales,
            n.Metadatos,
            n.CreadoEnUtc,
            n.Audiencia
        ));
    }
}
