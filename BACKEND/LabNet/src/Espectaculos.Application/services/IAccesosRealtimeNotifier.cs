using System.Threading;
using System.Threading.Tasks;
using EventoAccesoEntity = Espectaculos.Domain.Entities.EventoAcceso;

namespace Espectaculos.Application.Services
{
    public interface IAccesosRealtimeNotifier
    {
        Task NotificarNuevoAccesoAsync(EventoAccesoEntity evento, CancellationToken ct = default);
    }
}