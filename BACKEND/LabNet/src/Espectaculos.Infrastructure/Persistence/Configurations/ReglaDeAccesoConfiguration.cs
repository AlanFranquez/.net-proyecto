using Espectaculos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espectaculos.Infrastructure.Persistence.Configurations;

public class ReglaDeAccesoConfiguration : IEntityTypeConfiguration<ReglaDeAcceso>
{
    public void Configure(EntityTypeBuilder<ReglaDeAcceso> builder)
    {
        builder.ToTable("regla_de_acceso");
        builder.HasKey(e => e.ReglaId);
        builder.Property(e => e.VentanaHoraria).IsRequired();
        builder.Property(e => e.VigenciaInicio).IsRequired();
        builder.Property(e => e.VigenciaFin).IsRequired();
        builder.Property(e => e.Prioridad).IsRequired();
        builder.Property(e => e.Politica).IsRequired();
        builder.Property(e => e.Rol).IsRequired(false);
        builder.Property(e => e.RequiereBiometriaConfirmacion).IsRequired();

        builder.HasMany(e => e.Espacios)
            .WithOne(r => r.Regla)
            .HasForeignKey(r => r.ReglaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}