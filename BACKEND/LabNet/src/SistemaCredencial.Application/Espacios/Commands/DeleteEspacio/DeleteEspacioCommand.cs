using MediatR;

namespace Espectaculos.Application.Espacios.Commands.DeleteEspacio;

public class DeleteEspacioCommand : IRequest<Guid>
{
    public Guid Id { get; set; }
}