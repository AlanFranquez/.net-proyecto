using Espectaculos.Domain.Enums;

namespace Espectaculos.Domain.Entities;
public class ReglaDeAcceso
{
    public Guid ReglaId { get; set; }
    public string? VentanaHoraria { get; set; }
    public DateTime? VigenciaInicio { get; set; }
    public DateTime? VigenciaFin { get; set; }
    public int Prioridad { get; set; }
    public AccesoTipo Politica { get; set; }
    public bool RequiereBiometriaConfirmacion { get; set; }

    public ICollection<EspacioReglaDeAcceso> Espacios { get; set; } = new List<EspacioReglaDeAcceso>();

}