using MediatR;
using NTipo = Espectaculos.Domain.Enums.NotificacionTipo;
using Espectaculos.Application.Novedades.Queries;
using Espectaculos.Application.Novedades.Commands.CreateNovedad;
using Espectaculos.Application.Novedades.Commands.UpdateNovedad;
using Espectaculos.Application.Novedades.Commands.PublishUnpublish;
using Espectaculos.Application.Novedades.Commands.DeleteNovedad;

namespace Espectaculos.WebApi.Endpoints.Novedades;

public static class NovedadesEndpoints
{
    public static IEndpointRouteBuilder MapNovedades(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/novedades").WithTags("Novedades");

        g.MapGet("", async (ISender sender, string? q, NTipo? tipo, bool published = false, bool active = false,
                            int page = 1, int pageSize = 20)
            => await sender.Send(new ListarNovedadesQuery(q, tipo, published, active, page, pageSize)));

        g.MapPost("", async (CreateNovedadCommand cmd, ISender sender) =>
        {
            var id = await sender.Send(cmd);
            return Results.Created($"/api/novedades/{id}", new { id });
        });

        g.MapPut("{id:guid}", async (Guid id, UpdateNovedadCommand cmd, ISender sender) =>
        {
            if (id != cmd.NovedadId) return Results.BadRequest("Id mismatch");
            await sender.Send(cmd);
            return Results.NoContent();
        });

        g.MapPut("{id:guid}/publish", async (Guid id, DateTime? desdeUtc, DateTime? hastaUtc, ISender sender) =>
        {
            await sender.Send(new PublishNovedadCommand(id, desdeUtc, hastaUtc));
            return Results.NoContent();
        });

        g.MapPut("{id:guid}/unpublish", async (Guid id, ISender sender) =>
        {
            await sender.Send(new UnpublishNovedadCommand(id));
            return Results.NoContent();
        });

        g.MapDelete("{id:guid}", async (Guid id, ISender sender) =>
        {
            await sender.Send(new DeleteNovedadCommand(id));
            return Results.NoContent();
        });

        return app;
    }
}
