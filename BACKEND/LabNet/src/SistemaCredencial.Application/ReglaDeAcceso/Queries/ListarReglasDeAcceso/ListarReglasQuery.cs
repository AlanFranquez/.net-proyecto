using MediatR;
using Espectaculos.Application.DTOs;

namespace Espectaculos.Application.ReglaDeAcceso.Queries.ListarReglasDeAcceso
{
    public record ListarReglasQuery() : IRequest<List<ReglaDeAccesoDTO>>;
}
