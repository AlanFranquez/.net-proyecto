using Espectaculos.Domain.Enums;
using MediatR;

namespace Espectaculos.Application.Credenciales.Commands.CreateCredencial
{
    public class CreateCredencialCommand : IRequest<Guid>
    {
        public CredencialTipo Tipo { get; set; }
        public CredencialEstado Estado { get; set; }
        public string? IdCriptografico { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime? FechaExpiracion { get; set; }
        public Guid UsuarioId { get; set; }
    }
}