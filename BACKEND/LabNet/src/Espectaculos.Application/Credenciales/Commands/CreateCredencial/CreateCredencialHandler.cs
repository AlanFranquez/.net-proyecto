using Espectaculos.Application.Abstractions;
using Espectaculos.Domain.Entities;
using FluentValidation;
using MediatR;

namespace Espectaculos.Application.Credenciales.Commands.CreateCredencial
{
    public class CreateCredencialHandler : IRequestHandler<CreateCredencialCommand, Guid>
    {
        private readonly IUnitOfWork _uow;
        private readonly IValidator<CreateCredencialCommand> _validator;

        public CreateCredencialHandler(IUnitOfWork uow, IValidator<CreateCredencialCommand> validator)
        {
            _uow = uow;
            _validator = validator;
        }

        public async Task<Guid> Handle(CreateCredencialCommand command, CancellationToken ct)
        {
            await _validator.ValidateAndThrowAsync(command, ct);

            // 1) Verificamos que el usuario exista
            var usuario = await _uow.Usuarios.GetByIdAsync(command.UsuarioId, ct);
            if (usuario is null)
                throw new KeyNotFoundException("El usuario indicado no existe.");

            // 2) Regla: un usuario solo puede tener una credencial
            if (usuario.CredencialId.HasValue)
                throw new InvalidOperationException(
                    "El usuario ya tiene una credencial asignada. Edítela o elimínela antes de crear una nueva."
                );

            // 3) Creamos la credencial
            var credencialId = Guid.NewGuid();

            var credencial = new Credencial
            {
                CredencialId    = credencialId,
                Tipo            = command.Tipo,
                Estado          = command.Estado,
                IdCriptografico = command.IdCriptografico?.Trim(),
                FechaEmision    = command.FechaEmision,
                FechaExpiracion = command.FechaExpiracion,
                UsuarioId       = command.UsuarioId,
                EventosAcceso   = new List<Domain.Entities.EventoAcceso>()
            };

            // 4) Actualizamos el lado del usuario (en memoria)
            usuario.CredencialId = credencialId;
            // opcional:
            // usuario.Credencial = credencial;

            // 5) Registramos AMBOS cambios de forma explícita ✅
            await _uow.Credenciales.AddAsync(credencial, ct);
            await _uow.Usuarios.UpdateAsync(usuario, ct);   // <-- clave

            await _uow.SaveChangesAsync(ct);

            return credencialId;
        }
    }
}
