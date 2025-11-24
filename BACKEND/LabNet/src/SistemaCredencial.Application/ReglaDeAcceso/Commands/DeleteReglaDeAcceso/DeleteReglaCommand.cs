using MediatR;


namespace Espectaculos.Application.ReglaDeAcceso.Commands.DeleteReglaDeAcceso;

public class DeleteReglaCommand : IRequest<Guid>
{
    public Guid ReglaId { get; set; }
}