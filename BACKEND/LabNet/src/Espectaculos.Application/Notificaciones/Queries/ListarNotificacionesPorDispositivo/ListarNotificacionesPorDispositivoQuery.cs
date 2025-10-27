using Espectaculos.Domain.Enums;
using MediatR;
using System.Collections.Generic;

namespace Espectaculos.Application.Notificaciones.Queries.ListarNotificacionesPorDispositivo;

public sealed record ListarNotificacionesPorDispositivoQuery(Guid DispositivoId, NotificacionLecturaEstado? Lectura = null, int? Take = 50, int? Skip = 0) : IRequest<IReadOnlyList<Espectaculos.Domain.Entities.Notificacion>>;
