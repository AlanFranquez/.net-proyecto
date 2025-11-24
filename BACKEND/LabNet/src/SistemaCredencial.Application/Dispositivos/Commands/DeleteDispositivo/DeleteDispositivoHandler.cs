using Espectaculos.Application.Abstractions;
using FluentValidation;
using MediatR;

namespace Espectaculos.Application.Dispositivos.Commands.DeleteDispositivo;

public class DeleteDispositivoHandler : IRequestHandler<DeleteDispositivoCommand, Guid>
{
    private readonly IUnitOfWork _uow;
    private readonly IValidator<DeleteDispositivoCommand> _validator;

    public DeleteDispositivoHandler(IUnitOfWork uow, IValidator<DeleteDispositivoCommand> validator)
    {
        _uow = uow;
        _validator = validator;
    }
    
    public async Task<Guid> Handle(DeleteDispositivoCommand command, CancellationToken ct = default)
    {
        await _validator.ValidateAndThrowAsync(command, ct);

        await _uow.Dispositivos.DeleteAsync(command.DispositivoId, ct);
        await _uow.SaveChangesAsync(ct);
        return command.DispositivoId;
    }
}