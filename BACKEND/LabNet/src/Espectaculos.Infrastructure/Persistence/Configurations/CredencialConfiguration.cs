using Espectaculos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espectaculos.Infrastructure.Persistence.Configurations;

public class CredencialConfiguration : IEntityTypeConfiguration<Credencial>
{
    public void Configure(EntityTypeBuilder<Credencial> builder)
    {
        builder.ToTable("credencial");
        builder.HasKey(e => e.CredencialId);
        builder.Property(e => e.Tipo).HasConversion<string>().IsRequired();
        builder.Property(e => e.Estado).HasConversion<string>().IsRequired();
        builder.Property(e => e.IdCriptografico).IsRequired();
        builder.Property(e => e.FechaEmision).HasConversion<string>().IsRequired();
        builder.Property(e => e.FechaExpiracion).HasMaxLength(1000).IsRequired(false);
            
        builder.HasOne(e => e.Usuario)
            .WithOne(c => c.Credencial)
            .HasForeignKey<Credencial>(e => e.UsuarioId);

        builder.HasMany(e => e.EventosAcceso)
            .WithOne(c => c.Credencial)
            .HasForeignKey(e => e.EventoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}