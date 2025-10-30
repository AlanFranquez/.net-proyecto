using MediatR;
using Espectaculos.Application.Abstractions;
using Espectaculos.Application.DTOs;

namespace Espectaculos.Application.Roles.Queries.GetRolById;

public class GetRolByIdHandler : IRequestHandler<GetRolByIdQuery, RolDTO?>
{
    private readonly IUnitOfWork _uow;
    public GetRolByIdHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<RolDTO?> Handle(GetRolByIdQuery q, CancellationToken ct)
    {
        var e = await _uow.Roles.GetByIdAsync(q.RolId, ct);
        if (e is null) return null;

        return new RolDTO
        {
            RolId = e.RolId,
            Tipo = e.Tipo,
            Prioridad = e.Prioridad,
            FechaAsignado = e.FechaAsignado,
            UsuariosIDs = e.UsuarioRoles.Select(ur => ur.UsuarioId).ToList()
        };
    }
}
