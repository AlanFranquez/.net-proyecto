using MediatR;
using Espectaculos.Application.DTOs;

namespace Espectaculos.Application.EventoAcceso.Queries.ListarEventos
{
    public record ListarEventosQuery() : IRequest<List<EventoAccesoDTO>>;
}