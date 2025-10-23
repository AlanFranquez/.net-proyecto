using Espectaculos.Application.Abstractions;
using FluentValidation;
using MediatR;

namespace Espectaculos.Application.EventoAcceso.Commands.CreateEvento
{
    public class CreateEventoHandler : IRequestHandler<CreateEventoCommand, Guid>
    {
        private readonly IUnitOfWork _uow;
        private readonly IValidator<CreateEventoCommand> _validator;

        public CreateEventoHandler(IUnitOfWork uow, IValidator<CreateEventoCommand> validator)
        {
            _uow = uow;
            _validator = validator;
        }

        public async Task<Guid> Handle(CreateEventoCommand command, CancellationToken ct)
        {
            await _validator.ValidateAndThrowAsync(command, ct);
            
            var espacio = await _uow.Espacios.GetByIdAsync(command.EspacioId, ct)
                          ?? throw new KeyNotFoundException("El espacio indicado no existe.");
            
            // ------- DESCOMENTAR UNA VEZ EXISTA CREDENCIAL
            //var credencial = await _uow.Credenciales.GetByIdAsync(command.CredencialId, ct)
            //                 ?? throw new KeyNotFoundException("La credencial indicada no existe.");

            var credencial = espacio;
            
            var evento = new Domain.Entities.EventoAcceso
            {
                EventoId = Guid.NewGuid(),
                MomentoDeAcceso = command.MomentoDeAcceso,
                CredencialId = command.CredencialId,
                // ------- DESCOMENTAR UNA VEZ EXISTA CREDENCIAL
                //Credencial = credencial,
                EspacioId = command.EspacioId,
                Espacio = espacio,
                Resultado = command.Resultado,
                Motivo = command.Motivo?.Trim(),
                Modo = command.Modo,
                Firma = command.Firma
            };

            await _uow.EventosAccesos.AddAsync(evento, ct);
            await _uow.SaveChangesAsync(ct);

            return evento.EventoId;
        }
    }
}