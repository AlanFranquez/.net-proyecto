using MediatR;

namespace Espectaculos.Application.Sincronizaciones.Commands.DeleteSincronizacion;

public class DeleteSincronizacionCommand : IRequest<Guid>
{
    public Guid SincronizacionId { get; set; }
}