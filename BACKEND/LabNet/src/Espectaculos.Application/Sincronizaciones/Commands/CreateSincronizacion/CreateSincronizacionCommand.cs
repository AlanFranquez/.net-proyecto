using MediatR;

namespace Espectaculos.Application.Sincronizaciones.Commands.CreateSincronizacion
{
    public class CreateSincronizacionCommand : IRequest<Guid>
    {
        public DateTime CreadoEn { get; set; } = DateTime.Now;
        public int CantidadItems { get; set; } = default!;
        public string? Tipo { get; set; } = null;
        public string? Estado { get; set; } = null;
        public string? DetalleError { get; set; } = null;
        public string? Checksum { get; set; } = null;
        public Guid DispositivoId  { get; set; }
    }
}