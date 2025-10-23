using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Espectaculos.Domain.Entities;
using Espectaculos.Application.Abstractions.Repositories;

namespace Espectaculos.Application.Abstractions.Repositories
{
    public interface IReglaDeAccesoRepository : IRepository<ReglaDeAcceso, Guid>
    {
        Task<IReadOnlyList<ReglaDeAcceso>> ListVigentesAsync(DateTime onDateUtc, CancellationToken ct = default);
        Task<ReglaDeAcceso?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task UpdateAsync(ReglaDeAcceso regla, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<ReglaDeAcceso>> ListByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}