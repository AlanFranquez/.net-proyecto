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
            
        // Relación 1:1 con Usuario: FK vive en Credencial (Credencial.UsuarioId)
        builder.HasOne(e => e.Usuario)
            .WithOne(c => c.Credencial)
            .HasForeignKey<Credencial>(e => e.UsuarioId)
            .IsRequired();

        // Eventos de acceso: la FK es EventoAcceso.CredencialId
        builder.HasMany(e => e.EventosAcceso)
            .WithOne(c => c.Credencial)
            .HasForeignKey(e => e.CredencialId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}