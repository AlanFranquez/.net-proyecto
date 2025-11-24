using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Espectaculos.Domain.Entities;
using Espectaculos.Application.Abstractions.Repositories;

namespace Espectaculos.Application.Abstractions.Repositories
{
    public interface ISincronizacionRepository : IRepository<Sincronizacion, Guid>
    {
        Task<IReadOnlyList<Sincronizacion>> ListRecientesByDispositivoAsync(Guid dispositivoId, int take = 50, CancellationToken ct = default);
        
        Task<IReadOnlyList<Sincronizacion>> ListAsync(CancellationToken ct = default);
        Task<Sincronizacion?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task UpdateAsync(Sincronizacion sincronizacion, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<Sincronizacion>> ListByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
        Task<int> CountPendientesAsync(CancellationToken ct = default);
    }
}