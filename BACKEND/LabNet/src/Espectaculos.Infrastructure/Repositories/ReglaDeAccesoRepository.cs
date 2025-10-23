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
    public class ReglaDeAccesoRepository : BaseEfRepository<Espectaculos.Domain.Entities.ReglaDeAcceso, Guid>, IReglaDeAccesoRepository
    {
        public ReglaDeAccesoRepository(EspectaculosDbContext db) : base(db) { }

        public async Task<IReadOnlyList<Espectaculos.Domain.Entities.ReglaDeAcceso>> ListVigentesAsync(DateTime onDateUtc, CancellationToken ct = default)
            => await _set.AsNoTracking()
                         .Where(r => (!r.VigenciaInicio.HasValue || r.VigenciaInicio <= onDateUtc)
                                  && (!r.VigenciaFin.HasValue     || r.VigenciaFin   >= onDateUtc))
                         .ToListAsync(ct);
        public async Task AddAsync(ReglaDeAcceso regla, CancellationToken ct = default)
            => await _db.Set<ReglaDeAcceso>().AddAsync(regla, ct);

        public virtual async Task<IReadOnlyList<Espectaculos.Domain.Entities.ReglaDeAcceso>> ListAsync(CancellationToken ct = default)
            => await _set.AsNoTracking().Include(r => r.Espacios).ToListAsync(ct);
        
        public async Task<Espectaculos.Domain.Entities.ReglaDeAcceso?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _db.Set<ReglaDeAcceso>().FirstOrDefaultAsync(e => e.ReglaId == id, ct);

        
        public async Task UpdateAsync(ReglaDeAcceso regla, CancellationToken ct = default)
        {
            _db.Set<ReglaDeAcceso>().Update(regla);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _db.Set<ReglaDeAcceso>().FindAsync(new object?[] { id }, ct);
            if (entity != null)
                _db.Set<ReglaDeAcceso>().Remove(entity);
        }
        public async Task<IReadOnlyList<Espectaculos.Domain.Entities.ReglaDeAcceso>> ListByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
        {
            var idArray = ids.Distinct().ToArray();
            return await _db.Set<ReglaDeAcceso>().Where(r => idArray.Contains(r.ReglaId)).ToListAsync(ct);
        }
        
        public async Task RemoveEspaciosRelacionados(Guid reglaId, CancellationToken ct = default)
        {
            var relaciones = await _db.Set<EspacioReglaDeAcceso>()
                .Where(era => era.ReglaId == reglaId)
                .ToListAsync(ct);

            if (relaciones.Any())
            {
                _db.Set<EspacioReglaDeAcceso>().RemoveRange(relaciones);
            }
        }
        
        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _db.SaveChangesAsync(ct);
        }
    }
}