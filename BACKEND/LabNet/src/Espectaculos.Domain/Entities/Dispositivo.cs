using Espectaculos.Domain.Enums;

namespace Espectaculos.Domain.Entities;

public class Dispositivo
{
    public Guid DispositivoId { get; set; }
    public string? NumeroTelefono { get; set; }
    public PlataformaTipo Plataforma { get; set; }
    public string? HuellaDispositivo { get; set; }
    
    public string? NavegadorNombre { get; set; }  
    
    public string? NavegadorVersion { get; set; }
    public bool BiometriaHabilitada { get; set; }
    public DispositivoTipo Estado { get; set; }
    public Guid UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public ICollection<Notificacion> Notificaciones { get; set; } = new List<Notificacion>();
    public ICollection<Sincronizacion> Sincronizaciones { get; set; } = new List<Sincronizacion>();
}