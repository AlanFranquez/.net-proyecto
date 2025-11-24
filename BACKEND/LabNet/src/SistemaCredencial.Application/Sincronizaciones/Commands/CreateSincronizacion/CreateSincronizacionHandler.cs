using Espectaculos.Application.Abstractions;
using Espectaculos.Domain.Entities;
using FluentValidation;
using MediatR;

namespace Espectaculos.Application.Sincronizaciones.Commands.CreateSincronizacion
{
    public class CreateSincronizacionHandler : IRequestHandler<CreateSincronizacionCommand, Guid>
    {
        private readonly IUnitOfWork _uow;
        private readonly IValidator<CreateSincronizacionCommand> _validator;

        public CreateSincronizacionHandler(IUnitOfWork uow, IValidator<CreateSincronizacionCommand> validator)
        {
            _uow = uow;
            _validator = validator;
        }

        public async Task<Guid> Handle(CreateSincronizacionCommand command, CancellationToken ct)
        {
            await _validator.ValidateAndThrowAsync(command, ct);
            
            var dispositivo = await _uow.Dispositivos.GetByIdAsync(command.DispositivoId, ct)
                          ?? throw new KeyNotFoundException("El dispositivo indicado no existe.");
            
            var sync = new Sincronizacion
            {
                SincronizacionId = Guid.NewGuid(),
                CreadoEn = command.CreadoEn,
                CantidadItems = command.CantidadItems,
                Tipo = command.Tipo,
                Estado = command.Estado,
                DetalleError = command.DetalleError,
                Checksum = command.Checksum,
                DispositivoId = command.DispositivoId,
                Dispositivo = dispositivo
            };

            await _uow.Sincronizaciones.AddAsync(sync, ct);
            await _uow.SaveChangesAsync(ct);

            return sync.SincronizacionId;
        }
    }
}