namespace Espectaculos.Domain.Entities;

public class Rol
{
    public Guid RolId { get; set; }
    public string Tipo { get; set; } = default!;
    public int Prioridad { get; set; }
    public DateTime FechaAsignado { get; set; }

    public ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();
}