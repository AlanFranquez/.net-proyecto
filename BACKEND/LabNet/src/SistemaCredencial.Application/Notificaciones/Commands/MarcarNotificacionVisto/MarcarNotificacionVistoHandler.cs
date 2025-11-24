using Espectaculos.Application.Abstractions;
using MediatR;

namespace Espectaculos.Application.Notificaciones.Commands.MarcarNotificacionVisto;

public class MarcarNotificacionVistoHandler : IRequestHandler<MarcarNotificacionVistoCommand, bool>
{
    private readonly IUnitOfWork _uow;
    public MarcarNotificacionVistoHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<bool> Handle(MarcarNotificacionVistoCommand request, CancellationToken cancellationToken)
    {
        var notif = await _uow.Notificaciones.GetByIdAsync(request.NotificacionId, cancellationToken);
        if (notif is null) return false;
        if (notif.DispositivoId != request.DispositivoId) return false;

        notif.LecturaEstado = Espectaculos.Domain.Enums.NotificacionLecturaEstado.Visto;
        _uow.Notificaciones.Update(notif);
        await _uow.SaveChangesAsync(cancellationToken);
        return true;
    }
}
