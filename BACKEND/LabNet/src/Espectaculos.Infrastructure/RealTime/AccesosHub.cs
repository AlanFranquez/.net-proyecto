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
    }
}