using Espectaculos.Application.Abstractions;
using Espectaculos.Application.Services;
using Espectaculos.Domain.Enums;
using FluentValidation;
using MediatR;

namespace Espectaculos.Application.Dispositivos.Commands.DeleteDispositivo;

public class DeleteDispositivoHandler : IRequestHandler<DeleteDispositivoCommand, Guid>
{
    private readonly IUnitOfWork _uow;
    private readonly IValidator<DeleteDispositivoCommand> _validator;
    private readonly IDispositivosRealtimeNotifier _notifier;

    public DeleteDispositivoHandler(
        IUnitOfWork uow,
        IValidator<DeleteDispositivoCommand> validator,
        IDispositivosRealtimeNotifier notifier)
    {
        _uow = uow;
        _validator = validator;
        _notifier = notifier;
    }

    public async Task<Guid> Handle(DeleteDispositivoCommand command, CancellationToken ct = default)
    {
        await _validator.ValidateAndThrowAsync(command, ct);

        var dispositivo = await _uow.Dispositivos.GetByIdAsync(command.DispositivoId, ct)
                          ?? throw new KeyNotFoundException("Dispositivo no encontrado");

        dispositivo.Estado = DispositivoTipo.Revocado;

        await _uow.Dispositivos.UpdateAsync(dispositivo, ct);
        await _uow.SaveChangesAsync(ct);

        if (!string.IsNullOrWhiteSpace(dispositivo.HuellaDispositivo))
        {
            await _notifier.NotificarDispositivoRevocadoAsync(
                dispositivo.HuellaDispositivo,
                dispositivo.DispositivoId,
                ct
            );
        }

        return dispositivo.DispositivoId;
    }
}