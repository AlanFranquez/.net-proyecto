using Espectaculos.Domain.Enums;
using MediatR;

namespace Espectaculos.Application.Espacios.Commands.UpdateEspacio;

public class UpdateEspacioCommand : IRequest<Guid>
{
    public Guid Id { get; set; }
    public string? Nombre { get; set; } = null;
    public bool? Activo { get; set; } = null;
    public EspacioTipo? Tipo { get; set; } = null;
    public Modo? Modo { get; set; } = null;
    public IEnumerable<Guid>? EventoAccesoIds { get; set; } = null;
    public IEnumerable<Guid>? ReglaIds { get; set; } = null;
    public IEnumerable<Guid>? BeneficioIds { get; set; } = null;
}
