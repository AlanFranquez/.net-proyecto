using Espectaculos.Application.Notificaciones.Dtos;
using MediatR;

namespace Espectaculos.Application.Notificaciones.Queries.ListNotificaciones;

public record ListNotificacionesQuery(bool OnlyActive) : IRequest<IEnumerable<NotificacionDto>>;

