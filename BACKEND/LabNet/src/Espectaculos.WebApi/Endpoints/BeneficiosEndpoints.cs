using Espectaculos.Application.Beneficios.Commands.CreateBeneficio;
using Espectaculos.Application.Beneficios.Commands.UpdateBeneficio;
using Espectaculos.Application.Beneficios.Commands.CanjearBeneficio;
using Espectaculos.Application.Beneficios.Queries.ListBeneficios;
using Espectaculos.Application.Beneficios.Queries.GetBeneficioById;
using MediatR;
using Microsoft.AspNetCore.Builder;
using System.Text.Json;
using Espectaculos.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Espectaculos.WebApi.Endpoints;

public static class BeneficiosEndpoints
{
    public static void MapBeneficiosEndpoints(this IEndpointRouteBuilder api)
    {
        api.MapGet("/beneficios", async (IMediator mediator, [FromServices] RabbitMqService rabbit) =>
        { 

            try {
var items = await mediator.Send(new ListBeneficiosQuery());
            rabbit.SendMessage("beneficios.listar", $"Se ha listado los beneficios {DateTime.Now}: {items.Count} beneficios obtenidos");
            return Results.Ok(items);
            } catch(Exception ex) {
                rabbit.SendMessage("beneficios.listar-dlq", $"Error al listar beneficios: {ex.Message}");
                return Results.StatusCode(500);
            }
            
        });

        api.MapGet("/beneficios/{id:guid}", async (Guid id, IMediator mediator, [FromServices] RabbitMqService rabbit) =>
        {
            try {
                var item = await mediator.Send(new GetBeneficioByIdQuery(id));
                rabbit.SendMessage("beneficios.obtener", $"Se ha obtenido el beneficio con id {id}");
                return item is null ? Results.NotFound() : Results.Ok(item);
            } catch(Exception ex) {
                rabbit.SendMessage("beneficios.obtener-dlq", $"Error al obtener beneficio con id {id}: {ex.Message}");
                return Results.StatusCode(500);
            }
            
            
            
            
            
            return item is null ? Results.NotFound() : Results.Ok(item);
        });

        api.MapPost("/beneficios", async (Espectaculos.WebApi.Endpoints.Dtos.CreateBeneficioDto dto, IMediator mediator, [FromServices] RabbitMqService rabbit) =>
        {
            if (!TryParseTipo(dto.Tipo, out Espectaculos.Domain.Enums.BeneficioTipo tipo))
                return Results.BadRequest("Tipo inválido");



            var cmd = new CreateBeneficioCommand(dto.Nombre, tipo, dto.Descripcion, dto.VigenciaInicio, dto.VigenciaFin, dto.CupoTotal);
            var id = await mediator.Send(cmd);
            rabbit.SendMessage("beneficios", "Se ha creado el beneficio correctamente");
            return Results.Created($"/beneficios/{id}", new { id });
        });

        api.MapPut("/beneficios/{id:guid}", async (Guid id, UpdateBeneficioCommand cmd, IMediator mediator, [FromServices] RabbitMqService rabbit) =>
        {
            cmd.Id = id;

            try
            {
                var ok = await mediator.Send(cmd);
                if (!ok) return Results.NotFound();

                var updated = await mediator.Send(new GetBeneficioByIdQuery(id));
                if (updated is null) return Results.NotFound();

                rabbit.SendMessage("beneficios", $"Editado el beneficio con id {cmd.Id}");
                return Results.Ok(new { id = updated.Id, nombre = updated.Nombre, message = "Beneficio actualizado" });
            }
            catch (Espectaculos.Application.Common.Exceptions.ConcurrencyException)
            {
                rabbit.SendToDlq("beneficios-dlq", $"No se ha podido editar el beneficio con id: {cmd.Id}");
                return Results.Conflict("Conflicto de concurrencia");
            }
        });


        api.MapPost("/beneficios/{id:guid}/canjear", 
async (Guid id, CanjearBeneficioCommand cmd, [FromServices] RabbitMqService rabbit) =>
{
    if (id != cmd.BeneficioId) 
        return Results.BadRequest("Id mismatch");

    rabbit.EnqueueCanje(cmd.BeneficioId, cmd.UsuarioId);

    return Results.Accepted();
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
