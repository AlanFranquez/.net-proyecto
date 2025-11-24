using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Espectaculos.Domain.Entities;

namespace Espectaculos.Application.Abstractions
{
    public interface INotificationSender
    {
        Task SendToDevicesAsync(IEnumerable<Dispositivo> dispositivos, Notificacion notificacion, CancellationToken ct = default);
    }
}
