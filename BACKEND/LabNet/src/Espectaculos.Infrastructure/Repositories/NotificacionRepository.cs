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

    public async Task<IReadOnlyList<Notificacion>> ListByDispositivoAsync(Guid dispositivoId, Espectaculos.Domain.Enums.NotificacionLecturaEstado? lectura = null, int? take = null, int? skip = null, CancellationToken ct = default)
    {
        var q = _set.AsNoTracking().Where(n => n.DispositivoId == dispositivoId);
        if (lectura.HasValue)
            q = q.Where(n => n.LecturaEstado == lectura.Value);

        q = q.OrderByDescending(n => n.CreadoEnUtc);

        if (skip.HasValue && skip.Value > 0) q = q.Skip(skip.Value);
        if (take.HasValue && take.Value > 0) q = q.Take(take.Value);

        return await q.ToListAsync(ct);
    }
}