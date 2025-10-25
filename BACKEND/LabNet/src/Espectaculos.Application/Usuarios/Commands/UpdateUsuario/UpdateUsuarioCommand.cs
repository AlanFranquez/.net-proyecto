using Espectaculos.Domain.Enums;
using MediatR;

namespace Espectaculos.Application.Usuarios.Commands.UpdateUsuario
{
    public class UpdateUsuarioCommand : IRequest<Guid>
    {
        public Guid UsuarioId { get; set; }
        public string? Nombre { get; set; } = null;
        public string? Apellido { get; set; } = null;
        public string? Email { get; set; } = null;
        public string? Documento { get; set; } = null;
        public string? Password { get; set; } = null;
        public UsuarioEstado? Estado { get; set; } = null;
        public Guid? CredencialId { get; set; } = null;
        public IEnumerable<Guid>? RolesIDs { get; set; } = null;
        public IEnumerable<Guid>? BeneficiosIDs { get; set; } = null;
        public IEnumerable<Guid>? DispositivosIDs { get; set; } = null;
        public IEnumerable<Guid>? CanjesIDs { get; set; } = null;
    }
}