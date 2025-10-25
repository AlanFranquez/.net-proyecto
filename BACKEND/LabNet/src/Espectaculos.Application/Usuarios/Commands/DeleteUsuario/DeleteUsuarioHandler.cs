using Espectaculos.Application.Abstractions;
using FluentValidation;
using MediatR;

namespace Espectaculos.Application.Usuarios.Commands.DeleteUsuario;

public class DeleteUsuarioHandler : IRequestHandler<DeleteUsuarioCommand, Guid>
{
    private readonly IUnitOfWork _uow;
    private readonly IValidator<DeleteUsuarioCommand> _validator;

    public DeleteUsuarioHandler(IUnitOfWork uow, IValidator<DeleteUsuarioCommand> validator)
    {
        _uow = uow;
        _validator = validator;
    }

    public async Task<Guid> Handle(DeleteUsuarioCommand command, CancellationToken ct = default)
    {
        await _validator.ValidateAndThrowAsync(command, ct);

        await _uow.Usuarios.DeleteAsync(command.UsuarioId, ct);
        await _uow.SaveChangesAsync(ct);
        return command.UsuarioId;
    }
}