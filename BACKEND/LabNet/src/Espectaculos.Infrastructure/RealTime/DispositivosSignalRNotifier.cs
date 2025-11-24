using System;
using System.Threading;
using System.Threading.Tasks;
using Espectaculos.Application.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Espectaculos.Infrastructure.RealTime
{
    public class DispositivosSignalRNotifier : IDispositivosRealtimeNotifier
    {
        private readonly IHubContext<DispositivosHub> _hub;
        private readonly ILogger<DispositivosSignalRNotifier> _logger;

        public DispositivosSignalRNotifier(
            IHubContext<DispositivosHub> hub,
            ILogger<DispositivosSignalRNotifier> logger)
        {
            _hub = hub;
            _logger = logger;
        }

        public async Task NotificarDispositivoRevocadoAsync(string huellaDispositivo, Guid dispositivoId, CancellationToken ct = default)
        {
            try
            {
                await _hub.Clients
                    .Group(DispositivosHub.DeviceGroup(huellaDispositivo))
                    .SendAsync("DispositivoRevocado", new
                    {
                        dispositivoId,
                        huellaDispositivo
                    }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando DispositivoRevocado a huella {Huella}", huellaDispositivo);
            }
        }
    }
}