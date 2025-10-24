using Espectaculos.Domain.Enums;
using MediatR;

namespace Espectaculos.Application.Notificaciones.Commands.CreateNotificacion;

public record CreateNotificacionCommand(NotificacionTipo Tipo, string Titulo, string? Cuerpo, DateTime? ProgramadaParaUtc) : IRequest<Guid>;

