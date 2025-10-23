using Espectaculos.Application.Abstractions;
using FluentValidation;
using MediatR;
using Espectaculos.Domain.Entities;

namespace Espectaculos.Application.ReglaDeAcceso.Commands.CreateReglaDeAcceso;

public class CreateReglaHandler : IRequestHandler<CreateReglaCommand, Guid>
{
    private readonly IUnitOfWork _uow;
    private readonly IValidator<CreateReglaCommand> _validator;

    public CreateReglaHandler(IUnitOfWork uow, IValidator<CreateReglaCommand> validator)
    {
        _uow = uow;
        _validator = validator;
    }
    
    public async Task<Guid> Handle(CreateReglaCommand command, CancellationToken ct = default)
    {
        await _validator.ValidateAndThrowAsync(command, ct);
        if (command.VigenciaInicio.HasValue && command.VigenciaInicio > command.VigenciaFin)
            throw new ArgumentException("VigenciaInicio debe ser anterior o igual a VigenciaFin");
        
        var e = new Espectaculos.Domain.Entities.ReglaDeAcceso
        {
            ReglaId = Guid.NewGuid(),
            VentanaHoraria = command.VentanaHoraria.Trim(),
            VigenciaInicio = command.VigenciaInicio,
            VigenciaFin = command.VigenciaFin,
            Prioridad = command.Prioridad,
            Politica = command.Politica,
            RequiereBiometriaConfirmacion = command.RequiereBiometriaConfirmacion.Equals(true),
        };
        if (command.EspaciosIDs is not null)
        {
            var espaciosExistentes = await _uow.Espacios.ListByIdsAsync(command.EspaciosIDs, ct);
            if (espaciosExistentes.Count() != command.EspaciosIDs.Distinct().Count())
                throw new KeyNotFoundException("Alguna regla enviada no existe.");

            // Reemplazamos la colección de join-entities
            e.Espacios = command.EspaciosIDs
                .Distinct()
                .Select(rid => new EspacioReglaDeAcceso
                {
                    EspacioId = rid,
                    ReglaId = e.ReglaId
                })
                .ToList();
        }

        await _uow.Reglas.AddAsync(e, ct);
        await _uow.SaveChangesAsync(ct);

        return e.ReglaId;
    }
    
}