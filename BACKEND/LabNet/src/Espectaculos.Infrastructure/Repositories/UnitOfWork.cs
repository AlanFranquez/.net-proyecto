using Espectaculos.Application.Abstractions;
using Espectaculos.Application.Abstractions.Repositories;
using Espectaculos.Infrastructure.Persistence;

namespace Espectaculos.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly EspectaculosDbContext _db;

    public UnitOfWork(EspectaculosDbContext db,
                      IUsuarioRepository usuarios,
                      IEspacioRepository espacios,
                      IReglaDeAccesoRepository reglas,
                      IBeneficioRepository beneficios,
                      IBeneficioUsuarioRepository beneficioUsuarios,
                      IBeneficioEspacioRepository beneficioEspacios,
                      ICanjeRepository canjes,
                      IEventoAccesoRepository eventosAccesos,
                      ICredencialRepository credenciales,
                      INotificacionRepository notificaciones,
                      IRolRepository roles,
                      IDispositivoRepository dispositivos,
                      ISincronizacionRepository sincronizaciones
    )
    {
        _db = db;
        Usuarios = usuarios;
        Espacios = espacios;
        Reglas = reglas;
        Beneficios = beneficios;
        BeneficioUsuarios = beneficioUsuarios;
        BeneficioEspacios = beneficioEspacios;
        Canjes = canjes;
        EventosAccesos = eventosAccesos;
        Credenciales = credenciales;
        Notificaciones = notificaciones;
        Roles = roles;
        Dispositivos = dispositivos;
        Sincronizaciones = sincronizaciones;
    }
    public IUsuarioRepository Usuarios { get; }
    public IEspacioRepository Espacios { get; }
    public IReglaDeAccesoRepository Reglas { get; }
    public IBeneficioRepository Beneficios { get; }
    public IBeneficioUsuarioRepository BeneficioUsuarios { get; }
    public IBeneficioEspacioRepository BeneficioEspacios { get; }
    public ICanjeRepository Canjes { get; }
    public IEventoAccesoRepository EventosAccesos { get; }
    public ICredencialRepository Credenciales { get; }
    public INotificacionRepository Notificaciones { get; }
    public IRolRepository Roles { get; }
    public IDispositivoRepository Dispositivos { get; }
    public ISincronizacionRepository Sincronizaciones { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return _db.SaveChangesAsync(cancellationToken);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException ex)
        {
            // map to application-level concurrency exception
            throw new Espectaculos.Application.Common.Exceptions.ConcurrencyException("Concurrency conflict during SaveChanges", ex);
        }
    }
}
