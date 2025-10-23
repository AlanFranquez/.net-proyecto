using Espectaculos.Domain.Enums;
using MediatR;

namespace Espectaculos.Application.ReglaDeAcceso.Commands.UpdateReglaDeAcceso;

public class UpdateReglaCommand : IRequest<Guid>
{
    public Guid ReglaId { get; set; }
    public string? VentanaHoraria { get; set; }
    public DateTime? VigenciaInicio { get; set; }
    public DateTime? VigenciaFin { get; set; }
    public int? Prioridad { get; set; }
    public AccesoTipo? Politica { get; set; }
    public bool? RequiereBiometriaConfirmacion { get; set; } = false;

    public ICollection<Guid>? EspaciosIDs { get; set; } = new List<Guid>();

}