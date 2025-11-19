using Espectaculos.Domain.Enums;

namespace Espectaculos.Application.DTOs;
public class CredencialDTO
{
    public Guid CredencialId { get; set; }
    public CredencialTipo? Tipo { get; set; } = null;
    public CredencialEstado? Estado { get; set; } = null;
    public string? IdCriptografico { get; set; } = null;
    public DateTime? FechaEmision { get; set; } = null;
    public DateTime? FechaExpiracion { get; set; } = null;
    public Guid UsuarioId { get; set; }
    public IEnumerable<Guid>? EventoAccesoIds { get; set; } = null;
    public string? UsuarioNombre { get; set; }
    public string? UsuarioApellido { get; set; }
    public string? UsuarioEmail { get; set; }

}