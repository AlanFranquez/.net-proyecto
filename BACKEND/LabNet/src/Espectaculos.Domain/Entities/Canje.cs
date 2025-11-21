using Espectaculos.Domain.Enums;

namespace Espectaculos.Domain.Entities;

public class Canje
{
    public Guid CanjeId { get; set; }
    public Guid BeneficioId { get; set; }
    public Beneficio Beneficio { get; set; } = null!;
    public Guid UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public DateTime Fecha { get; set; }
    public EstadoCanje Estado { get; set; }
    public bool? VerificacionBiometrica { get; set; }
    public string? Firma { get; set; }
    // Factory y operaciones simples de dominio
    public static Canje CreatePending(Guid beneficioId, Guid usuarioId, DateTime fechaUtc, bool? verificacionBiometrica = null, string? firma = null)
    {
        if (beneficioId == Guid.Empty) throw new ArgumentException("beneficioId inválido", nameof(beneficioId));
        if (usuarioId == Guid.Empty) throw new ArgumentException("usuarioId inválido", nameof(usuarioId));
        return new Canje
        {
            CanjeId = Guid.NewGuid(),
            BeneficioId = beneficioId,
            UsuarioId = usuarioId,
            Fecha = fechaUtc,
            Estado = EstadoCanje.Pendiente,
            VerificacionBiometrica = verificacionBiometrica,
            Firma = firma
        };
    }

    public void Confirm()
    {
        Estado = EstadoCanje.Confirmado;
    }

    public void Cancel()
    {
        Estado = EstadoCanje.Anulado;
    }
}