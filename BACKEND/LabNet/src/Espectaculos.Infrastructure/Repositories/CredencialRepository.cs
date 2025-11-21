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
    public class CredencialRepository : BaseEfRepository<Credencial, Guid>, ICredencialRepository
    {
        public CredencialRepository(EspectaculosDbContext db) : base(db) { }

        public async Task<IReadOnlyList<Credencial>> ListVigentesAsync(DateTime onDateUtc, CancellationToken ct = default)
            => await _set
                .Include(c => c.Usuario) 
                .AsNoTracking()
                .Where(c => (!c.FechaExpiracion.HasValue || c.FechaExpiracion >= onDateUtc)
                         &&  c.FechaEmision <= onDateUtc)
                .ToListAsync(ct);
        
        public override async Task<IReadOnlyList<Credencial>> ListAsync(CancellationToken ct = default)
            => await _set
                .Include(c => c.Usuario)                 // <-- ADDED
                .Include(c => c.EventosAcceso)
                .AsNoTracking()
                .ToListAsync(ct);

        public async Task AddAsync(Credencial credencial, CancellationToken ct = default)
            => await _db.Set<Credencial>().AddAsync(credencial, ct);

        public async Task<Credencial?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _db.Set<Credencial>()
                .Include(c => c.Usuario)                 // <-- ADDED
                .Include(c => c.EventosAcceso)
                .FirstOrDefaultAsync(e => e.CredencialId == id, ct);

        public async Task UpdateAsync(Credencial credencial, CancellationToken ct = default)
        {
            _db.Set<Credencial>().Update(credencial);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _db.Set<Credencial>().FindAsync(new object?[] { id }, ct);
            if (entity != null)
                _db.Set<Credencial>().Remove(entity);
        }
        
        public async Task<IReadOnlyList<Credencial>> ListByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
        {
            var idArray = ids.Distinct().ToArray();
            return await _db.Set<Credencial>()
                .Include(c => c.Usuario) 
                .Where(r => idArray.Contains(r.CredencialId))
                .AsNoTracking()
                .ToListAsync(ct);
        }
    }
}
