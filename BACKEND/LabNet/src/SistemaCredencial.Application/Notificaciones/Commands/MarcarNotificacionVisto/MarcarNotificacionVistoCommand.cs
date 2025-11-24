using MediatR;

namespace Espectaculos.Application.Notificaciones.Commands.MarcarNotificacionVisto;

public sealed record MarcarNotificacionVistoCommand(Guid DispositivoId, Guid NotificacionId) : IRequest<bool>;
