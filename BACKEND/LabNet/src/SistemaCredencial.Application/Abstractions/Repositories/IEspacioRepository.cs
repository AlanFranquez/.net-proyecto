using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Espectaculos.Domain.Entities;
using Espectaculos.Application.Abstractions.Repositories;

namespace Espectaculos.Application.Abstractions.Repositories
{
    public interface IEspacioRepository : IRepository<Espacio, Guid>
    {
        Task<IReadOnlyList<Espacio>> ListActivosAsync(CancellationToken ct = default);
        
        Task<IReadOnlyList<Espacio>> ListAsync(CancellationToken ct = default);
        Task<Espacio?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task UpdateAsync(Espacio espacio, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<Espacio>> ListByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
        Task RemoveReglasRelacionadas(Guid id, CancellationToken ct = default);
        Task RemoveBeneficiosRelacionados(Guid id, CancellationToken ct = default);
        
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}