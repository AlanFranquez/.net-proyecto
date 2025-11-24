using Espectaculos.Domain.Enums;
using MediatR;

namespace Espectaculos.Application.Roles.Commands.CreateRol
{
    public class CreateRolCommand : IRequest<Guid>
    {
        public string Tipo { get; set; } = default!;
        public int Prioridad { get; set; }
        public DateTime FechaAsignado { get; set; }
    }
}