namespace Espectaculos.Application.DTOs;

public class RolDTO
{
    public Guid RolId { get; set; }
    public string? Tipo { get; set; } = null;
    public int? Prioridad { get; set; } = null;
    public DateTime? FechaAsignado { get; set; } = null;
    public IEnumerable<Guid>? UsuariosIDs { get; set; } = null;
}