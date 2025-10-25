using MediatR;

namespace Espectaculos.Application.Roles.Commands.DeleteRol;

public class DeleteRolCommand : IRequest<Guid>
{
    public Guid RolId { get; set; }
}