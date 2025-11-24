using Espectaculos.Application.Notificaciones.Dtos;
using MediatR;

namespace Espectaculos.Application.Notificaciones.Queries.ListNotificaciones;

public sealed record ListNotificacionesQuery(
    bool OnlyActive,
    Guid? UsuarioId
) : IRequest<IEnumerable<NotificacionDto>>;