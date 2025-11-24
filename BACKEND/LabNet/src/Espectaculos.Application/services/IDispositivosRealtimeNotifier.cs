using System.Threading;
using System.Threading.Tasks;

namespace Espectaculos.Application.Services
{
    public interface IDispositivosRealtimeNotifier
    {
        Task NotificarDispositivoRevocadoAsync(string huellaDispositivo, Guid dispositivoId, CancellationToken ct = default);
    }
}