using System;
using System.Threading;
using System.Threading.Tasks;

namespace Espectaculos.Application.Abstractions;

public interface IAvailabilityService
{
    Task RecomputeForEventAsync(Guid eventoId, CancellationToken cancellationToken = default);
    Task<int> RecomputeAllAsync(CancellationToken cancellationToken = default);
}
