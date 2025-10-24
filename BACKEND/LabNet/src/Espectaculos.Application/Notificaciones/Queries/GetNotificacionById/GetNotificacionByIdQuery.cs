using Espectaculos.Application.Notificaciones.Dtos;
using MediatR;

namespace Espectaculos.Application.Notificaciones.Queries.GetNotificacionById;

public record GetNotificacionByIdQuery(Guid Id) : IRequest<NotificacionDto?>;

