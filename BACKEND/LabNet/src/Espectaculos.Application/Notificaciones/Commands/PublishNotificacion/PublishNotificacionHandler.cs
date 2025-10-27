using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Espectaculos.Application.Notificaciones.Commands.PublishNotificacion;
using Espectaculos.Application.Abstractions;
using Espectaculos.Application.Common.Exceptions;

namespace Espectaculos.Application.Notificaciones.Commands.PublishNotificacion;

public class PublishNotificacionHandler : IRequestHandler<PublishNotificacionCommand, bool>
{
    private readonly IUnitOfWork _uow;
    private readonly Espectaculos.Application.Abstractions.Repositories.IDispositivoRepository _dispositivoRepo;
    private readonly INotificationSender _sender;

    public PublishNotificacionHandler(IUnitOfWork uow, Espectaculos.Application.Abstractions.Repositories.IDispositivoRepository dispositivoRepo, INotificationSender sender)
    {
        _uow = uow;
        _dispositivoRepo = dispositivoRepo;
        _sender = sender;
    }

    public async Task<bool> Handle(PublishNotificacionCommand request, CancellationToken cancellationToken)
    {
        var notificacion = await _uow.Notificaciones.GetByIdAsync(request.Id, cancellationToken);
        if (notificacion is null) return false;

        if (request.ProgramadaParaUtc.HasValue)
        {
            var dt = request.ProgramadaParaUtc.Value;
            var utc = dt.Kind switch
            {
                DateTimeKind.Utc => dt,
                DateTimeKind.Local => dt.ToUniversalTime(),
                _ => DateTime.SpecifyKind(dt, DateTimeKind.Utc)
            };
            notificacion.Schedule(utc);
        }
        else
        {
            notificacion.Publish();
        }

        // Destinatarios según audiencia y/o usuario
        var targetUsuarioIds = new HashSet<Guid>();
        if (notificacion.UsuarioId.HasValue)
        {
            targetUsuarioIds.Add(notificacion.UsuarioId.Value);
        }
        else
        {
            var vigentes = await _uow.Credenciales.ListVigentesAsync(DateTime.UtcNow, cancellationToken);
            foreach (var c in vigentes)
            {
                var aud = notificacion.Audiencia;
                var matches = aud == Espectaculos.Domain.Enums.NotificacionAudiencia.Todos
                              || (aud == Espectaculos.Domain.Enums.NotificacionAudiencia.Campus && c.Tipo == Espectaculos.Domain.Enums.CredencialTipo.Campus)
                              || (aud == Espectaculos.Domain.Enums.NotificacionAudiencia.Empresa && c.Tipo == Espectaculos.Domain.Enums.CredencialTipo.Empresa);
                if (matches)
                    targetUsuarioIds.Add(c.UsuarioId);
            }
        }

        // Enviar y persistir copia por dispositivo (bandeja del dispositivo)
        foreach (var uid in targetUsuarioIds)
        {
            var dispositivos = await _dispositivoRepo.ListActivosByUsuarioAsync(uid, cancellationToken);
            if (dispositivos.Count > 0)
            {
                // Enviar (push/log)
                await _sender.SendToDevicesAsync(dispositivos, notificacion, cancellationToken);

                // Persistir una notificación por dispositivo como "SinVer"
                foreach (var d in dispositivos)
                {
                    var copia = Espectaculos.Domain.Entities.Notificacion.Create(notificacion.Tipo, notificacion.Titulo, notificacion.Cuerpo, null, notificacion.Audiencia);
                    copia.Estado = Espectaculos.Domain.Enums.NotificacionEstado.Publicada;
                    copia.DispositivoId = d.DispositivoId;
                    copia.UsuarioId = d.UsuarioId;
                    copia.LecturaEstado = Espectaculos.Domain.Enums.NotificacionLecturaEstado.SinVer;
                    await _uow.Notificaciones.AddAsync(copia, cancellationToken);
                }
            }
        }

        _uow.Notificaciones.Update(notificacion);
        await _uow.SaveChangesAsync(cancellationToken);

        return true;
    }
}
