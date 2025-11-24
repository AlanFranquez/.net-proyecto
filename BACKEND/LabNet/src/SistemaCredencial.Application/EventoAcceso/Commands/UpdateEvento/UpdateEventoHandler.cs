using Espectaculos.Application.Abstractions;
using Espectaculos.Domain.Entities;
using FluentValidation;
using MediatR;

namespace Espectaculos.Application.EventoAcceso.Commands.UpdateEvento;

public class UpdateEventoHandler : IRequestHandler<UpdateEventoCommand, Guid>
{
    private readonly IUnitOfWork _uow;
    private readonly IValidator<UpdateEventoCommand> _validator;

    public UpdateEventoHandler(IUnitOfWork uow, IValidator<UpdateEventoCommand> validator)
    {
        _uow = uow;
        _validator = validator;
    }

    public async Task<Guid> Handle(UpdateEventoCommand command, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAsync(command, ct);

        var evento = await _uow.EventosAccesos.GetByIdAsync(command.EventoId, ct)
                      ?? throw new KeyNotFoundException("Evento no encontrado.");

        if (command.MomentoDeAcceso.HasValue)
            evento.MomentoDeAcceso = command.MomentoDeAcceso.Value;

        if (command.CredencialId.HasValue)
        {
            var credencial = await _uow.Credenciales.GetByIdAsync(command.CredencialId.Value, ct)
                             ?? throw new KeyNotFoundException("Credencial no encontrada.");
            evento.CredencialId = command.CredencialId.Value;
            evento.Credencial = credencial;
        }

        if (command.EspacioId.HasValue)
        {
            var espacio = await _uow.Espacios.GetByIdAsync(command.EspacioId.Value, ct)
                          ?? throw new KeyNotFoundException("Espacio no encontrado.");
            evento.EspacioId = command.EspacioId.Value;
            evento.Espacio = espacio;
        }

        if (command.Resultado.HasValue)
            evento.Resultado = command.Resultado.Value;

        if (command.Modo.HasValue)
            evento.Modo = command.Modo.Value;

        if (command.Motivo is not null)
            evento.Motivo = command.Motivo.Trim();

        if (command.Firma is not null)
            evento.Firma = command.Firma.Trim();

        await _uow.EventosAccesos.UpdateAsync(evento, ct);
        await _uow.SaveChangesAsync(ct);

        return evento.EventoId;
    }
}
