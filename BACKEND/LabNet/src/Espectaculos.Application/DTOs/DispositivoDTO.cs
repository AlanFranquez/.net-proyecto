using Espectaculos.Domain.Enums;

namespace Espectaculos.Application.DTOs;

public class DispositivoDTO
{
    public Guid DispositivoId { get; set; }
    public string? NumeroTelefono { get; set; } = null;
    public PlataformaTipo? Plataforma { get; set; } = null;
    public string? HuellaDispositivo { get; set; } = null;
    public bool? BiometriaHabilitada { get; set; } = null;
    public DispositivoTipo? Estado { get; set; } = null;
    public Guid? UsuarioId { get; set; } = null;
    public IEnumerable<Guid>? NotificacionesIds { get; set; } = null;
    public IEnumerable<Guid>? SincronizacionesIds { get; set; } = null;
}