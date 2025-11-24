using Espectaculos.Application.Abstractions;
using Espectaculos.Domain.Entities;
using FluentValidation;
using MediatR;

namespace Espectaculos.Application.Roles.Commands.CreateRol
{
    public class CreateRolHandler : IRequestHandler<CreateRolCommand, Guid>
    {
        private readonly IUnitOfWork _uow;
        private readonly IValidator<CreateRolCommand> _validator;

        public CreateRolHandler(IUnitOfWork uow, IValidator<CreateRolCommand> validator)
        {
            _uow = uow;
            _validator = validator;
        }

        public async Task<Guid> Handle(CreateRolCommand command, CancellationToken ct)
        {
            await _validator.ValidateAndThrowAsync(command, ct);

            var e = new Rol
            {
                RolId = Guid.NewGuid(),
                Tipo = command.Tipo,
                Prioridad = command.Prioridad,
                FechaAsignado = command.FechaAsignado,
                UsuarioRoles = new List<UsuarioRol>()
            };

            await _uow.Roles.AddAsync(e, ct);
            await _uow.SaveChangesAsync(ct);

            return e.RolId;
        }
    }
}