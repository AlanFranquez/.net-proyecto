using Espectaculos.Domain.Common;
using Espectaculos.Domain.Entities;
using Espectaculos.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace Espectaculos.Infrastructure.Persistence;

public class EspectaculosDbContext : DbContext
{
    private readonly AuditableEntitySaveChangesInterceptor _auditableInterceptor;

    public EspectaculosDbContext(DbContextOptions<EspectaculosDbContext> options,
                                 AuditableEntitySaveChangesInterceptor auditableInterceptor)
        : base(options)
    {
        _auditableInterceptor = auditableInterceptor;
    }
    
    public DbSet<Usuario> Usuario => Set<Usuario>();
    
    public DbSet<Novedad> Novedades { get; set; } = default!;

    // Novedad removed - use Notificaciones instead
    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EspectaculosDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_auditableInterceptor);
        // Interceptor para recomputar disponibilidad luego de escrituras (reserva de stock, seed, etc.)
        optionsBuilder.AddInterceptors(new AvailabilityRecomputeInterceptor());
        base.OnConfiguring(optionsBuilder);
    }
}
