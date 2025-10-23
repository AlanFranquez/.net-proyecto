using Espectaculos.Domain.Enums;

namespace Espectaculos.Domain.Entities;

public class Notificacion
{
    public Guid NotificacionId { get; set; }
    public NotificacionTipo Tipo { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Cuerpo { get; set; }
    public DateTime? ProgramadaParaUtc { get; set; }
    public NotificacionEstado Estado { get; set; }
    public string[] Canales { get; set; } = Array.Empty<string>();
    public Dictionary<string, string>? Metadatos { get; set; }
    public DateTime CreadoEnUtc { get; set; }
    // Relación opcional con Dispositivo
    public Guid? DispositivoId { get; set; }
    public Dispositivo? Dispositivo { get; set; }

    public static Notificacion Create(NotificacionTipo tipo, string titulo, string? cuerpo = null, DateTime? programadaParaUtc = null)
    {
        if (string.IsNullOrWhiteSpace(titulo)) throw new ArgumentException("Titulo es obligatorio", nameof(titulo));

        return new Notificacion
        {
            NotificacionId = Guid.NewGuid(),
            Tipo = tipo,
            Titulo = titulo.Trim(),
            Cuerpo = cuerpo,
            ProgramadaParaUtc = programadaParaUtc,
            Estado = programadaParaUtc.HasValue ? NotificacionEstado.Programada : NotificacionEstado.Borrador,
            Canales = Array.Empty<string>(),
            Metadatos = null,
            CreadoEnUtc = DateTime.UtcNow
        };
    }

    public void Publish()
    {
        Estado = NotificacionEstado.Publicada;
    }

    public void Schedule(DateTime scheduledUtc)
    {
        ProgramadaParaUtc = scheduledUtc;
        Estado = NotificacionEstado.Programada;
    }

    public void Cancel()
    {
        Estado = NotificacionEstado.Cancelada;
    }
}
