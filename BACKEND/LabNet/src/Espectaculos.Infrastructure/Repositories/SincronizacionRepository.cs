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
    public class SincronizacionRepository : BaseEfRepository<Sincronizacion, Guid>, ISincronizacionRepository
    {
        public SincronizacionRepository(EspectaculosDbContext db) : base(db) { }

        public async Task<IReadOnlyList<Sincronizacion>> ListRecientesByDispositivoAsync(Guid dispositivoId, int take = 50, CancellationToken ct = default)
            => await _set.AsNoTracking()
                         .Where(s => s.DispositivoId == dispositivoId)
                         .OrderByDescending(s => s.CreadoEn)
                         .Take(take)
                         .ToListAsync(ct);
        
        public virtual async Task<IReadOnlyList<Sincronizacion>> ListAsync(CancellationToken ct = default)
            => await _set.AsNoTracking().Include(r => r.Dispositivo).ToListAsync(ct);
        public async Task AddAsync(Sincronizacion sincronizacion, CancellationToken ct = default)
            => await _db.Set<Sincronizacion>().AddAsync(sincronizacion, ct);

    
        public async Task<Sincronizacion?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _db.Set<Sincronizacion>().FirstOrDefaultAsync(e => e.SincronizacionId == id, ct);

        
        public async Task UpdateAsync(Sincronizacion sincronizacion, CancellationToken ct = default)
        {
            _db.Set<Sincronizacion>().Update(sincronizacion);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _db.Set<Sincronizacion>().FindAsync(new object?[] { id }, ct);
            if (entity != null)
                _db.Set<Sincronizacion>().Remove(entity);
        }
        
        public async Task<IReadOnlyList<Sincronizacion>> ListByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
        {
            var idArray = ids.Distinct().ToArray();
            return await _db.Set<Sincronizacion>().Where(r => idArray.Contains(r.SincronizacionId)).ToListAsync(ct);
        }

        public async Task<int> CountPendientesAsync(CancellationToken ct = default)
        {
            return await _db.Set<Sincronizacion>()
                .AsNoTracking()
                .CountAsync(s => s.Estado != "OK", ct);
        }
    }
}