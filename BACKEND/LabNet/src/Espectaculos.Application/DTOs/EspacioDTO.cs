using Espectaculos.Domain.Enums;

namespace Espectaculos.Application.DTOs;

public class EspacioDTO
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = default!;
    public bool Activo { get; set; } = default!;
    public EspacioTipo Tipo { get; set; } = default!;
    public Modo Modo { get; set; } = default!;
    public IEnumerable<Guid>? EventoAccesoIds { get; set; } = null;
    public IEnumerable<Guid>? ReglaIds { get; set; } = null;
    public IEnumerable<Guid>? BeneficioIds { get; set; } = null;
}