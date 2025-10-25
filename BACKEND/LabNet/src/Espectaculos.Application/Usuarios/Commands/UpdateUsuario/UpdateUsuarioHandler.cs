using Espectaculos.Application.Abstractions;
using Espectaculos.Domain.Entities;
using FluentValidation;
using MediatR;

namespace Espectaculos.Application.Usuarios.Commands.UpdateUsuario;

public class UpdateUsuarioHandler : IRequestHandler<UpdateUsuarioCommand, Guid>
{
    private readonly IUnitOfWork _uow;
    private readonly IValidator<UpdateUsuarioCommand> _validator;

    public UpdateUsuarioHandler(IUnitOfWork uow, IValidator<UpdateUsuarioCommand> validator)
    {
        _uow = uow;
        _validator = validator;
    }
    
    public async Task<Guid> Handle(UpdateUsuarioCommand command, CancellationToken ct = default)
    {
        await _validator.ValidateAndThrowAsync(command, ct);

        var usuario = await _uow.Usuarios.GetByIdAsync(command.UsuarioId, ct)
                      ?? throw new KeyNotFoundException("Usuario no encontrado");
        
        if (command.Nombre is not null)
            usuario.Nombre = command.Nombre.Trim();
        
        if (command.Apellido is not null)
            usuario.Apellido = command.Apellido.Trim();
        
        if (command.Email is not null)
            usuario.Email = command.Email.Trim();
        
        if (command.Documento is not null)
            usuario.Documento = command.Documento.Trim();
        
        if (command.Password is not null)
            usuario.PasswordHash = command.Password.Trim();

        if (command.Estado.HasValue)
            usuario.Estado = command.Estado.Value;

        if (command.CredencialId.HasValue)
        {
            var credencial = await _uow.Credenciales.GetByIdAsync(command.CredencialId.Value, ct)
                          ?? throw new KeyNotFoundException("Credencial no encontrada.");
            usuario.CredencialId = command.CredencialId.Value;
            usuario.Credencial = credencial;
        }
        
        if (command.RolesIDs is not null)
        {
            var rolesExistentes = await _uow.Roles.ListByIdsAsync(command.RolesIDs, ct);
            if (rolesExistentes.Count() != command.RolesIDs.Distinct().Count())
                throw new KeyNotFoundException("Algun rol enviado no existe.");
            await _uow.Usuarios.RemoveRolesRelacionados(usuario.UsuarioId, ct);

            // Reemplazamos la colección de join-entities
            usuario.UsuarioRoles = command.RolesIDs
                .Distinct()
                .Select(rid => new UsuarioRol
                {
                    UsuarioId = usuario.UsuarioId,
                    RolId = rid
                })
                .ToList();
        }

        if (command.BeneficiosIDs is not null)
        {
            var beneficiosExistentes = await _uow.Beneficios.ListByIdsAsync(command.BeneficiosIDs, ct);
            if (beneficiosExistentes.Count() != command.BeneficiosIDs.Distinct().Count())
                throw new KeyNotFoundException("Algún beneficio enviado no existe.");
            
            await _uow.Usuarios.RemoveBeneficiosRelacionados(usuario.UsuarioId, ct);

            usuario.Beneficios = command.BeneficiosIDs
                .Distinct()
                .Select(bid => new BeneficioUsuario()
                {
                    UsuarioId = usuario.UsuarioId,
                    BeneficioId = bid
                })
                .ToList();
        }

        if (command.CanjesIDs is not null)
        {
            usuario.Canjes = command.CanjesIDs
                .Select(eid => new Canje { CanjeId = eid })
                .ToList();
        }
        
        if (command.DispositivosIDs is not null)
        {
            usuario.Dispositivos = command.DispositivosIDs
                .Select(eid => new Dispositivo { DispositivoId = eid })
                .ToList();
        }


        await _uow.Usuarios.UpdateAsync(usuario, ct);
        await _uow.SaveChangesAsync(ct);
        return usuario.UsuarioId;
    }
}
