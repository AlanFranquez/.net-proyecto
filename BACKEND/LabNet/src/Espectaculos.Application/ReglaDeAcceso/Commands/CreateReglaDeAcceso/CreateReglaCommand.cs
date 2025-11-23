using Espectaculos.Domain.Enums;
using MediatR;

namespace Espectaculos.Application.ReglaDeAcceso.Commands.CreateReglaDeAcceso;

public class CreateReglaCommand : IRequest<Guid>
{
    public string VentanaHoraria { get; set; }
    public DateTime? VigenciaInicio { get; set; }
    public DateTime? VigenciaFin { get; set; }
    public int Prioridad { get; set; }
    public AccesoTipo Politica { get; set; }
    public bool? RequiereBiometriaConfirmacion { get; set; } = false;
    public string? Rol { get; set; }
    public ICollection<Guid>? EspaciosIDs { get; set; } = new List<Guid>();

}