// Espectaculos.Application/Abstractions/Repositories/INovedadRepository.cs
using Espectaculos.Domain.Entities;
using Espectaculos.Domain.Enums;

namespace Espectaculos.Application.Abstractions.Repositories;

public record NovedadFilter(string? Q, NotificacionTipo? Tipo, bool OnlyPublished, bool OnlyActiveUtcNow,
    int Page = 1, int PageSize = 20);

public interface INovedadRepository
{
    Task AddAsync(Novedad entity, CancellationToken ct);
    Task<Novedad?> GetByIdAsync(Guid id, CancellationToken ct);
    Task UpdateAsync(Novedad entity, CancellationToken ct);
    Task DeleteAsync(Novedad entity, CancellationToken ct);

    // para ListarNovedades
    Task<(IReadOnlyList<Novedad> Items, int Total)> ListAsync(NovedadFilter filter, CancellationToken ct);
}