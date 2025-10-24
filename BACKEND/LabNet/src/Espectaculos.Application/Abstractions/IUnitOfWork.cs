using Espectaculos.Application.Abstractions.Repositories;

namespace Espectaculos.Application.Abstractions;

public interface IUnitOfWork
{
    IEventoRepository Eventos { get; }
    IEntradaRepository Entradas { get; }
    IOrdenRepository Ordenes { get; }
    IUsuarioRepository Usuarios { get; }
    IEspacioRepository Espacios { get; }
    IReglaDeAccesoRepository Reglas { get; }
    IBeneficioRepository Beneficios { get; }
    IBeneficioUsuarioRepository BeneficioUsuarios { get; }
    IBeneficioEspacioRepository BeneficioEspacios { get; }
    ICanjeRepository Canjes { get; }
    IEventoAccesoRepository EventosAccesos { get; }
    ICredencialRepository Credenciales { get; }
    INotificacionRepository Notificaciones { get; }

Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
