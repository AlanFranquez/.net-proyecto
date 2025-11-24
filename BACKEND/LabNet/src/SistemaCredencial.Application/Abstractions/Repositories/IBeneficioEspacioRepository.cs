using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Espectaculos.Domain.Entities;
using Espectaculos.Application.Abstractions.Repositories;

namespace Espectaculos.Application.Abstractions.Repositories
{
    public interface IBeneficioEspacioRepository : IRepository<BeneficioEspacio, (Guid BeneficioId, Guid EspacioId)>
    {
        Task<IReadOnlyList<BeneficioEspacio>> ListByBeneficioAsync(Guid beneficioId, CancellationToken ct = default);
        Task<IReadOnlyList<BeneficioEspacio>> ListByEspacioAsync(Guid espacioId, CancellationToken ct = default);
    }
}