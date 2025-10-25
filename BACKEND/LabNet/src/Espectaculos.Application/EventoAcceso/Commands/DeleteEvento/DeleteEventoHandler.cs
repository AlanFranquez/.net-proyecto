using Espectaculos.Application.Abstractions;
using FluentValidation;
using MediatR;

namespace Espectaculos.Application.EventoAcceso.Commands.DeleteEvento;

public class DeleteEventoHandler : IRequestHandler<DeleteEventoCommand, Guid>
{
    private readonly IUnitOfWork _uow;
    private readonly IValidator<DeleteEventoCommand> _validator;

    public DeleteEventoHandler(IUnitOfWork uow, IValidator<DeleteEventoCommand> validator)
    {
        _uow = uow;
        _validator = validator;
    }
    
    public async Task<Guid> Handle(DeleteEventoCommand command, CancellationToken ct = default)
    {
        await _validator.ValidateAndThrowAsync(command, ct);

        await _uow.EventosAccesos.DeleteAsync(command.EventoId, ct);
        await _uow.SaveChangesAsync(ct);
        return command.EventoId;
    }
}