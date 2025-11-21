using MediatR;
using Espectaculos.Application.Novedades.Queries;

namespace Espectaculos.WebApi.Endpoints.Novedades;

public static class NovedadesEndpoints
{
    public static IEndpointRouteBuilder MapNovedades(this IEndpointRouteBuilder app)
    {
        // If you call api.MapNovedades() and api = app.MapGroup("/api"),
        // your final route is /api/novedades
        var g = app.MapGroup("/novedades")
            .WithTags("Novedades");

        // READ-ONLY, NO FILTERS: get all novedades, paged
        g.MapGet("",
            async (ISender sender,
                int page = 1,
                int pageSize = 20,
                CancellationToken ct = default) =>
            {
                // q = null, tipo = null, published = false, active = false
                // -> repo.ListAsync will NOT filter by publicado or active
                var (items, total) = await sender.Send(
                    new ListarNovedadesQuery(null, null, false, false, page, pageSize),
                    ct);

                // Wrap it so the JSON is clean: { items: [...], total: N }
                return Results.Ok(new
                {
                    items,
                    total
                });
            });

        return app;
    }
}