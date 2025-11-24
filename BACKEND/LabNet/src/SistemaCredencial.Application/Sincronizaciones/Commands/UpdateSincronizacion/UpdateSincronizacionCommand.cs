using MediatR;

namespace Espectaculos.Application.Sincronizaciones.Commands.UpdateSincronizacion
{
    public class UpdateSincronizacionCommand : IRequest<Guid>
    {
        public Guid SincronizacionId { get; set; }
        public DateTime? CreadoEn { get; set; } = null;
        public int? CantidadItems { get; set; } = null;
        public string? Tipo { get; set; } = null;
        public string? Estado { get; set; } = null;
        public string? DetalleError { get; set; } = null;
        public string? Checksum { get; set; } = null;
        public Guid? DispositivoId  { get; set; } = null;
    }
}