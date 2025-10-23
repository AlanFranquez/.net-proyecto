using Espectaculos.Domain.Enums;

namespace Espectaculos.Domain.Entities;

public class Beneficio
{
    public Guid BeneficioId { get; set; }
    public BeneficioTipo Tipo { get; set; }
    public string Nombre { get; set; } = default!;
    public string? Descripcion { get; set; }
    public DateTime? VigenciaInicio { get; set; }
    public DateTime? VigenciaFin { get; set; }
    public int? CupoTotal { get; set; }
    public int? CupoPorUsuario { get; set; }
    public bool RequiereBiometria { get; set; }
    public string? CriterioElegibilidad { get; set; }

    public ICollection<BeneficioEspacio> Espacios { get; set; } = new List<BeneficioEspacio>();
    public ICollection<BeneficioUsuario> Usuarios { get; set; } = new List<BeneficioUsuario>();

    // Row version para control de concurrencia al decrementar cupos
    public byte[]? RowVersion { get; set; }

    // --- Métodos de dominio (invariantes y operaciones) ---
    public static Beneficio Create(string nombre,
                                   BeneficioTipo tipo,
                                   DateTime? vigenciaInicio = null,
                                   DateTime? vigenciaFin = null,
                                   int? cupoTotal = null,
                                   int? cupoPorUsuario = null,
                                   bool requiereBiometria = false,
                                   string? criterioElegibilidad = null)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("Nombre de beneficio es obligatorio", nameof(nombre));
        if (cupoTotal.HasValue && cupoTotal.Value < 0)
            throw new ArgumentException("CupoTotal no puede ser negativo", nameof(cupoTotal));
        if (cupoPorUsuario.HasValue && cupoPorUsuario.Value < 0)
            throw new ArgumentException("CupoPorUsuario no puede ser negativo", nameof(cupoPorUsuario));
        if (vigenciaInicio.HasValue && vigenciaFin.HasValue && vigenciaInicio > vigenciaFin)
            throw new ArgumentException("VigenciaInicio debe ser anterior o igual a VigenciaFin");

        return new Beneficio
        {
            BeneficioId = Guid.NewGuid(),
            Nombre = nombre.Trim(),
            Tipo = tipo,
            Descripcion = null,
            VigenciaInicio = vigenciaInicio,
            VigenciaFin = vigenciaFin,
            CupoTotal = cupoTotal,
            CupoPorUsuario = cupoPorUsuario,
            RequiereBiometria = requiereBiometria,
            CriterioElegibilidad = criterioElegibilidad?.Trim()
        };
    }

    public bool IsVigente(DateTime atUtc)
    {
        return (!VigenciaInicio.HasValue || VigenciaInicio.Value <= atUtc)
            && (!VigenciaFin.HasValue || VigenciaFin.Value >= atUtc);
    }

    public bool HasCupoTotalDisponible(int cantidad = 1)
    {
        if (!CupoTotal.HasValue) return true; // ilimitado
        return CupoTotal.Value >= cantidad;
    }

    public void DecrementCupoTotal(int cantidad = 1)
    {
        if (!CupoTotal.HasValue) return; // ilimitado
        if (cantidad <= 0) return;
        if (CupoTotal.Value < cantidad)
            throw new InvalidOperationException("Cupo total insuficiente");
        CupoTotal -= cantidad;
    }

    public bool AllowsUserRedeem(int usedCountInRange)
    {
        if (!CupoPorUsuario.HasValue) return true;
        return usedCountInRange < CupoPorUsuario.Value;
    }

}