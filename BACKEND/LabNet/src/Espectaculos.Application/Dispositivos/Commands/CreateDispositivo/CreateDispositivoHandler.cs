using Espectaculos.Application.Abstractions;
using Espectaculos.Domain.Entities;
using FluentValidation;
using MediatR;

namespace Espectaculos.Application.Dispositivos.Commands.CreateDispositivo
{
    public class CreateDispositivoHandler : IRequestHandler<CreateDispositivoCommand, Guid>
    {
        private readonly IUnitOfWork _uow;
        private readonly IValidator<CreateDispositivoCommand> _validator;

        public CreateDispositivoHandler(IUnitOfWork uow, IValidator<CreateDispositivoCommand> validator)
        {
            _uow = uow;
            _validator = validator;
        }

        public async Task<Guid> Handle(CreateDispositivoCommand command, CancellationToken ct)
        {
            await _validator.ValidateAndThrowAsync(command, ct);

            var usuario = await _uow.Usuarios.GetByIdAsync(command.UsuarioId, ct)
                          ?? throw new KeyNotFoundException("El usuario indicado no existe.");

            
            var e = new Dispositivo
            {
                DispositivoId = Guid.NewGuid(),
                NumeroTelefono = command.NumeroTelefono,
                Plataforma = command.Plataforma,
                HuellaDispositivo = command.HuellaDispositivo,
                BiometriaHabilitada = command.BiometriaHabilitada,
                Estado = command.Estado,
                UsuarioId = command.UsuarioId,
                Usuario = usuario,
                Notificaciones = new List<Notificacion>(),
                Sincronizaciones = new List<Sincronizacion>()
            };

            await _uow.Dispositivos.AddAsync(e, ct);
            await _uow.SaveChangesAsync(ct);

            return e.DispositivoId;
        }
    }
}