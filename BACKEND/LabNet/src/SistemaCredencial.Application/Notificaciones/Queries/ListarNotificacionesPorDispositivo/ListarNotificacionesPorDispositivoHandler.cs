using Espectaculos.Application.Abstractions;
using MediatR;

namespace Espectaculos.Application.Notificaciones.Queries.ListarNotificacionesPorDispositivo;

public class ListarNotificacionesPorDispositivoHandler : IRequestHandler<ListarNotificacionesPorDispositivoQuery, IReadOnlyList<Espectaculos.Domain.Entities.Notificacion>>
{
    private readonly IUnitOfWork _uow;
    public ListarNotificacionesPorDispositivoHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<IReadOnlyList<Espectaculos.Domain.Entities.Notificacion>> Handle(ListarNotificacionesPorDispositivoQuery request, CancellationToken cancellationToken)
    {
        return await _uow.Notificaciones.ListByDispositivoAsync(request.DispositivoId, request.Lectura, request.Take, request.Skip, cancellationToken);
    }
}
