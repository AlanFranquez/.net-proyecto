using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Espectaculos.Application.Abstractions;
using Espectaculos.Application.Services;   // 👈 IMPORTANTE: este using
using FluentValidation;
using MediatR;

namespace Espectaculos.Application.EventoAcceso.Commands.CreateEvento
{
    public class CreateEventoHandler : IRequestHandler<CreateEventoCommand, Guid>
    {
        private readonly IUnitOfWork _uow;
        private readonly IValidator<CreateEventoCommand> _validator;
        private readonly IAccesosRealtimeNotifier _notifier;   // 👈 SOLO UNO

        public CreateEventoHandler(
            IUnitOfWork uow,
            IValidator<CreateEventoCommand> validator,
            IAccesosRealtimeNotifier notifier)                  // 👈 SE INYECTA ACÁ
        {
            _uow       = uow;
            _validator = validator;
            _notifier  = notifier;
        }

        public async Task<Guid> Handle(CreateEventoCommand command, CancellationToken ct)
        {
            await _validator.ValidateAndThrowAsync(command, ct);

            var espacio = await _uow.Espacios.GetByIdAsync(command.EspacioId, ct)
                          ?? throw new KeyNotFoundException("El espacio indicado no existe.");

            var credencial = await _uow.Credenciales.GetByIdAsync(command.CredencialId, ct)
                             ?? throw new KeyNotFoundException("La credencial indicada no existe.");

            var evento = new Espectaculos.Domain.Entities.EventoAcceso
            {
                EventoId        = Guid.NewGuid(),
                MomentoDeAcceso = command.MomentoDeAcceso,
                CredencialId    = command.CredencialId,
                Credencial      = credencial,
                EspacioId       = command.EspacioId,
                Espacio         = espacio,
                Resultado       = command.Resultado,
                Motivo          = command.Motivo?.Trim(),
                Modo            = command.Modo,
                Firma           = command.Firma
            };

            await _uow.EventosAccesos.AddAsync(evento, ct);
            await _uow.SaveChangesAsync(ct);

            await _notifier.NotificarNuevoAccesoAsync(evento, ct);

            return evento.EventoId;
        }
    }
}
