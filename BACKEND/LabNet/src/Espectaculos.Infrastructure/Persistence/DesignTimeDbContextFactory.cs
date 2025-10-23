using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using Espectaculos.Infrastructure.Persistence.Interceptors;

namespace Espectaculos.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<EspectaculosDbContext>
{
    public EspectaculosDbContext CreateDbContext(string[] args)
    {
        // Leer connection string desde variable de entorno o usar default local
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
                               ?? Environment.GetEnvironmentVariable("DEFAULT_CONNECTION")
                               ?? "Host=localhost;Port=5432;Database=espectaculosdb;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<EspectaculosDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        // Crear instancia del interceptor requerido por el DbContext
        var auditableInterceptor = new AuditableEntitySaveChangesInterceptor();

        return new EspectaculosDbContext(optionsBuilder.Options, auditableInterceptor);
    }
}
