using MediatR;
using Espectaculos.Application.DTOs;

namespace Espectaculos.Application.Sincronizaciones.Queries.ListarSincronizaciones
{
    public record ListarSincronizacionesQuery() : IRequest<List<SincronizacionDTO>>;
}