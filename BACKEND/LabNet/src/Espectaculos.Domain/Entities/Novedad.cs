using Espectaculos.Domain.Enums;

namespace Espectaculos.Domain.Entities;

public class Novedad
{
    public Guid NovedadId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Contenido { get; set; }
    public NotificacionTipo Tipo { get; set; }
    public DateTime CreadoEnUtc { get; set; }
    public DateTime? PublicadoDesdeUtc { get; set; }
    public DateTime? PublicadoHastaUtc { get; set; }
    public bool Publicado { get; private set; }

    // invariantes y factory
    public static Novedad Create(string titulo, string? contenido, NotificacionTipo tipo, DateTime? desdeUtc = null, DateTime? hastaUtc = null)
    {
        if (string.IsNullOrWhiteSpace(titulo)) throw new ArgumentException("Titulo es obligatorio", nameof(titulo));
        if (desdeUtc.HasValue && hastaUtc.HasValue && desdeUtc > hastaUtc) throw new ArgumentException("PublicadoDesde debe ser anterior a PublicadoHasta");

        return new Novedad
        {
            NovedadId = Guid.NewGuid(),
            Titulo = titulo.Trim(),
            Contenido = contenido,
            Tipo = tipo,
            CreadoEnUtc = DateTime.UtcNow,
            PublicadoDesdeUtc = desdeUtc,
            PublicadoHastaUtc = hastaUtc,
            Publicado = false
        };
    }

    // operaciones de negocio
    public void Publish(DateTime? fromUtc = null, DateTime? toUtc = null)
    {
        if (fromUtc.HasValue && toUtc.HasValue && fromUtc > toUtc) throw new InvalidOperationException("Rango de publicación inválido");
        PublicadoDesdeUtc = fromUtc ?? DateTime.UtcNow;
        PublicadoHastaUtc = toUtc;
        Publicado = true;
    }

    public void Unpublish()
    {
        Publicado = false;
    }

    public bool IsActive(DateTime atUtc)
    {
        if (!Publicado) return false;
        if (PublicadoDesdeUtc.HasValue && atUtc < PublicadoDesdeUtc.Value) return false;
        if (PublicadoHastaUtc.HasValue && atUtc > PublicadoHastaUtc.Value) return false;
        return true;
    }
}
