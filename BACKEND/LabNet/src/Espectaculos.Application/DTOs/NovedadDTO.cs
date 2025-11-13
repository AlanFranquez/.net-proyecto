namespace Espectaculos.Application.DTOs;

using System;
using Espectaculos.Domain.Enums;

public record NovedadDto(
    Guid Id,
    string Titulo,
    string? Contenido,
    NotificacionTipo Tipo,
    bool Publicado,
    DateTime? DesdeUtc,
    DateTime? HastaUtc
);