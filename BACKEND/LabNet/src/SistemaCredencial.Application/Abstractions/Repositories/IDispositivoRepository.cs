using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Espectaculos.Domain.Entities;
using Espectaculos.Application.Abstractions.Repositories;

namespace Espectaculos.Application.Abstractions.Repositories
{
    public interface IDispositivoRepository : IRepository<Dispositivo, Guid>
    {
        Task<IReadOnlyList<Dispositivo>> ListByUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
        Task<IReadOnlyList<Dispositivo>> ListActivosByUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
        
        Task<IReadOnlyList<Dispositivo>> ListAsync(CancellationToken ct = default);
        Task<Dispositivo?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task UpdateAsync(Dispositivo dispositivo, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<Dispositivo>> ListByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);

    }
}