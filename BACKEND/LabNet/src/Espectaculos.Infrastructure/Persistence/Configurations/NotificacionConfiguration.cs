using Espectaculos.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Espectaculos.Infrastructure.Persistence.Configurations;

public class NotificacionConfiguration : IEntityTypeConfiguration<Notificacion>
{
    public void Configure(EntityTypeBuilder<Notificacion> builder)
    {
        builder.ToTable("notificacion");
        builder.HasKey(n => n.NotificacionId).HasName("pk_notificacion");
        builder.Property(n => n.NotificacionId).HasColumnName("id").IsRequired();
        builder.Property(n => n.Tipo).HasColumnName("tipo").IsRequired();
        builder.Property(n => n.Titulo).HasColumnName("titulo").IsRequired();
        builder.Property(n => n.Cuerpo).HasColumnName("cuerpo");
        builder.Property(n => n.ProgramadaParaUtc).HasColumnName("programada_para_utc");
    builder.Property(n => n.Estado).HasColumnName("estado").IsRequired();
    builder.Property(n => n.LecturaEstado).HasColumnName("lectura_estado").IsRequired();
    builder.Property(n => n.Audiencia).HasColumnName("audiencia").IsRequired();
        // Canales and Metadatos stored as JSON
        builder.Property(n => n.Canales)
               .HasColumnName("canales")
               .HasColumnType("jsonb");

        builder.Property(n => n.Metadatos)
               .HasColumnName("metadatos")
               .HasColumnType("jsonb");

        builder.Property(n => n.CreadoEnUtc).HasColumnName("creado_en_utc").IsRequired();
    builder.HasIndex(n => n.Estado).HasDatabaseName("ix_notificacion_estado");
    builder.HasIndex(n => new { n.DispositivoId, n.LecturaEstado }).HasDatabaseName("ix_notificacion_dispositivo_lectura");
        // Relación con Dispositivo (opcional)
        builder.Property(n => n.DispositivoId).HasColumnName("dispositivo_id").IsRequired(false);
        builder.HasOne(n => n.Dispositivo)
            .WithMany(d => d.Notificaciones)
            .HasForeignKey(n => n.DispositivoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relación con Usuario (opcional)
        builder.Property(n => n.UsuarioId).HasColumnName("usuario_id").IsRequired(false);
        builder.HasOne(n => n.Usuario)
            .WithMany(u => u.Notificaciones)
            .HasForeignKey(n => n.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
