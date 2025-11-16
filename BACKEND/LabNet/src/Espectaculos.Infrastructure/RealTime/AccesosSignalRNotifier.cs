using System;
using System.Threading;
using System.Threading.Tasks;
using Espectaculos.Application.Services;
using EventoAccesoEntity = Espectaculos.Domain.Entities.EventoAcceso;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Espectaculos.Infrastructure.RealTime
{
    public class AccesosSignalRNotifier : IAccesosRealtimeNotifier
    {
        private readonly IHubContext<AccesosHub> _hub;
        private readonly ILogger<AccesosSignalRNotifier> _logger;

        public AccesosSignalRNotifier(
            IHubContext<AccesosHub> hub,
            ILogger<AccesosSignalRNotifier> logger)
        {
            _hub = hub;
            _logger = logger;
        }

        public async Task NotificarNuevoAccesoAsync(EventoAccesoEntity evento, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation(
                    "SignalR: enviando NuevoAcceso para EventoId {EventoId}",
                    evento.EventoId);

                await _hub.Clients.All.SendAsync("NuevoAcceso", new
                {
                    momento = evento.MomentoDeAcceso.ToLocalTime().ToString("G"),
                    espacio = evento.Espacio?.Nombre,
                    usuario = evento.Credencial?.Usuario != null
                        ? $"{evento.Credencial.Usuario.Nombre} {evento.Credencial.Usuario.Apellido}"
                        : null,
                    resultado = evento.Resultado.ToString(),
                    modo = evento.Modo.ToString(),
                    motivo = evento.Motivo
                }, CancellationToken.None);

                _logger.LogInformation(
                    "SignalR: NuevoAcceso enviado OK para EventoId {EventoId}",
                    evento.EventoId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error enviando NuevoAcceso por SignalR para EventoId {EventoId}",
                    evento.EventoId);
                throw;
            }
        }
    }
}
