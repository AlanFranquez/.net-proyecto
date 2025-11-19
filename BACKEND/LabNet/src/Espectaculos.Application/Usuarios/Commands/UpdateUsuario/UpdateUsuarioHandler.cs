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

        // Usuario debe venir trackeado por el mismo DbContext del UoW
        var usuario = await _uow.Usuarios.GetByIdAsync(command.UsuarioId, ct)
                      ?? throw new KeyNotFoundException("Usuario no encontrado");
        
        // Campos simples
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

        // Credencial
        if (command.CredencialId.HasValue)
        {
            var credencial = await _uow.Credenciales.GetByIdAsync(command.CredencialId.Value, ct)
                          ?? throw new KeyNotFoundException("Credencial no encontrada.");
            usuario.CredencialId = command.CredencialId.Value;
            usuario.Credencial = credencial;
        }
        
        // ----- ROLES -----
        if (command.RolesIDs is not null)
        {
            var rolesExistentes = await _uow.Roles.ListByIdsAsync(command.RolesIDs, ct);
            if (rolesExistentes.Count() != command.RolesIDs.Distinct().Count())
                throw new KeyNotFoundException("Algún rol enviado no existe.");

            await _uow.Usuarios.RemoveRolesRelacionados(usuario.UsuarioId, ct);

            usuario.UsuarioRoles ??= new List<UsuarioRol>();
            usuario.UsuarioRoles.Clear();

            foreach (var rid in command.RolesIDs.Distinct())
            {
                usuario.UsuarioRoles.Add(new UsuarioRol
                {
                    UsuarioId = usuario.UsuarioId,
                    RolId = rid
                });
            }
        }

        // ----- BENEFICIOS -----
        if (command.BeneficiosIDs is not null)
        {
            var beneficiosExistentes = await _uow.Beneficios.ListByIdsAsync(command.BeneficiosIDs, ct);
            if (beneficiosExistentes.Count() != command.BeneficiosIDs.Distinct().Count())
                throw new KeyNotFoundException("Algún beneficio enviado no existe.");
            
            // Limpio en DB (si tu método hace DELETE directos, está bien)
            await _uow.Usuarios.RemoveBeneficiosRelacionados(usuario.UsuarioId, ct);

            // Me aseguro de que EF vea los cambios en la navegación
            usuario.Beneficios ??= new List<BeneficioUsuario>();
            usuario.Beneficios.Clear();

            foreach (var bid in command.BeneficiosIDs.Distinct())
            {
                usuario.Beneficios.Add(new BeneficioUsuario
                {
                    UsuarioId = usuario.UsuarioId,
                    BeneficioId = bid
                });
            }
        }

        // ----- CANJES -----
        if (command.CanjesIDs is not null)
        {
            // Ojo: esto asume que Canjes ya existen con esos IDs;
            // si no, deberías cargarlos en lugar de new Canje { CanjeId = ... }
            usuario.Canjes = command.CanjesIDs
                .Select(eid => new Canje { CanjeId = eid })
                .ToList();
        }
        
        // ----- DISPOSITIVOS -----
        if (command.DispositivosIDs is not null)
        {
            usuario.Dispositivos = command.DispositivosIDs
                .Select(eid => new Dispositivo { DispositivoId = eid })
                .ToList();
        }

        // IMPORTANTE:
        // Si el usuario fue obtenido con tracking, no hace falta llamar UpdateAsync.
        // EF ya sabe qué cambió. Solo guardamos.
        // await _uow.Usuarios.UpdateAsync(usuario, ct);  // <- mejor NO usarlo aquí

        await _uow.SaveChangesAsync(ct);
        return usuario.UsuarioId;
    }
}
