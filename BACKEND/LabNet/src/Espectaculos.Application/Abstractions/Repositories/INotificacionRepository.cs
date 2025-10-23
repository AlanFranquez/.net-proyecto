using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Espectaculos.Domain.Entities;

namespace Espectaculos.Application.Abstractions.Repositories;

public interface INotificacionRepository : IRepository<Notificacion, Guid>
{
    Task<IReadOnlyList<Notificacion>> ListByEstadoAsync(int estado, CancellationToken ct = default);
}