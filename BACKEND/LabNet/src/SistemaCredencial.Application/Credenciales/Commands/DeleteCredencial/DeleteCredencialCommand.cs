using MediatR;

namespace Espectaculos.Application.Credenciales.Commands.DeleteCredencial;

public class DeleteCredencialCommand : IRequest<Guid>
{
    public Guid CredencialId { get; set; }
}