using System;
using System.Threading;
using System.Threading.Tasks;
using Espectaculos.Application.Abstractions;
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
        private readonly IUnitOfWork _uow;

        public AccesosSignalRNotifier(
            IHubContext<AccesosHub> hub,
            ILogger<AccesosSignalRNotifier> logger,
            IUnitOfWork uow)
        {
            _hub = hub;
            _logger = logger;
            _uow = uow;
        }

        public async Task NotificarNuevoAccesoAsync(EventoAccesoEntity evento, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation(
                    "SignalR: enviando NuevoAcceso para EventoId {EventoId}",
                    evento.EventoId);

                // Buscar usuario por CredencialId
                string? usuarioNombre = null;
                string? usuarioEmail  = null;

                if (evento.CredencialId != Guid.Empty)
                {
                    var usuarios = await _uow.Usuarios.ListAsync(ct);
                    var usuario = usuarios.FirstOrDefault(u => u.CredencialId == evento.CredencialId);

                    if (usuario is not null)
                    {
                        usuarioNombre = $"{usuario.Nombre} {usuario.Apellido}".Trim();
                        usuarioEmail  = usuario.Email;
                    }
                }

                await _hub.Clients.All.SendAsync("NuevoAcceso", new
                {
                    momento = evento.MomentoDeAcceso.ToLocalTime().ToString("G"),
                    espacio = evento.Espacio?.Nombre,
                    usuario = usuarioNombre,
                    email   = usuarioEmail,
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
