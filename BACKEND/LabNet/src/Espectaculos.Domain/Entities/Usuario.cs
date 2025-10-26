using Espectaculos.Domain.Enums;

namespace Espectaculos.Domain.Entities;

public class Usuario
{
    public Guid UsuarioId { get; set; }
    public string Documento { get; set; }
    public string PasswordHash { get; set; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public string Email { get; set; }
    public UsuarioEstado Estado { get; set; }
    public Credencial? Credencial { get; set; } = null;
    public Guid? CredencialId { get; set; } = null;
    public ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();
    public ICollection<Dispositivo>  Dispositivos { get; set; } = new List<Dispositivo>();
    public ICollection<BeneficioUsuario> Beneficios { get; set; } = new List<BeneficioUsuario>();
    public ICollection<Canje> Canjes { get; set; }= new List<Canje>();
    // Notificaciones asociadas al usuario (registro histórico de notificaciones recibidas)
    public ICollection<Notificacion> Notificaciones { get; set; } = new List<Notificacion>();
}