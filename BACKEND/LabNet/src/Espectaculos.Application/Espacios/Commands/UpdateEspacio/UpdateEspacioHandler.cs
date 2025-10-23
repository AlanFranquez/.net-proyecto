using Espectaculos.Application.Abstractions;
using Espectaculos.Domain.Entities;
using FluentValidation;
using MediatR;

namespace Espectaculos.Application.Espacios.Commands.UpdateEspacio;

public class UpdateEspacioHandler : IRequestHandler<UpdateEspacioCommand, Guid>
{
    private readonly IUnitOfWork _uow;
    private readonly IValidator<UpdateEspacioCommand> _validator;

    public UpdateEspacioHandler(IUnitOfWork uow, IValidator<UpdateEspacioCommand> validator)
    {
        _uow = uow;
        _validator = validator;
    }
    
    public async Task<Guid> Handle(UpdateEspacioCommand command, CancellationToken ct = default)
    {
        await _validator.ValidateAndThrowAsync(command, ct);

        var espacio = await _uow.Espacios.GetByIdAsync(command.Id, ct)
                      ?? throw new KeyNotFoundException("Espacio no encontrado");
        
        if (command.Nombre is not null)
            espacio.Nombre = command.Nombre.Trim();

        if (command.Activo.HasValue)
            espacio.Activo = command.Activo.Value;

        if (command.Tipo.HasValue)
            espacio.Tipo = command.Tipo.Value;

        if (command.Modo.HasValue)
            espacio.Modo = command.Modo.Value;

        if (command.ReglaIds is not null)
        {
            var reglasExistentes = await _uow.Reglas.ListByIdsAsync(command.ReglaIds, ct);
            if (reglasExistentes.Count() != command.ReglaIds.Distinct().Count())
                throw new KeyNotFoundException("Alguna regla enviada no existe.");

            // Reemplazamos la colección de join-entities
            espacio.Reglas = command.ReglaIds
                .Distinct()
                .Select(rid => new EspacioReglaDeAcceso
                {
                    EspacioId = espacio.Id, // importante si la join-entity tiene esta propiedad
                    ReglaId = rid
                    // Si tu join-entity tiene otras props (ej: CreatedAt), setéalas aquí
                })
                .ToList();
        }

        if (command.BeneficioIds is not null)
        {
            var beneficiosExistentes = await _uow.Beneficios.ListByIdsAsync(command.BeneficioIds, ct);
            if (beneficiosExistentes.Count() != command.BeneficioIds.Distinct().Count())
                throw new KeyNotFoundException("Algún beneficio enviado no existe.");

            espacio.Beneficios = command.BeneficioIds
                .Distinct()
                .Select(bid => new BeneficioEspacio
                {
                    EspacioId = espacio.Id,
                    BeneficioId = bid
                })
                .ToList();
        }

        if (command.EventoAccesoIds is not null)
        {
            espacio.EventoAccesos = command.EventoAccesoIds
                .Select(eid => new EventoAcceso { EventoId = eid })
                .ToList();
        }


        await _uow.Espacios.UpdateAsync(espacio, ct);
        await _uow.SaveChangesAsync(ct);
        return espacio.Id;
    }
}