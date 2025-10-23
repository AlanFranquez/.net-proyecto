using Espectaculos.Application.Abstractions;
using FluentValidation;
using MediatR;

namespace Espectaculos.Application.ReglaDeAcceso.Commands.DeleteReglaDeAcceso;

public class DeleteReglaHandler: IRequestHandler<DeleteReglaCommand, Guid>
{
    private readonly IUnitOfWork _uow;
    private readonly IValidator<DeleteReglaCommand> _validator;

    public DeleteReglaHandler(IUnitOfWork uow, IValidator<DeleteReglaCommand> validator)
    {
        _uow = uow;
        _validator = validator;
    }
    
    public async Task<Guid> Handle(DeleteReglaCommand command, CancellationToken ct = default)
    {
        await _validator.ValidateAndThrowAsync(command, ct);

        await _uow.Reglas.DeleteAsync(command.ReglaId, ct);
        await _uow.SaveChangesAsync(ct);
        return command.ReglaId;
    }
}