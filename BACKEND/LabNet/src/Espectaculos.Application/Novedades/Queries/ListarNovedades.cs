namespace Espectaculos.Application.Novedades.Queries;

public class ListarNovedades
{
    
}// ListarNovedadesQuery.cs
using Espectaculos.Application.Abstractions.Repositories;
using Espectaculos.Domain.Enums;
using MediatR;

public record ListarNovedadesQuery(string? Q, NotificacionTipo? Tipo, bool OnlyPublished, bool OnlyActive,
    int Page = 1, int PageSize = 20)
    : IRequest<(IReadOnlyList<NovedadDto> Items, int Total)>;

public record NovedadDto(Guid Id, string Titulo, string? Contenido, NotificacionTipo Tipo,
    bool Publicado, DateTime? DesdeUtc, DateTime? HastaUtc);

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
