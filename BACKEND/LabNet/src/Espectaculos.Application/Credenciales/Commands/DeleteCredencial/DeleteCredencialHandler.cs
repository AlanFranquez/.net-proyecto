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

        // 🔹 1) Buscamos la credencial
        var cred = await _uow.Credenciales.GetByIdAsync(command.CredencialId, ct)
                   ?? throw new KeyNotFoundException("Credencial no encontrada.");

        // 🔹 2) Buscamos el usuario dueño (si existiera)
        var usuario = await _uow.Usuarios.GetByIdAsync(cred.UsuarioId, ct);
        if (usuario is not null)
        {
            // limpiamos la FK y la navegación
            usuario.CredencialId = null;
            usuario.Credencial   = null;

            await _uow.Usuarios.UpdateAsync(usuario, ct);
        }

        // 🔹 3) Eliminamos la credencial
        await _uow.Credenciales.DeleteAsync(command.CredencialId, ct);

        // 🔹 4) Guardamos todo junto
        await _uow.SaveChangesAsync(ct);

        return command.CredencialId;
    }
}