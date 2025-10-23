using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Espectaculos.Domain.Entities;
using Espectaculos.Application.Abstractions.Repositories;
using Espectaculos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Espectaculos.Infrastructure.Repositories
{
    public class EventoAccesoRepository : BaseEfRepository<EventoAcceso, (Guid EventoId, DateTime MomentoDeAcceso, Guid CredencialId, Guid EspacioId)>, IEventoAccesoRepository
    {
        public EventoAccesoRepository(EspectaculosDbContext db) : base(db) { }

        public async Task<IReadOnlyList<EventoAcceso>> ListByEventoAsync(Guid eventoId, DateTime? fromUtc = null, DateTime? toUtc = null, CancellationToken ct = default)
        {
            var q = _set.AsNoTracking().Where(x => x.EventoId == eventoId);
            if (fromUtc.HasValue) q = q.Where(x => x.MomentoDeAcceso >= fromUtc.Value);
            if (toUtc.HasValue)   q = q.Where(x => x.MomentoDeAcceso <= toUtc.Value);
            return await q.ToListAsync(ct);
        }

        public async Task<IReadOnlyList<EventoAcceso>> ListByCredencialAsync(Guid credencialId, DateTime? fromUtc = null, DateTime? toUtc = null, CancellationToken ct = default)
        {
            var q = _set.AsNoTracking().Where(x => x.CredencialId == credencialId);
            if (fromUtc.HasValue) q = q.Where(x => x.MomentoDeAcceso >= fromUtc.Value);
            if (toUtc.HasValue)   q = q.Where(x => x.MomentoDeAcceso <= toUtc.Value);
            return await q.ToListAsync(ct);
        }
        
        
        public virtual async Task<IReadOnlyList<EventoAcceso>> ListAsync(CancellationToken ct = default)
            => await _set.AsNoTracking().Include(r => r.Credencial).Include(r => r.Espacio).ToListAsync(ct);
        public async Task AddAsync(EventoAcceso evento, CancellationToken ct = default)
            => await _db.Set<EventoAcceso>().AddAsync(evento, ct);

    
        public async Task<EventoAcceso?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _db.Set<EventoAcceso>().FirstOrDefaultAsync(e => e.EventoId == id, ct);

        
        public async Task UpdateAsync(EventoAcceso evento, CancellationToken ct = default)
        {
            _db.Set<EventoAcceso>().Update(evento);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _db.Set<EventoAcceso>().FindAsync(new object?[] { id }, ct);
            if (entity != null)
                _db.Set<EventoAcceso>().Remove(entity);
        }
        
        public async Task<IReadOnlyList<EventoAcceso>> ListByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
        {
            var idArray = ids.Distinct().ToArray();
            return await _db.Set<EventoAcceso>().Where(r => idArray.Contains(r.EventoId)).ToListAsync(ct);
        }
    }
}