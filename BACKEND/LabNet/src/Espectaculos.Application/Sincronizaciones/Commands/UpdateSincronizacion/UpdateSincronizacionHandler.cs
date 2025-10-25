using Espectaculos.Application.Abstractions;
using Espectaculos.Domain.Entities;
using FluentValidation;
using MediatR;

namespace Espectaculos.Application.Sincronizaciones.Commands.UpdateSincronizacion;

public class UpdateSincronizacionHandler : IRequestHandler<UpdateSincronizacionCommand, Guid>
{
    private readonly IUnitOfWork _uow;
    private readonly IValidator<UpdateSincronizacionCommand> _validator;

    public UpdateSincronizacionHandler(IUnitOfWork uow, IValidator<UpdateSincronizacionCommand> validator)
    {
        _uow = uow;
        _validator = validator;
    }

    public async Task<Guid> Handle(UpdateSincronizacionCommand command, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAsync(command, ct);

        var sync = await _uow.Sincronizaciones.GetByIdAsync(command.SincronizacionId, ct)
                      ?? throw new KeyNotFoundException("Sincronizacion no encontrada.");

        if (command.CreadoEn.HasValue)
            sync.CreadoEn = command.CreadoEn.Value;

        if (command.CantidadItems.HasValue)
            sync.CantidadItems = command.CantidadItems.Value;

        if (command.Tipo is not null)
            sync.Tipo = command.Tipo.Trim();

        if (command.Estado is not null)
            sync.Estado = command.Estado.Trim();

        if (command.DetalleError is not null)
            sync.DetalleError = command.DetalleError.Trim();
        
        if (command.Checksum is not null)
            sync.Checksum = command.Checksum.Trim();

        if (command.DispositivoId.HasValue)
        {
            var dispositivo = await _uow.Dispositivos.GetByIdAsync(command.DispositivoId.Value, ct)
                          ?? throw new KeyNotFoundException("Dispositivo no encontrado.");
            sync.DispositivoId = command.DispositivoId.Value;
            sync.Dispositivo = dispositivo;
        }
        
        await _uow.Sincronizaciones.UpdateAsync(sync, ct);
        await _uow.SaveChangesAsync(ct);

        return sync.DispositivoId;
    }
}
