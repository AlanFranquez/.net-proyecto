using Espectaculos.Domain.Enums;
using MediatR;

namespace Espectaculos.Application.Usuarios.Commands.UpdateUsuario;

public class UpdateUsuarioCommand : IRequest<Guid>
{
    public Guid UsuarioId { get; set; }
    public string? Nombre { get; set; }
    public string? Apellido { get; set; }
    public string? Email { get; set; }
    public string? Documento { get; set; }
    public string? Password { get; set; }
    public UsuarioEstado? Estado { get; set; }

    public Guid? CredencialId { get; set; }

    public List<Guid>? RolesIDs { get; set; } = new();
    public List<Guid>? BeneficiosIDs { get; set; } = new();
    public List<Guid>? CanjesIDs { get; set; } = new();
    public List<Guid>? DispositivosIDs { get; set; } = new();
}