using Espectaculos.Application.Abstractions;
using Espectaculos.Application.Services;

using Espectaculos.Domain.Entities;
using Espectaculos.Domain.Enums;
using FluentValidation;
using MediatR;

namespace Espectaculos.Application.Usuarios.Commands.CreateUsuario
{
    public class CreateUsuarioHandler : IRequestHandler<CreateUsuarioCommand, Guid>
    {
        private readonly IUnitOfWork _uow;
        private readonly IValidator<CreateUsuarioCommand> _validator;
        private readonly ICognitoService _cognito;

        public CreateUsuarioHandler(IUnitOfWork uow, IValidator<CreateUsuarioCommand> validator, ICognitoService cognito)
        {
            _uow = uow;
            _validator = validator;
            _cognito = cognito;
        }

        public async Task<Guid> Handle(CreateUsuarioCommand command, CancellationToken ct)
        {
            await _validator.ValidateAndThrowAsync(command, ct);
            
            var cognitoSub = await _cognito.RegisterUserAsync(command.Email, command.Password, ct);

            var usuario = new Usuario
            {
                UsuarioId = Guid.NewGuid(),
                Nombre = command.Nombre,
                Apellido = command.Apellido,
                Email = command.Email,
                Documento = command.Documento,
                PasswordHash = command.Password,
                Estado = UsuarioEstado.Activo,
                Credencial = null,
                CredencialId = null,
                UsuarioRoles = new List<UsuarioRol>(),
                Dispositivos = new List<Dispositivo>(),
                Beneficios = new List<BeneficioUsuario>(),
                Canjes = new List<Canje>()
            };
            
            if (command.RolesIDs is not null)
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