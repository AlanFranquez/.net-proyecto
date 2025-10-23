using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Espectaculos.Domain.Entities;
using Espectaculos.Application.Abstractions.Repositories;

namespace Espectaculos.Application.Abstractions.Repositories
{
    public interface IBeneficioRepository : IRepository<Beneficio, Guid>
    {
        Task<IReadOnlyList<Beneficio>> ListAsync(CancellationToken ct = default);
        Task<IReadOnlyList<Beneficio>> ListVigentesAsync(DateTime onDateUtc, CancellationToken ct = default);
        Task<IReadOnlyList<Beneficio>> SearchByNombreAsync(string term, CancellationToken ct = default);
        Task<Beneficio?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task UpdateAsync(Beneficio beneficio, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<Beneficio>> ListByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
        Task RemoveEspaciosRelacionados(Guid id, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}