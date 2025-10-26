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
    public class DispositivoRepository : BaseEfRepository<Dispositivo, Guid>, IDispositivoRepository
    {
        public DispositivoRepository(EspectaculosDbContext db) : base(db) { }

        public async Task<IReadOnlyList<Dispositivo>> ListByUsuarioAsync(Guid usuarioId, CancellationToken ct = default)
            => await _set.AsNoTracking().Where(d => d.UsuarioId == usuarioId).ToListAsync(ct);

        public async Task<IReadOnlyList<Dispositivo>> ListActivosByUsuarioAsync(Guid usuarioId, CancellationToken ct = default)
            => await _set.AsNoTracking().Where(d => d.UsuarioId == usuarioId && d.Estado != 0).ToListAsync(ct);

        
        
        public virtual async Task<IReadOnlyList<Dispositivo>> ListAsync(CancellationToken ct = default)
            => await _set.AsNoTracking().Include(r => r.Usuario).Include(r => r.Sincronizaciones).Include(r => r.Notificaciones).ToListAsync(ct);
        public async Task AddAsync(Dispositivo dispositivo, CancellationToken ct = default)
            => await _db.Set<Dispositivo>().AddAsync(dispositivo, ct);

    
        public async Task<Dispositivo?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _db.Set<Dispositivo>().FirstOrDefaultAsync(e => e.DispositivoId == id, ct);

        
        public async Task UpdateAsync(Dispositivo dispositivo, CancellationToken ct = default)
        {
            _db.Set<Dispositivo>().Update(dispositivo);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _db.Set<Dispositivo>().FindAsync(new object?[] { id }, ct);
            if (entity != null)
                _db.Set<Dispositivo>().Remove(entity);
        }
        
        public async Task<IReadOnlyList<Dispositivo>> ListByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
        {
            var idArray = ids.Distinct().ToArray();
            return await _db.Set<Dispositivo>().Where(r => idArray.Contains(r.DispositivoId)).ToListAsync(ct);
        }
    }
}