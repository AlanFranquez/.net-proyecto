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
    public class EspacioRepository : BaseEfRepository<Espacio, Guid>, IEspacioRepository
    {
        public EspacioRepository(EspectaculosDbContext db) : base(db) { }

        public async Task<IReadOnlyList<Espacio>> ListActivosAsync(CancellationToken ct = default)
            => await _set.AsNoTracking().Where(e => e.Activo).ToListAsync(ct);
        
        public async Task AddAsync(Espacio espacio, CancellationToken ct = default)
            => await _db.Set<Espacio>().AddAsync(espacio, ct);

    
        public async Task<Espacio?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _db.Set<Espacio>().FirstOrDefaultAsync(e => e.Id == id, ct);

        
        public async Task UpdateAsync(Espacio espacio, CancellationToken ct = default)
        {
            _db.Set<Espacio>().Update(espacio);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _db.Set<Espacio>().FindAsync(new object?[] { id }, ct);
            if (entity != null)
                _db.Set<Espacio>().Remove(entity);
        }
        
        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _db.SaveChangesAsync(ct);
        }
        
    }
}