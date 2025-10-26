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
    public class BeneficioRepository : BaseEfRepository<Beneficio, Guid>, IBeneficioRepository
    {
        public BeneficioRepository(EspectaculosDbContext db) : base(db) { }
        
        public virtual async Task<IReadOnlyList<Espectaculos.Domain.Entities.Beneficio>> ListAsync(CancellationToken ct = default)
            => await _set
                .AsNoTracking()
                .Include(r => r.Espacios)
                .Include(r => r.Usuarios) // Necesario para poblar UsuariosIDs en el DTO
                .ToListAsync(ct);

        public async Task<IReadOnlyList<Beneficio>> ListVigentesAsync(DateTime onDateUtc, CancellationToken ct = default)
            => await _set.AsNoTracking()
                         .Where(b => (!b.VigenciaInicio.HasValue || b.VigenciaInicio <= onDateUtc)
                                  && (!b.VigenciaFin.HasValue     || b.VigenciaFin   >= onDateUtc))
                         .ToListAsync(ct);

        public async Task<IReadOnlyList<Beneficio>> SearchByNombreAsync(string term, CancellationToken ct = default)
            => await _set.AsNoTracking().Where(b => b.Nombre.Contains(term)).ToListAsync(ct);
        
        public async Task AddAsync(Beneficio beneficio, CancellationToken ct = default)
            => await _db.Set<Beneficio>().AddAsync(beneficio, ct);

    
        public async Task<Beneficio?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _db.Set<Beneficio>().FirstOrDefaultAsync(e => e.BeneficioId == id, ct);

        
        public async Task UpdateAsync(Beneficio beneficio, CancellationToken ct = default)
        {
            _db.Set<Beneficio>().Update(beneficio);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _db.Set<Beneficio>().FindAsync(new object?[] { id }, ct);
            if (entity != null)
                _db.Set<Beneficio>().Remove(entity);
        }
        public async Task<IReadOnlyList<Beneficio>> ListByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
        {
            var idArray = ids.Distinct().ToArray();
            return await _db.Set<Beneficio>().Where(r => idArray.Contains(r.BeneficioId)).ToListAsync(ct);
        }
        
        public async Task RemoveEspaciosRelacionados(Guid id, CancellationToken ct = default)
        {
            var relaciones = await _db.Set<BeneficioEspacio>()
                .Where(be => be.BeneficioId == id)
                .ToListAsync(ct);

            if (relaciones.Any())
            {
                _db.Set<BeneficioEspacio>().RemoveRange(relaciones);
            }
        }
        
        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _db.SaveChangesAsync(ct);
        }
        
    }
}