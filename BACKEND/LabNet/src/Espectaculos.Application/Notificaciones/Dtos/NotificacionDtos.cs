// Espectaculos.Application/Notificaciones/Dtos/NotificacionDto.cs
using Espectaculos.Domain.Enums;

namespace Espectaculos.Application.Notificaciones.Dtos;

// Añadimos UsuarioId como segundo campo
public record CreateNotificacionDto(
    NotificacionTipo Tipo,
    string Titulo,
    string? Cuerpo,
    DateTime? ProgramadaParaUtc,
    NotificacionAudiencia Audiencia
);

public record NotificacionDto(
    Guid NotificacionId,
    Guid? UsuarioId,
    NotificacionTipo Tipo,
    string Titulo,
    string? Cuerpo,
    DateTime? ProgramadaParaUtc,
    string Estado,
    string[] Canales,
    Dictionary<string, string>? Metadatos,
    DateTime CreadoEnUtc,
    NotificacionAudiencia Audiencia
);