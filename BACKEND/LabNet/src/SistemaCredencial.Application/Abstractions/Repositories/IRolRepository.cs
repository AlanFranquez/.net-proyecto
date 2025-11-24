using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Espectaculos.Domain.Entities;
using Espectaculos.Application.Abstractions.Repositories;

namespace Espectaculos.Application.Abstractions.Repositories
{
    public interface IRolRepository : IRepository<Rol, Guid>
    {
        Task<Rol?> GetByTipoAsync(string tipo, CancellationToken ct = default);
        Task<IReadOnlyList<Rol>> ListByPrioridadAsync(int? min = null, int? max = null, CancellationToken ct = default);
        
        Task<IReadOnlyList<Rol>> ListAsync(CancellationToken ct = default);
        Task<Rol?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task UpdateAsync(Rol rol, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<Rol>> ListByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
        Task RemoveUsuariosRelacionados(Guid id, CancellationToken ct = default);
    }
}