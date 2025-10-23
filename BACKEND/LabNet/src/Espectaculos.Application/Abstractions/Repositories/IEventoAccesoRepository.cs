using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Espectaculos.Domain.Entities;
using Espectaculos.Application.Abstractions.Repositories;

namespace Espectaculos.Application.Abstractions.Repositories
{
    public interface IEventoAccesoRepository : IRepository<Domain.Entities.EventoAcceso, (Guid EventoId, DateTime MomentoDeAcceso, Guid CredencialId, Guid EspacioId)>
    {
        Task<IReadOnlyList<Domain.Entities.EventoAcceso>> ListByEventoAsync(Guid eventoId, DateTime? fromUtc = null, DateTime? toUtc = null, CancellationToken ct = default);
        Task<IReadOnlyList<Domain.Entities.EventoAcceso>> ListByCredencialAsync(Guid credencialId, DateTime? fromUtc = null, DateTime? toUtc = null, CancellationToken ct = default);
        
        Task<IReadOnlyList<Domain.Entities.EventoAcceso>> ListAsync(CancellationToken ct = default);
        Task<Domain.Entities.EventoAcceso?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task UpdateAsync(Domain.Entities.EventoAcceso eventoAcceso, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<Domain.Entities.EventoAcceso>> ListByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    }
}