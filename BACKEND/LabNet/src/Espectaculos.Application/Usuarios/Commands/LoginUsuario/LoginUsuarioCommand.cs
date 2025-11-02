using Espectaculos.Domain.Enums;
using MediatR;

namespace Espectaculos.Application.Usuarios.Commands.LoginUsuario
{
    public class LoginUsuarioCommand : IRequest<string>
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}