namespace Espectaculos.Domain.Entities;

public class Sincronizacion
{
    public Guid SincronizacionId { get; set; }
    public DateTime CreadoEn { get; set; }
    public int CantidadItems { get; set; }
    public string? Tipo { get; set; }
    public string? Estado { get; set; }
    public string? DetalleError { get; set; }
    public string? Checksum { get; set; }
    public Guid DispositivoId { get; set; }
    public Dispositivo Dispositivo { get; set; } = default!;
}