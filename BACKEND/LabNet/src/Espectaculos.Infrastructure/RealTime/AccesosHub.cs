using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Espectaculos.Infrastructure.RealTime
{
    public class AccesosHub : Hub
    {
        private readonly ILogger<AccesosHub> _logger;

        public AccesosHub(ILogger<AccesosHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Cliente conectado al hub: {ConnectionId}", Context.ConnectionId);

            await Clients.Caller.SendAsync("NuevoAcceso", new
            {
                momento = DateTime.Now.ToString("G"),
                espacio = "DEBUG",
                usuario = "HubTest",
                resultado = "Permitido",
                modo = "Manual",
                motivo = "Evento de prueba desde OnConnectedAsync"
            });

            await base.OnConnectedAsync();
        }
    }
}