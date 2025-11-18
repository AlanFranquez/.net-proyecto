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
        private readonly ICognitoService _cognitoService; // Better naming

        public CreateUsuarioHandler(
            IUnitOfWork uow, 
            IValidator<CreateUsuarioCommand> validator, 
            ICognitoService cognitoService) // Better naming
        {
            _uow = uow;
            _validator = validator;
            _cognitoService = cognitoService;
        }

        public async Task<Guid> Handle(CreateUsuarioCommand command, CancellationToken ct)
        {
            await _validator.ValidateAndThrowAsync(command, ct);
            
            // Register user in Cognito
            var cognitoSub = await _cognitoService.RegisterUserAsync(command.Email, command.Password, ct);

            var usuario = new Usuario
            {
                UsuarioId = Guid.NewGuid(),
                Nombre = command.Nombre,
                Apellido = command.Apellido,
                Email = command.Email,
                Documento = command.Documento,
                PasswordHash = command.Password, // Consider hashing this
                Estado = UsuarioEstado.Activo,
                Credencial = null,
                CredencialId = null,
                UsuarioRoles = new List<UsuarioRol>(),
                Dispositivos = new List<Dispositivo>(),
                Beneficios = new List<BeneficioUsuario>(),
                Canjes = new List<Canje>()
            };
            
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