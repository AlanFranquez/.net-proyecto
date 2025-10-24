using MediatR;

namespace Espectaculos.Application.Notificaciones.Commands.PublishNotificacion;

public sealed record PublishNotificacionCommand(Guid Id, DateTime? ProgramadaParaUtc) : IRequest<bool>;

