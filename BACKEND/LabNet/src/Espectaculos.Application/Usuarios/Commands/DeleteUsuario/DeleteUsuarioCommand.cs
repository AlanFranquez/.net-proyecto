using MediatR;

namespace Espectaculos.Application.Usuarios.Commands.DeleteUsuario
{
    public record DeleteUsuarioCommand(
        Guid UsuarioId
    ) : IRequest;     
    
}


    
