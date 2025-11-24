using Espectaculos.Application.Abstractions;
using Espectaculos.Domain.Entities;
using FluentValidation;
using MediatR;

namespace Espectaculos.Application.Credenciales.Commands.UpdateCredencial;

public class UpdateCredencialHandler : IRequestHandler<UpdateCredencialCommand, Guid>
{
    private readonly IUnitOfWork _uow;
    private readonly IValidator<UpdateCredencialCommand> _validator;

    public UpdateCredencialHandler(IUnitOfWork uow, IValidator<UpdateCredencialCommand> validator)
    {
        _uow = uow;
        _validator = validator;
    }
    
    public async Task<Guid> Handle(UpdateCredencialCommand command, CancellationToken ct = default)
    {
        await _validator.ValidateAndThrowAsync(command, ct);

        var credencial = await _uow.Credenciales.GetByIdAsync(command.CredencialId, ct)
                      ?? throw new KeyNotFoundException("Credencial no encontrada");
        
        if (command.Tipo.HasValue)
            credencial.Tipo = command.Tipo.Value;

        if (command.Estado.HasValue)
            credencial.Estado = command.Estado.Value;

        if (command.IdCriptografico is not null)
            credencial.IdCriptografico = command.IdCriptografico.Trim();

        if (command.FechaEmision.HasValue)
            credencial.FechaEmision = command.FechaEmision.Value;
        
        if (command.FechaExpiracion.HasValue)
            credencial.FechaExpiracion = command.FechaExpiracion.Value;
        
        if (command.EventoAccesoIds is not null)
        {
            credencial.EventosAcceso = command.EventoAccesoIds
                .Select(eid => new Domain.Entities.EventoAcceso { EventoId = eid })
                .ToList();
        }


        await _uow.Credenciales.UpdateAsync(credencial, ct);
        await _uow.SaveChangesAsync(ct);
        return credencial.CredencialId;
    }
}