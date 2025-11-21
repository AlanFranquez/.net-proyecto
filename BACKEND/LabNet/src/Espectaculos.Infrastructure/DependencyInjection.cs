using Espectaculos.Application.Abstractions;
using Espectaculos.Application.Abstractions.Repositories;
using Espectaculos.Application.Abstractions.Security;
using Espectaculos.Infrastructure.Persistence;
using Espectaculos.Infrastructure.Persistence.Interceptors;
using Espectaculos.Infrastructure.Repositories;
using Espectaculos.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Espectaculos.Application.Abstractions.Security;
using Microsoft.Extensions.DependencyInjection;
namespace Espectaculos.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddScoped<AuditableEntitySaveChangesInterceptor>();
        services.AddDbContext<EspectaculosDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddScoped<IDispositivoRepository, DispositivoRepository>();
        services.AddScoped<IBeneficioRepository, BeneficioRepository>();
        services.AddScoped<ICanjeRepository, CanjeRepository>();
        services.AddScoped<ICredencialRepository, CredencialRepository>();
        services.AddScoped<IEspacioRepository, EspacioRepository>();
        services.AddScoped<INotificacionRepository, NotificacionRepository>();
        services.AddScoped<IReglaDeAccesoRepository, ReglaDeAccesoRepository>();
        services.AddScoped<IRolRepository, RolRepository>();
        services.AddScoped<ISincronizacionRepository, SincronizacionRepository>();
        services.AddScoped<IBeneficioEspacioRepository, BeneficioEspacioRepository>();
        services.AddScoped<IBeneficioUsuarioRepository, BeneficioUsuarioRepository>();
        services.AddScoped<IEspacioReglaDeAccesoRepository, EspacioReglaDeAccesoRepository>();
        services.AddScoped<IEventoAccesoRepository, EventoAccesoRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IUsuarioRolRepository, UsuarioRolRepository>();
        services.AddScoped<INovedadRepository, NovedadRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();

        return services;
    }
}