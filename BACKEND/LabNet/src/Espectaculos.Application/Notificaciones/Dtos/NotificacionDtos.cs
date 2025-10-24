using Espectaculos.Domain.Enums;

namespace Espectaculos.Application.Notificaciones.Dtos;

public record CreateNotificacionDto(NotificacionTipo Tipo, string Titulo, string? Cuerpo, DateTime? ProgramadaParaUtc);
public record NotificacionDto(Guid NotificacionId, NotificacionTipo Tipo, string Titulo, string? Cuerpo, DateTime? ProgramadaParaUtc, string Estado, string[] Canales, Dictionary<string,string>? Metadatos, DateTime CreadoEnUtc);
