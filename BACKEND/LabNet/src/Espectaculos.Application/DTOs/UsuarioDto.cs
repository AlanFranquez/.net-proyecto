using Espectaculos.Domain.Enums;

namespace Espectaculos.Application.DTOs;

public class UsuarioDto
{
    public Guid UsuarioId { get; set; }
    public string Documento { get; set; } = default!;
    public string Nombre { get; set; } = default!;
    public string Apellido { get; set; } = default!;
    public string Email { get; set; } = default!;
    public UsuarioEstado Estado { get; set; }

    public string Password { get; set; }
    public Guid? CredencialId { get; set; }
    public IEnumerable<Guid>? RolesIDs { get; set; } = null;
    public IEnumerable<Guid>? DispositivosIDs { get; set; } = null;
    public IEnumerable<Guid>? BeneficiosIDs { get; set; } = null;
    public IEnumerable<Guid>? CanjesIDs { get; set; } = null;
}