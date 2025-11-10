using Espectaculos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espectaculos.Infrastructure.Persistence.Configurations;

public class NovedadConfiguration : IEntityTypeConfiguration<Novedad>
{
    public void Configure(EntityTypeBuilder<Novedad> b)
    {
        b.ToTable("Novedades");
        b.HasKey(x => x.NovedadId);

        b.Property(x => x.Titulo).HasMaxLength(200).IsRequired();
        b.Property(x => x.Contenido); // texto libre
        b.Property(x => x.Tipo).IsRequired();

        // índices útiles para listados
        b.HasIndex(x => x.Publicado);
        b.HasIndex(x => x.PublicadoDesdeUtc);
        b.HasIndex(x => x.PublicadoHastaUtc);
        b.HasIndex(x => x.CreadoEnUtc);
    }
}