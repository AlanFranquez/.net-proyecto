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
    public class RolRepository : BaseEfRepository<Rol, Guid>, IRolRepository
    {
        public RolRepository(EspectaculosDbContext db) : base(db) { }

        public async Task<Rol?> GetByTipoAsync(string tipo, CancellationToken ct = default)
            => await _set.AsNoTracking().FirstOrDefaultAsync(r => r.Tipo == tipo, ct);

        public async Task<IReadOnlyList<Rol>> ListByPrioridadAsync(int? min = null, int? max = null, CancellationToken ct = default)
        {
            var q = _set.AsNoTracking().AsQueryable();
            if (min.HasValue) q = q.Where(r => r.Prioridad >= min.Value);
            if (max.HasValue) q = q.Where(r => r.Prioridad <= max.Value);
            return await q.ToListAsync(ct);
        }
        
        public virtual async Task<IReadOnlyList<Rol>> ListAsync(CancellationToken ct = default)
            => await _set.AsNoTracking().Include(r => r.UsuarioRoles).ToListAsync(ct);
        public async Task AddAsync(Rol rol, CancellationToken ct = default)
            => await _db.Set<Rol>().AddAsync(rol, ct);

    
        public async Task<Rol?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _db.Set<Rol>().FirstOrDefaultAsync(e => e.RolId == id, ct);

        
        public async Task UpdateAsync(Rol rol, CancellationToken ct = default)
        {
            _db.Set<Rol>().Update(rol);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _db.Set<Rol>().FindAsync(new object?[] { id }, ct);
            if (entity != null)
                _db.Set<Rol>().Remove(entity);
        }
        
        public async Task<IReadOnlyList<Rol>> ListByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
        {
            var idArray = ids.Distinct().ToArray();
            return await _db.Set<Rol>().Where(r => idArray.Contains(r.RolId)).ToListAsync(ct);
        }
        
        public async Task RemoveUsuariosRelacionados(Guid id, CancellationToken ct = default)
        {
            var relaciones = await _db.Set<UsuarioRol>()
                .Where(era => era.RolId == id)
                .ToListAsync(ct);

            if (relaciones.Any())
            {
                _db.Set<UsuarioRol>().RemoveRange(relaciones);
            }
        }
    }
}