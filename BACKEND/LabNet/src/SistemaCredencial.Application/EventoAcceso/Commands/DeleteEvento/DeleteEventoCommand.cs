using MediatR;

namespace Espectaculos.Application.EventoAcceso.Commands.DeleteEvento;

public class DeleteEventoCommand : IRequest<Guid>
{
    public Guid EventoId { get; set; }
}