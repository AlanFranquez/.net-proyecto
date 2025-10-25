using Espectaculos.Application.Abstractions;
using FluentValidation;
using MediatR;

namespace Espectaculos.Application.Sincronizaciones.Commands.DeleteSincronizacion;

public class DeleteSincronizacionHandler : IRequestHandler<DeleteSincronizacionCommand, Guid>
{
    private readonly IUnitOfWork _uow;
    private readonly IValidator<DeleteSincronizacionCommand> _validator;

    public DeleteSincronizacionHandler(IUnitOfWork uow, IValidator<DeleteSincronizacionCommand> validator)
    {
        _uow = uow;
        _validator = validator;
    }
    
    public async Task<Guid> Handle(DeleteSincronizacionCommand command, CancellationToken ct = default)
    {
        await _validator.ValidateAndThrowAsync(command, ct);

        await _uow.Sincronizaciones.DeleteAsync(command.SincronizacionId, ct);
        await _uow.SaveChangesAsync(ct);
        return command.SincronizacionId;
    }
}