using Espectaculos.Domain.Enums;

namespace Espectaculos.Domain.Entities;

public class Credencial
{
    public Guid CredencialId { get; set; }
    public CredencialTipo Tipo { get; set; }
    public CredencialEstado Estado { get; set; }
    public string? IdCriptografico { get; set; }
    public DateTime FechaEmision { get; set; }
    public DateTime? FechaExpiracion { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public Guid UsuarioId { get; set; }

    public ICollection<EventoAcceso> EventosAcceso { get; set; } = new List<EventoAcceso>();
}