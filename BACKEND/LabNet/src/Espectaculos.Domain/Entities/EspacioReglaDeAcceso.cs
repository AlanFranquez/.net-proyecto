namespace Espectaculos.Domain.Entities;

public class EspacioReglaDeAcceso
{
    public Guid EspacioId { get; set; }
    public Espacio Espacio { get; set; } = null!;
    public Guid ReglaId { get; set; }
    public ReglaDeAcceso Regla { get; set; } = null!;
}