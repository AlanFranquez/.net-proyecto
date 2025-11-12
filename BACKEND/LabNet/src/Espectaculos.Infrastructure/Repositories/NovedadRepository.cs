using Espectaculos.Application.Abstractions.Repositories;
using Espectaculos.Domain.Entities;
using Espectaculos.Domain.Enums;
using Espectaculos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Espectaculos.Infrastructure.Repositories;
using AppRepos = Espectaculos.Application.Abstractions.Repositories;

public class NovedadRepository : AppRepos.INovedadRepository
{
    private readonly EspectaculosDbContext _db;
    public NovedadRepository(EspectaculosDbContext db) => _db = db;

    public async Task AddAsync(Novedad entity, CancellationToken ct)
        => await _db.Set<Novedad>().AddAsync(entity, ct);

    public Task<Novedad?> GetByIdAsync(Guid id, CancellationToken ct)
        => _db.Set<Novedad>().FirstOrDefaultAsync(x => x.NovedadId == id, ct);

    public Task UpdateAsync(Novedad entity, CancellationToken ct)
    { _db.Set<Novedad>().Update(entity); return Task.CompletedTask; }

    public Task DeleteAsync(Novedad entity, CancellationToken ct)
    { _db.Set<Novedad>().Remove(entity); return Task.CompletedTask; }

    public async Task<(IReadOnlyList<Novedad> Items, int Total)> ListAsync(NovedadFilter f, CancellationToken ct)
    {
        var q = _db.Set<Novedad>().AsQueryable();

        if (!string.IsNullOrWhiteSpace(f.Q))
            q = q.Where(x => x.Titulo.Contains(f.Q) || (x.Contenido != null && x.Contenido.Contains(f.Q)));
        if (f.Tipo.HasValue) q = q.Where(x => x.Tipo == f.Tipo);
        if (f.OnlyPublished) q = q.Where(x => x.Publicado);

        if (f.OnlyActiveUtcNow)
        {
            var now = DateTime.UtcNow;
            q = q.Where(x => x.Publicado
                             && (!x.PublicadoDesdeUtc.HasValue || x.PublicadoDesdeUtc <= now)
                             && (!x.PublicadoHastaUtc.HasValue || x.PublicadoHastaUtc >= now));
        }

        var total = await q.CountAsync(ct);

        var items = await q.OrderByDescending(x => x.Publicado)
                           .ThenByDescending(x => x.PublicadoDesdeUtc)
                           .ThenByDescending(x => x.CreadoEnUtc)
                           .Skip((f.Page - 1) * f.PageSize)
                           .Take(f.PageSize)
                           .ToListAsync(ct);

        return (items, total);
    }
}
