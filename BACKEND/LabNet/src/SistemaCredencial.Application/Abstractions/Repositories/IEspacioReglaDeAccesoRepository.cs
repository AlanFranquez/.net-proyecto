using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Espectaculos.Domain.Entities;
using Espectaculos.Application.Abstractions.Repositories;

namespace Espectaculos.Application.Abstractions.Repositories
{
    public interface IEspacioReglaDeAccesoRepository : IRepository<EspacioReglaDeAcceso, (Guid EspacioId, Guid ReglaId)>
    {
        Task<IReadOnlyList<EspacioReglaDeAcceso>> ListByEspacioAsync(Guid espacioId, CancellationToken ct = default);
        Task<IReadOnlyList<EspacioReglaDeAcceso>> ListByReglaAsync(Guid reglaId, CancellationToken ct = default);
    }
}