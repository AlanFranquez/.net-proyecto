using Espectaculos.Domain.Enums;
using MediatR;

namespace Espectaculos.Application.EventoAcceso.Commands.CreateEvento
{
    public class CreateEventoCommand : IRequest<Guid>
    {
        public DateTime MomentoDeAcceso { get; set; } = default!;
        public Guid CredencialId { get; set; }
        public Guid EspacioId { get; set; }
        public AccesoTipo Resultado { get; set; }
        public string? Motivo { get; set; }
        public Modo Modo { get; set; }
        public string? Firma { get; set; }
    }
}