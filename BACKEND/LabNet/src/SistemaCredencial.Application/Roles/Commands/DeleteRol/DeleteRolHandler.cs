using Espectaculos.Application.Abstractions;
using FluentValidation;
using MediatR;

namespace Espectaculos.Application.Roles.Commands.DeleteRol;

public class DeleteRolHandler : IRequestHandler<DeleteRolCommand, Guid>
{
    private readonly IUnitOfWork _uow;
    private readonly IValidator<DeleteRolCommand> _validator;

    public DeleteRolHandler(IUnitOfWork uow, IValidator<DeleteRolCommand> validator)
    {
        _uow = uow;
        _validator = validator;
    }
    
    public async Task<Guid> Handle(DeleteRolCommand command, CancellationToken ct = default)
    {
        await _validator.ValidateAndThrowAsync(command, ct);

        await _uow.Roles.DeleteAsync(command.RolId, ct);
        await _uow.SaveChangesAsync(ct);
        return command.RolId;
    }
}