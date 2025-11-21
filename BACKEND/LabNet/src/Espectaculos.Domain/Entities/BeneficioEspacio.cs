namespace Espectaculos.Domain.Entities;

public class BeneficioEspacio
{
    public Guid BeneficioId { get; set; }
    public Beneficio Beneficio { get; set; } = null!;
    public Guid EspacioId { get; set; }
    public Espacio Espacio { get; set; } = null!;

}