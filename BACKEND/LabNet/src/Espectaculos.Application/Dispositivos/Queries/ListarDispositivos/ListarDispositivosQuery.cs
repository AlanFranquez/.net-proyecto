using MediatR;
using Espectaculos.Application.DTOs;

namespace Espectaculos.Application.Dispositivos.Queries.ListarDispositivos
{
    public record ListarDispositivosQuery() : IRequest<List<DispositivoDTO>>;
}