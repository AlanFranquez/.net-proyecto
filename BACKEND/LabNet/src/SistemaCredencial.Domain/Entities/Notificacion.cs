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
    public NotificacionLecturaEstado LecturaEstado { get; set; } = NotificacionLecturaEstado.SinVer;
    public string[] Canales { get; set; } = Array.Empty<string>();
    public Dictionary<string, string>? Metadatos { get; set; }
    public DateTime CreadoEnUtc { get; set; }
    public Espectaculos.Domain.Enums.NotificacionAudiencia Audiencia { get; set; } = Espectaculos.Domain.Enums.NotificacionAudiencia.Todos;
    // Relación opcional con Dispositivo
    public Guid? DispositivoId { get; set; }
    public Dispositivo? Dispositivo { get; set; }
    // Relación opcional con Usuario: si la notificación fue dirigida a un usuario específico
    public Guid? UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public static Notificacion Create(NotificacionTipo tipo, string titulo, string? cuerpo = null, DateTime? programadaParaUtc = null, Espectaculos.Domain.Enums.NotificacionAudiencia audiencia = Espectaculos.Domain.Enums.NotificacionAudiencia.Todos)
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
            CreadoEnUtc = DateTime.UtcNow,
            Audiencia = audiencia
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
