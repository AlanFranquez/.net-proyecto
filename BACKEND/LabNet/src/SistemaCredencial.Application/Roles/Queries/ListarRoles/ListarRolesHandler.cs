using Espectaculos.Application.Abstractions;
using Espectaculos.Application.DTOs;
using FluentValidation;
using MediatR;

namespace Espectaculos.Application.Roles.Queries.ListarRoles
{
    public class ListarRolesHandler : IRequestHandler<ListarRolesQuery, List<RolDTO>>
    {
        private readonly IUnitOfWork _uow;

        public ListarRolesHandler(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<List<RolDTO>> Handle(ListarRolesQuery query, CancellationToken ct)
        {
            var roles = await _uow.Roles.ListAsync(ct);

            return roles.Select(e => new RolDTO
            {
                RolId = e.RolId,
                Tipo = e.Tipo,
                Prioridad = e.Prioridad,
                FechaAsignado = e.FechaAsignado,
                UsuariosIDs = e.UsuarioRoles.Select(r => r.UsuarioId).ToList(),
            }).ToList();
        }
    }
}