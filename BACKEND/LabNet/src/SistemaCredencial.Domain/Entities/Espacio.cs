using Espectaculos.Domain.Enums;

namespace Espectaculos.Domain.Entities;

public class Espacio
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = default!;
    public bool Activo { get; set; }
    public EspacioTipo Tipo { get; set; }
    public Modo Modo { get; set; }

    public ICollection<EventoAcceso> EventoAccesos { get; set; } = new List<EventoAcceso>();
    public ICollection<EspacioReglaDeAcceso> Reglas { get; set; } = new List<EspacioReglaDeAcceso>();
    public ICollection<BeneficioEspacio> Beneficios { get; set; } = new List<BeneficioEspacio>();
}