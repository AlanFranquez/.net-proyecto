using Espectaculos.Domain.Enums;
using MediatR;

namespace Espectaculos.Application.Credenciales.Commands.UpdateCredencial
{
    public class UpdateCredencialCommand : IRequest<Guid>
    {
        public Guid CredencialId { get; set; }
        public CredencialTipo? Tipo { get; set; } = null;
        public CredencialEstado? Estado { get; set; } = null;
        public string? IdCriptografico { get; set; } = null;
        public DateTime? FechaEmision { get; set; } = null;
        public DateTime? FechaExpiracion { get; set; } = null;
        public IEnumerable<Guid>? EventoAccesoIds { get; set; } = null;
    }
}