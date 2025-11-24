using Espectaculos.Domain.Enums;

namespace Espectaculos.Application.DTOs;

public class ReglaDeAccesoDTO
{
    public Guid ReglaId { get; set; }
    public string? VentanaHoraria { get; set; }
    public DateTime? VigenciaInicio { get; set; }
    public DateTime? VigenciaFin { get; set; }
    public int Prioridad { get; set; }
    public AccesoTipo Politica { get; set; }
    public bool RequiereBiometriaConfirmacion { get; set; }
    public string? Rol { get; set; }
    public ICollection<Guid>? EspaciosIDs { get; set; } = new List<Guid>();
}