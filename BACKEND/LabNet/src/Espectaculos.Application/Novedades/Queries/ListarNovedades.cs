using MediatR;
using System.Linq;
using Espectaculos.Application.Abstractions.Repositories;
using Espectaculos.Application.DTOs;

namespace Espectaculos.Application.Novedades.Queries;
public record ListarNovedadesQuery(
    string? Q,
    Espectaculos.Domain.Enums.NotificacionTipo? Tipo, // ⬅️ fully-qualified
    bool OnlyPublished,
    bool OnlyActive,
    int Page = 1,
    int PageSize = 20
) : IRequest<(IReadOnlyList<NovedadDto> Items, int Total)>;

public class ListarNovedadesHandler
    : IRequestHandler<ListarNovedadesQuery, (IReadOnlyList<NovedadDto> Items, int Total)>
{
    private readonly INovedadRepository _repo;
    public ListarNovedadesHandler(INovedadRepository repo) => _repo = repo;

    public async Task<(IReadOnlyList<NovedadDto>, int)> Handle(ListarNovedadesQuery r, CancellationToken ct)
    {
        var (items, total) = await _repo.ListAsync(
            new NovedadFilter(r.Q, r.Tipo, r.OnlyPublished, r.OnlyActive, r.Page, r.PageSize), ct);

        return (items.Select(x => new NovedadDto(
            x.NovedadId, x.Titulo, x.Contenido, x.Tipo, x.Publicado, x.PublicadoDesdeUtc, x.PublicadoHastaUtc)).ToList(), total);
    }
}