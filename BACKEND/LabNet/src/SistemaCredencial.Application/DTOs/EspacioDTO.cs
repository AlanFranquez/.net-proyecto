namespace Espectaculos.Application.DTOs;

public class EspacioDTO
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = default!;
    public bool Activo { get; set; }
    public string Tipo { get; set; } = default!; 
    public string Modo { get; set; } = default!;
    public int ReglasCount { get; set; }
    public int BeneficiosCount { get; set; }
    public int EventosCount { get; set; }
    public List<Guid> ReglaIds { get; set; } = new();
    public List<Guid> BeneficioIds { get; set; } = new();
    public List<Guid> EventoIds { get; set; } = new();
}
