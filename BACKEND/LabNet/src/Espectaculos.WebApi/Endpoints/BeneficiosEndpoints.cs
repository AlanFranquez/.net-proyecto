using Espectaculos.Application.Beneficios.Commands.CreateBeneficio;
using Espectaculos.Application.Beneficios.Commands.UpdateBeneficio;
using Espectaculos.Application.Beneficios.Commands.CanjearBeneficio;
using Espectaculos.Application.Beneficios.Queries.ListBeneficios;
using Espectaculos.Application.Beneficios.Queries.GetBeneficioById;
using MediatR;
using Microsoft.AspNetCore.Builder;
using System.Text.Json;

namespace Espectaculos.WebApi.Endpoints;

public static class BeneficiosEndpoints
{
    public static void MapBeneficiosEndpoints(this IEndpointRouteBuilder api)
    {
        api.MapGet("/beneficios", async (IMediator mediator) =>
        {
            var items = await mediator.Send(new ListBeneficiosQuery());
            return Results.Ok(items);
        });

        api.MapGet("/beneficios/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var item = await mediator.Send(new GetBeneficioByIdQuery(id));
            return item is null ? Results.NotFound() : Results.Ok(item);
        });

        api.MapPost("/beneficios", async (Espectaculos.WebApi.Endpoints.Dtos.CreateBeneficioDto dto, IMediator mediator) =>
        {
            // parse Tipo (string or numeric)
            if (!TryParseTipo(dto.Tipo, out Espectaculos.Domain.Enums.BeneficioTipo tipo))
                return Results.BadRequest("Tipo inválido");

            var cmd = new CreateBeneficioCommand(dto.Nombre, tipo, dto.Descripcion, dto.VigenciaInicio, dto.VigenciaFin, dto.CupoTotal);
            var id = await mediator.Send(cmd);
            return Results.Created($"/beneficios/{id}", new { id });
        });

        api.MapPut("/beneficios/{id:guid}", async (Guid id, Espectaculos.WebApi.Endpoints.Dtos.UpdateBeneficioDto dto, IMediator mediator) =>
        {
            if (id != dto.Id) return Results.BadRequest("Id mismatch");

            Espectaculos.Domain.Enums.BeneficioTipo? tipo = null;
            if (dto.Tipo != null)
            {
                if (!TryParseTipo(dto.Tipo, out var parsed)) return Results.BadRequest("Tipo inválido");
                tipo = parsed;
            }

            var cmd = new UpdateBeneficioCommand(dto.Id, dto.Nombre, dto.Descripcion, dto.VigenciaInicio, dto.VigenciaFin, dto.CupoTotal);
            try
            {
                var ok = await mediator.Send(cmd);
                if (!ok) return Results.NotFound();

                // fetch updated entity to return current rowVersion and fields
                var updated = await mediator.Send(new Espectaculos.Application.Beneficios.Queries.GetBeneficioById.GetBeneficioByIdQuery(id));
                if (updated is null) return Results.NotFound();

                return Results.Ok(new { id = updated.BeneficioId, nombre = updated.Nombre, message = "Beneficio actualizado" });
            }
            catch (Espectaculos.Application.Common.Exceptions.ConcurrencyException)
            {
                return Results.Conflict("Conflicto de concurrencia");
            }
        });

        api.MapPost("/beneficios/{id:guid}/canjear", async (Guid id, CanjearBeneficioCommand cmd, IMediator mediator) =>
        {
            if (id != cmd.BeneficioId) return Results.BadRequest("Id mismatch");
            try
            {
                var canjeId = await mediator.Send(cmd);
                return Results.Ok(new { canjeId });
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (Espectaculos.Application.Common.Exceptions.ConcurrencyException)
            {
                return Results.Conflict("Conflicto de concurrencia");
            }
        });
    }

    private static bool TryParseTipo(object tipoObj, out Espectaculos.Domain.Enums.BeneficioTipo tipo)
    {
        tipo = default;
        if (tipoObj is null) return false;
        if (tipoObj is string s)
        {
            return Enum.TryParse<Espectaculos.Domain.Enums.BeneficioTipo>(s, ignoreCase: true, out tipo);
        }
        if (tipoObj is JsonElement je)
        {
            if (je.ValueKind == JsonValueKind.String) return Enum.TryParse<Espectaculos.Domain.Enums.BeneficioTipo>(je.GetString(), true, out tipo);
            if (je.ValueKind == JsonValueKind.Number && je.TryGetInt32(out var i)) { tipo = (Espectaculos.Domain.Enums.BeneficioTipo)i; return true; }
            return false;
        }
        if (tipoObj is int i2) { tipo = (Espectaculos.Domain.Enums.BeneficioTipo)i2; return true; }
        try
        {
            // last resort: try convert to string then parse
            var s2 = tipoObj.ToString();
            return Enum.TryParse<Espectaculos.Domain.Enums.BeneficioTipo>(s2, ignoreCase: true, out tipo);
        }
        catch { return false; }
    }
}
