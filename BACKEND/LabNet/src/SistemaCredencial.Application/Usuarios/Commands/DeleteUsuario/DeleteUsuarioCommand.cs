using MediatR;

namespace Espectaculos.Application.Usuarios.Commands.DeleteUsuario;

public class DeleteUsuarioCommand : IRequest<Guid>
{
    public Guid UsuarioId { get; set; }
}