using Espectaculos.Domain.Enums;
using MediatR;

namespace Espectaculos.Application.Dispositivos.Commands.CreateDispositivo
{
    public class CreateDispositivoCommand : IRequest<Guid>
    {
        public string? NumeroTelefono { get; set; }
        public PlataformaTipo Plataforma { get; set; }
        public string? HuellaDispositivo { get; set; }
        public bool BiometriaHabilitada { get; set; }
        public DispositivoTipo Estado { get; set; }
        public Guid UsuarioId { get; set; }
    }
}