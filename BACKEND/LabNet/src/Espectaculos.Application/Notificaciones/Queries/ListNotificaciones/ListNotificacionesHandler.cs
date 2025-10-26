using Espectaculos.Application.Abstractions;
using Espectaculos.Application.Notificaciones.Dtos;
using Espectaculos.Domain.Enums;
using MediatR;

namespace Espectaculos.Application.Notificaciones.Queries.ListNotificaciones;

public class ListNotificacionesHandler : IRequestHandler<ListNotificacionesQuery, IEnumerable<NotificacionDto>>
{
    private readonly IUnitOfWork _uow;

    public ListNotificacionesHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<NotificacionDto>> Handle(ListNotificacionesQuery request, CancellationToken cancellationToken)
    {
        var list = request.OnlyActive
            ? await _uow.Notificaciones.ListAsync(n => n.Estado == NotificacionEstado.Programada || n.Estado == NotificacionEstado.Publicada, cancellationToken)
            : await _uow.Notificaciones.ListAsync(cancellationToken);

        return list.Select(n => new NotificacionDto(
            n.NotificacionId,
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
