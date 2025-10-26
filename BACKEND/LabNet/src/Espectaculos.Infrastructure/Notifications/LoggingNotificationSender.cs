using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Espectaculos.Application.Abstractions;
using Espectaculos.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Espectaculos.Infrastructure.Notifications
{
    public class LoggingNotificationSender : INotificationSender
    {
        private readonly ILogger<LoggingNotificationSender> _logger;
        public LoggingNotificationSender(ILogger<LoggingNotificationSender> logger)
        {
            _logger = logger;
        }

        public Task SendToDevicesAsync(IEnumerable<Dispositivo> dispositivos, Notificacion notificacion, CancellationToken ct = default)
        {
            foreach (var d in dispositivos)
            {
                _logger.LogInformation("[Notification] Enviar NotificacionId={NotificacionId} Titulo={Titulo} a DispositivoId={DispositivoId} UsuarioId={UsuarioId}",
                    notificacion.NotificacionId, notificacion.Titulo, d.DispositivoId, d.UsuarioId);
            }
            return Task.CompletedTask;
        }
    }
}
