using Espectaculos.Domain.Enums;
using MediatR;

namespace Espectaculos.Application.Dispositivos.Commands.UpdateDispositivo;

public class UpdateDispositivoCommand : IRequest<Guid>
{
    public Guid DispositivoId { get; set; }

    public string? NumeroTelefono { get; set; }
    public PlataformaTipo? Plataforma { get; set; }

    public string? HuellaDispositivo { get; set; }

    public string? NavegadorNombre { get; set; }
    public string? NavegadorVersion { get; set; }

    public bool? BiometriaHabilitada { get; set; }
    public DispositivoTipo? Estado { get; set; }
    public Guid? UsuarioId { get; set; }

    public IEnumerable<Guid>? NotificacionesIds { get; set; }
    public IEnumerable<Guid>? SincronizacionesIds { get; set; }
}