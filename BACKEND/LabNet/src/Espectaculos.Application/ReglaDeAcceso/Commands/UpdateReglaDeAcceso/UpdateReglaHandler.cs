using Espectaculos.Application.Abstractions;
using Espectaculos.Domain.Entities;
using FluentValidation;
using MediatR;

namespace Espectaculos.Application.ReglaDeAcceso.Commands.UpdateReglaDeAcceso;

public class UpdateReglaHandler : IRequestHandler<UpdateReglaCommand, Guid>
{
    private readonly IUnitOfWork _uow;
    private readonly IValidator<UpdateReglaCommand> _validator;

    public UpdateReglaHandler(IUnitOfWork uow, IValidator<UpdateReglaCommand> validator)
    {
        _uow = uow;
        _validator = validator;
    }
    
    public async Task<Guid> Handle(UpdateReglaCommand command, CancellationToken ct = default)
    {
        await _validator.ValidateAndThrowAsync(command, ct);

        var regla = await _uow.Reglas.GetByIdAsync(command.ReglaId, ct)
                      ?? throw new KeyNotFoundException("Regla no encontrada");
        
        if (command.VentanaHoraria is not null)
            regla.VentanaHoraria = command.VentanaHoraria.Trim();

        if (command.VigenciaInicio.HasValue)
            regla.VigenciaInicio = command.VigenciaInicio.Value;

        if (command.VigenciaFin.HasValue)
            regla.VigenciaFin = command.VigenciaFin.Value;

        if (command.Prioridad.HasValue)
            regla.Prioridad = command.Prioridad.Value;
        
        if (command.Politica.HasValue)
            regla.Politica = command.Politica.Value;
        
        if (command.RequiereBiometriaConfirmacion.HasValue)
            regla.RequiereBiometriaConfirmacion = command.RequiereBiometriaConfirmacion.Value;

        if (command.EspaciosIDs is not null)
        {
            var espaciosExistentes = await _uow.Espacios.ListByIdsAsync(command.EspaciosIDs, ct);
            if (espaciosExistentes.Count() != command.EspaciosIDs.Distinct().Count())
                throw new KeyNotFoundException("Algun espacio enviado no existe.");
                
            await _uow.Reglas.RemoveEspaciosRelacionados(regla.ReglaId, ct);
            // Reemplazamos la colección de join-entities
            regla.Espacios = command.EspaciosIDs
                .Distinct()
                .Select(rid => new EspacioReglaDeAcceso
                {
                    EspacioId = rid,
                    ReglaId = regla.ReglaId
                })
                .ToList();
        }

        await _uow.Reglas.UpdateAsync(regla, ct);
        await _uow.SaveChangesAsync(ct);
        return regla.ReglaId;
    }
}