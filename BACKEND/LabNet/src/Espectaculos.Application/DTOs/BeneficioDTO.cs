using Espectaculos.Domain.Enums;

namespace Espectaculos.Application.DTOs;

public class BeneficioDTO
{
    public Guid Id { get; set; }
    public BeneficioTipo? Tipo { get; set; } = null;
    public string? Nombre { get; set; } = null;
    public string? Descripcion { get; set; } = null;
    public DateTime? VigenciaInicio { get; set; } = null;
    public DateTime? VigenciaFin { get; set; } = null;
    public int? CupoTotal { get; set; } = null;
    public int? CupoPorUsuario { get; set; } = null;
    public bool? RequiereBiometria { get; set; } = null;
    public string? CriterioElegibilidad { get; set; } = null;
    public ICollection<Guid>? EspaciosIDs { get; set; } = null;
    public ICollection<Guid>? UsuariosIDs { get; set; } = null;
}