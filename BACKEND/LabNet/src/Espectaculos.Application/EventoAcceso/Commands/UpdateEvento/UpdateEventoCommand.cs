using Espectaculos.Domain.Enums;
using MediatR;

namespace Espectaculos.Application.EventoAcceso.Commands.UpdateEvento
{
    public class UpdateEventoCommand : IRequest<Guid>
    {
        public Guid EventoId { get; set; }
        public DateTime? MomentoDeAcceso { get; set; } = null;
        public Guid? CredencialId { get; set; } = null;
        public Guid? EspacioId { get; set; } = null;
        public AccesoTipo? Resultado { get; set; } = null;
        public string? Motivo { get; set; } = null;
        public Modo? Modo { get; set; } = null;
        public string? Firma { get; set; } = null;
    }
}