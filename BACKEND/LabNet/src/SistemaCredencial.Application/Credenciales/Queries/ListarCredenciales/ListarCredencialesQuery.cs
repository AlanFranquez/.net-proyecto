using MediatR;
using Espectaculos.Application.DTOs;

namespace Espectaculos.Application.Credenciales.Queries.ListarCredenciales
{
    public record ListarCredencialesQuery() : IRequest<List<CredencialDTO>>;
}