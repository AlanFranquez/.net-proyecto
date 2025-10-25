using Espectaculos.Application.Abstractions;
using FluentValidation;
using MediatR;

namespace Espectaculos.Application.Credenciales.Commands.DeleteCredencial;

public class DeleteCredencialHandler : IRequestHandler<DeleteCredencialCommand, Guid>
{
    private readonly IUnitOfWork _uow;
    private readonly IValidator<DeleteCredencialCommand> _validator;

    public DeleteCredencialHandler(IUnitOfWork uow, IValidator<DeleteCredencialCommand> validator)
    {
        _uow = uow;
        _validator = validator;
    }
    
    public async Task<Guid> Handle(DeleteCredencialCommand command, CancellationToken ct = default)
    {
        await _validator.ValidateAndThrowAsync(command, ct);

        await _uow.Credenciales.DeleteAsync(command.CredencialId, ct);
        await _uow.SaveChangesAsync(ct);
        return command.CredencialId;
    }
}