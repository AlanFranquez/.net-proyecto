using Espectaculos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espectaculos.Infrastructure.Persistence.Configurations
{
    public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("usuario");
            builder.HasKey(u => u.UsuarioId);
            builder.Property(u => u.Documento).IsRequired().HasMaxLength(50);
            builder.Property(u => u.Nombre).IsRequired().HasMaxLength(100);
            builder.Property(u => u.Apellido).IsRequired().HasMaxLength(100);
            builder.Property(u => u.Email).IsRequired().HasMaxLength(150);
            builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(150);
            builder.Property(u => u.Estado).HasConversion<string>();
            
            builder.HasMany(e => e.Canjes)
                .WithOne(c => c.Usuario)
                .HasForeignKey(e => e.CanjeId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasMany(e => e.Dispositivos)
                .WithOne(c => c.Usuario)
                .HasForeignKey(e => e.DispositivoId)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.HasMany(e => e.Beneficios)
                .WithOne(b => b.Usuario)
                .HasForeignKey(b => b.BeneficioId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasMany(e => e.UsuarioRoles)
                .WithOne(b => b.Usuario)
                .HasForeignKey(b => b.RolId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}