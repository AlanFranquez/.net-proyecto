using Espectaculos.Domain.Enums;
using MediatR;

namespace Espectaculos.Application.Usuarios.Commands.CreateUsuario
{
    public class CreateUsuarioCommand : IRequest<Guid>
    {
        public string Nombre { get; set; } = default!;
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Documento { get; set; }
        public string Password { get; set; }
        public IEnumerable<Guid>? RolesIDs { get; set; } = null;
    }
}