using Espectaculos.Application.Abstractions;
using Espectaculos.Application.Notificaciones.Dtos;
using MediatR;

namespace Espectaculos.Application.Notificaciones.Queries.GetNotificacionById;

public class GetNotificacionByIdHandler : IRequestHandler<GetNotificacionByIdQuery, NotificacionDto?>
{
    private readonly IUnitOfWork _uow;

    public GetNotificacionByIdHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<NotificacionDto?> Handle(GetNotificacionByIdQuery request, CancellationToken cancellationToken)
    {
        var n = await _uow.Notificaciones.GetByIdAsync(request.Id, cancellationToken);
        if (n is null) return null;
        return new NotificacionDto(
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
        );
    }
}
