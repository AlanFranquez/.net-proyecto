using Espectaculos.Domain.Enums;

namespace Espectaculos.Domain.Entities;

public class EventoAcceso
{
    public Guid EventoId { get; set; }
    public DateTime MomentoDeAcceso { get; set; }
    public Guid? CredencialId { get; set; }
    public Credencial? Credencial { get; set; } = default!;
    public Guid EspacioId { get; set; }
    public Espacio Espacio { get; set; } = default!;
    public AccesoTipo Resultado { get; set; }
    public string? Motivo { get; set; }
    public Modo Modo { get; set; }
    public string? Firma { get; set; }
}