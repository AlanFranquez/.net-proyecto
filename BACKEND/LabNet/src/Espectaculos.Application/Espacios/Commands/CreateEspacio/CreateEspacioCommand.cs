using Espectaculos.Domain.Enums;
using MediatR;

namespace Espectaculos.Application.Espacios.Commands.CreateEspacio
{
    public class CreateEspacioCommand : IRequest<Guid>
    {
        public string Nombre { get; set; } = default!;
        public bool Activo { get; set; }
        public EspacioTipo Tipo { get; set; }
        public Modo Modo { get; set; }
        public List<Guid> ReglaIds { get; set; } = new();
        public List<Guid> BeneficioIds { get; set; } = new();
    }
}