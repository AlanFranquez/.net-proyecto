using Espectaculos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espectaculos.Infrastructure.Persistence.Configurations;

public class BeneficioConfiguration : IEntityTypeConfiguration<Beneficio>
{
    public void Configure(EntityTypeBuilder<Beneficio> builder)
    {
        builder.ToTable("beneficio");
        builder.HasKey(e => e.BeneficioId);
        builder.Property(e => e.Tipo).HasConversion<string>().IsRequired();
        builder.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
    // Descripcion es nullable en el modelo de dominio; mapear como opcional
    builder.Property(e => e.Descripcion).IsRequired(false);
    // Estas propiedades son nullable en el modelo de dominio; mapear como opcionales
    builder.Property(e => e.VigenciaInicio).IsRequired(false);
    builder.Property(e => e.VigenciaFin).IsRequired(false);
    builder.Property(e => e.CupoTotal).IsRequired(false);
    builder.Property(e => e.CupoPorUsuario).IsRequired(false);
    builder.Property(e => e.RequiereBiometria).IsRequired();
    // CriterioElegibilidad is optional in the domain model (string?) so map it as optional
    builder.Property(e => e.CriterioElegibilidad).IsRequired(false);

    // RowVersion para control de concurrencia al decrementar cupos
    builder.Property(e => e.RowVersion).IsRowVersion();
        
        // Relaciones con tablas puente — las FKs apuntan al Id del Beneficio en la tabla puente
        builder.HasMany(e => e.Espacios)
            .WithOne(r => r.Beneficio)
            .HasForeignKey(r => r.BeneficioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Usuarios)
            .WithOne(b => b.Beneficio)
            .HasForeignKey(b => b.BeneficioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}