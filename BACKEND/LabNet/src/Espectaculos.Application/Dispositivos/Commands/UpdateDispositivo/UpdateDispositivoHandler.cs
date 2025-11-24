using Espectaculos.Application.Abstractions;
using Espectaculos.Domain.Entities;
using FluentValidation;
using MediatR;

namespace Espectaculos.Application.Dispositivos.Commands.UpdateDispositivo;

public class UpdateDispositivoHandler : IRequestHandler<UpdateDispositivoCommand, Guid>
{
    private readonly IUnitOfWork _uow;
    private readonly IValidator<UpdateDispositivoCommand> _validator;

    public UpdateDispositivoHandler(IUnitOfWork uow, IValidator<UpdateDispositivoCommand> validator)
    {
        _uow = uow;
        _validator = validator;
    }

    public async Task<Guid> Handle(UpdateDispositivoCommand command, CancellationToken ct = default)
    {
        await _validator.ValidateAndThrowAsync(command, ct);

        var dispositivo = await _uow.Dispositivos.GetByIdAsync(command.DispositivoId, ct)
                          ?? throw new KeyNotFoundException("Dispositivo no encontrado");

        if (command.NumeroTelefono is not null)
            dispositivo.NumeroTelefono = command.NumeroTelefono.Trim();

        if (command.Plataforma.HasValue)
            dispositivo.Plataforma = command.Plataforma.Value;

        if (command.HuellaDispositivo is not null)
            dispositivo.HuellaDispositivo = command.HuellaDispositivo.Trim();

        if (command.NavegadorNombre is not null)
            dispositivo.NavegadorNombre = command.NavegadorNombre.Trim();

        if (command.NavegadorVersion is not null)
            dispositivo.NavegadorVersion = command.NavegadorVersion.Trim();

        if (command.BiometriaHabilitada.HasValue)
            dispositivo.BiometriaHabilitada = command.BiometriaHabilitada.Value;

        if (command.Estado.HasValue)
            dispositivo.Estado = command.Estado.Value;

        if (command.UsuarioId.HasValue)
        {
            var usuario = await _uow.Usuarios.GetByIdAsync(command.UsuarioId.Value, ct)
                          ?? throw new KeyNotFoundException("Usuario no encontrado.");

            dispositivo.UsuarioId = command.UsuarioId.Value;
            dispositivo.Usuario = usuario;
        }

        if (command.NotificacionesIds is not null)
        {
            dispositivo.Notificaciones = command.NotificacionesIds
                .Select(eid => new Notificacion { NotificacionId = eid })
                .ToList();
        }

        if (command.SincronizacionesIds is not null)
        {
            dispositivo.Sincronizaciones = command.SincronizacionesIds
                .Select(eid => new Sincronizacion { SincronizacionId = eid })
                .ToList();
        }

        await _uow.Dispositivos.UpdateAsync(dispositivo, ct);
        await _uow.SaveChangesAsync(ct);

        return dispositivo.DispositivoId;
    }
}
