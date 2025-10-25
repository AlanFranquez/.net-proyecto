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
            
            var usuario = await _uow.Usuarios.GetByIdAsync(command.UsuarioId, ct)
                          ?? throw new KeyNotFoundException("El usuario indicado no existe.");

            var e = new Credencial
            {
                CredencialId = Guid.NewGuid(),
                Tipo = command.Tipo,
                Estado = command.Estado,
                IdCriptografico = command.IdCriptografico.Trim(),
                FechaEmision = command.FechaEmision,
                FechaExpiracion = command.FechaExpiracion,
                UsuarioId = command.UsuarioId,
                Usuario = usuario,
                EventosAcceso = new List<Domain.Entities.EventoAcceso>()
            };

            await _uow.Credenciales.AddAsync(e, ct);
            await _uow.SaveChangesAsync(ct);

            return e.CredencialId;
        }
    }
}