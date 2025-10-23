using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Espectaculos.Domain.Entities;
using Espectaculos.Application.Abstractions.Repositories;

namespace Espectaculos.Application.Abstractions.Repositories
{
    public interface IReglaDeAccesoRepository : IRepository<Espectaculos.Domain.Entities.ReglaDeAcceso, Guid>
    {
        Task<IReadOnlyList<Espectaculos.Domain.Entities.ReglaDeAcceso>> ListAsync(CancellationToken ct = default);
        Task<Espectaculos.Domain.Entities.ReglaDeAcceso?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task UpdateAsync(Espectaculos.Domain.Entities.ReglaDeAcceso regla, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<Espectaculos.Domain.Entities.ReglaDeAcceso>> ListByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
        Task RemoveEspaciosRelacionados(Guid reglaId, CancellationToken ct = default);

        Task SaveChangesAsync(CancellationToken ct = default);
    }
}