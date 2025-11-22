using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Espectaculos.Application.Abstractions;
using Espectaculos.Application.Abstractions.Security;
using Espectaculos.Application.Services;
using Espectaculos.Domain.Entities;
using Espectaculos.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;


namespace Espectaculos.Application.Usuarios.Commands.CreateUsuario
{
    public class CreateUsuarioHandler : IRequestHandler<CreateUsuarioCommand, Guid>
    {
        private readonly IUnitOfWork _uow;
        private readonly IValidator<CreateUsuarioCommand> _validator;
        private readonly ICognitoService _cognitoService;
        private readonly IConfiguration _config;
        private readonly IPasswordHasher _passwordHasher;

        public CreateUsuarioHandler(
            IUnitOfWork uow,
            IValidator<CreateUsuarioCommand> validator,
            ICognitoService cognitoService,
            IConfiguration config,
            IPasswordHasher passwordHasher)
        {
            _uow = uow;
            _validator = validator;
            _cognitoService = cognitoService;
            _config = config;
            _passwordHasher = passwordHasher;
        }

        public async Task<Guid> Handle(CreateUsuarioCommand command, CancellationToken ct)
        {
            await _validator.ValidateAndThrowAsync(command, ct);

            // 1) Register user in Cognito (still uses the raw password)
            var cognitoSub = await _cognitoService.RegisterUserAsync(command.Email, command.Password, ct);

            // 2) Decide estado (special-case Backoffice admin)
            var backofficeAdminEmail = _config["Backoffice:AdminEmail"];
            var estado = UsuarioEstado.Activo;

            if (!string.IsNullOrWhiteSpace(backofficeAdminEmail) &&
                string.Equals(command.Email, backofficeAdminEmail, StringComparison.OrdinalIgnoreCase))
            {
                estado = UsuarioEstado.Admin;
            }

            // 3) Hash password for local persistence
            var trimmedPassword = command.Password?.Trim() ?? string.Empty;
            var passwordHash = _passwordHasher.Hash(trimmedPassword);

            // 4) Create domain user
            var usuario = new Usuario
            {
                UsuarioId = Guid.NewGuid(),
                Nombre = command.Nombre,
                Apellido = command.Apellido,
                Email = command.Email,
                Documento = command.Documento,
                PasswordHash = trimmedPassword,
                Estado = estado,
                Credencial = null,
                CredencialId = null,
                UsuarioRoles = new List<UsuarioRol>(),
                Dispositivos = new List<Dispositivo>(),
                Beneficios = new List<BeneficioUsuario>(),
                Canjes = new List<Canje>()
            };

            // 5) Roles
            if (command.RolesIDs is not null && command.RolesIDs.Any())
            {
                var rolesExistentes = await _uow.Roles.ListByIdsAsync(command.RolesIDs, ct);
                if (rolesExistentes.Count() != command.RolesIDs.Distinct().Count())
                    throw new KeyNotFoundException("Algun rol enviado no existe.");

                usuario.UsuarioRoles = command.RolesIDs
                    .Distinct()
                    .Select(rid => new UsuarioRol
                    {
                        UsuarioId = usuario.UsuarioId,
                        RolId = rid
                    })
                    .ToList();
            }

            await _uow.Usuarios.AddAsync(usuario, ct);
            await _uow.SaveChangesAsync(ct);

            return usuario.UsuarioId;
        }
    }
}
