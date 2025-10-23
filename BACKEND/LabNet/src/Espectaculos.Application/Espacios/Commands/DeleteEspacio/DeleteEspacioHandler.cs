using Espectaculos.Application.Abstractions;
using FluentValidation;
using MediatR;

namespace Espectaculos.Application.Espacios.Commands.DeleteEspacio;

public class DeleteEspacioHandler : IRequestHandler<DeleteEspacioCommand, Guid>
{
    private readonly IUnitOfWork _uow;
    private readonly IValidator<DeleteEspacioCommand> _validator;

    public DeleteEspacioHandler(IUnitOfWork uow, IValidator<DeleteEspacioCommand> validator)
    {
        _uow = uow;
        _validator = validator;
    }
    
    public async Task<Guid> Handle(DeleteEspacioCommand command, CancellationToken ct = default)
    {
        await _validator.ValidateAndThrowAsync(command, ct);

        await _uow.Espacios.DeleteAsync(command.Id, ct);
        await _uow.SaveChangesAsync(ct);
        return command.Id;
    }
}