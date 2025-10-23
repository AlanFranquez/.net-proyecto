using Espectaculos.Application.Abstractions;
using Espectaculos.Domain.Entities;
using Microsoft.AspNetCore.Builder;

namespace Espectaculos.WebApi.Endpoints;

public static class CanjesEndpoints
{
    public static void MapCanjesEndpoints(this IEndpointRouteBuilder api)
    {
        // GET /api/canjes?usuarioId={guid}&beneficioId={guid}
        api.MapGet("/canjes", async (Guid? usuarioId, Guid? beneficioId, IUnitOfWork uow) =>
        {
            // if both null, return bad request to avoid returning everything accidentally
            if (usuarioId is null && beneficioId is null)
                return Results.BadRequest("Se requiere al menos usuarioId o beneficioId como filtro");

            if (usuarioId is not null)
            {
                var list = await uow.Canjes.ListByUsuarioAsync(usuarioId.Value);
                return Results.Ok(list.Select(c => new {
                    c.CanjeId,
                    c.BeneficioId,
                    c.UsuarioId,
                    c.Fecha,
                    Estado = c.Estado.ToString(),
                    c.VerificacionBiometrica,
                    c.Firma
                }));
            }

            var listByBenef = await uow.Canjes.ListByBeneficioAsync(beneficioId!.Value);
            return Results.Ok(listByBenef.Select(c => new {
                c.CanjeId,
                c.BeneficioId,
                c.UsuarioId,
                c.Fecha,
                Estado = c.Estado.ToString(),
                c.VerificacionBiometrica,
                c.Firma
            }));
        });

        // GET /api/beneficios/{id}/canjes
        api.MapGet("/beneficios/{id:guid}/canjes", async (Guid id, IUnitOfWork uow) =>
        {
            var list = await uow.Canjes.ListByBeneficioAsync(id);
            return Results.Ok(list.Select(c => new {
                c.CanjeId,
                c.BeneficioId,
                c.UsuarioId,
                c.Fecha,
                Estado = c.Estado.ToString(),
                c.VerificacionBiometrica,
                c.Firma
            }));
        });
    }
}
