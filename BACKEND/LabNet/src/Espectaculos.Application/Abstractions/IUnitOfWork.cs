using Espectaculos.Application.Abstractions.Repositories;

namespace Espectaculos.Application.Abstractions;

public interface IUnitOfWork
{
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
    IRolRepository Roles { get; }
    IDispositivoRepository Dispositivos { get; }
    ISincronizacionRepository Sincronizaciones { get; }
    INovedadRepository Novedades { get; }

Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

}
