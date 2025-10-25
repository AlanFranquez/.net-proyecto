using Espectaculos.Application.Abstractions;
using Espectaculos.Domain.Entities;
using FluentValidation;
using MediatR;

namespace Espectaculos.Application.Roles.Commands.UpdateRol;

public class UpdateRolHandler : IRequestHandler<UpdateRolCommand, Guid>
{
    private readonly IUnitOfWork _uow;
    private readonly IValidator<UpdateRolCommand> _validator;

    public UpdateRolHandler(IUnitOfWork uow, IValidator<UpdateRolCommand> validator)
    {
        _uow = uow;
        _validator = validator;
    }
    
    public async Task<Guid> Handle(UpdateRolCommand command, CancellationToken ct = default)
    {
        await _validator.ValidateAndThrowAsync(command, ct);

        var rol = await _uow.Roles.GetByIdAsync(command.RolId, ct)
                      ?? throw new KeyNotFoundException("Rol no encontrado");
        
        if (command.Tipo is not null)
            rol.Tipo = command.Tipo.Trim();

        if (command.Prioridad.HasValue)
            rol.Prioridad = command.Prioridad.Value;

        if (command.FechaAsignado.HasValue)
            rol.FechaAsignado = command.FechaAsignado.Value;

        if (command.UsuariosIDs is not null)
        {
            var usuariosExistentes = await _uow.Usuarios.ListByIdsAsync(command.UsuariosIDs, ct);
            if (usuariosExistentes.Count() != command.UsuariosIDs.Distinct().Count())
                throw new KeyNotFoundException("Algun usuario enviado no existe.");
            await _uow.Roles.RemoveUsuariosRelacionados(rol.RolId, ct);

            // Reemplazamos la colección de join-entities
            rol.UsuarioRoles = command.UsuariosIDs
                .Distinct()
                .Select(rid => new UsuarioRol()
                {
                    RolId = rol.RolId,
                    UsuarioId = rid
                })
                .ToList();
        }

        await _uow.Roles.UpdateAsync(rol, ct);
        await _uow.SaveChangesAsync(ct);
        return rol.RolId;
    }
}