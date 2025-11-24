using MediatR;

namespace Espectaculos.Application.Dispositivos.Commands.DeleteDispositivo;

public class DeleteDispositivoCommand : IRequest<Guid>
{
    public Guid DispositivoId { get; set; }
}