using MediatR;
using Espectaculos.Application.DTOs;

namespace Espectaculos.Application.Roles.Queries.GetRolById;

public record GetRolByIdQuery(Guid RolId) : IRequest<RolDTO?>;