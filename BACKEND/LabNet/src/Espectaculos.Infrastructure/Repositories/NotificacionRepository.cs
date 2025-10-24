using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Espectaculos.Application.Abstractions.Repositories;
using Espectaculos.Domain.Entities;
using Espectaculos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Espectaculos.Infrastructure.Repositories;

public class NotificacionRepository : BaseEfRepository<Notificacion, Guid>, INotificacionRepository
{
    public NotificacionRepository(EspectaculosDbContext db) : base(db) { }

    public async Task<IReadOnlyList<Notificacion>> ListByEstadoAsync(int estado, CancellationToken ct = default)
    {
        return await _set.AsNoTracking().Where(n => (int)n.Estado == estado).ToListAsync(ct);
    }
}