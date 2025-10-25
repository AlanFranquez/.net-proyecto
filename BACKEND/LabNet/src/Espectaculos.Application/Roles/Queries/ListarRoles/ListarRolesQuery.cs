using MediatR;
using Espectaculos.Application.DTOs;

namespace Espectaculos.Application.Roles.Queries.ListarRoles
{
    public record ListarRolesQuery() : IRequest<List<RolDTO>>;
}