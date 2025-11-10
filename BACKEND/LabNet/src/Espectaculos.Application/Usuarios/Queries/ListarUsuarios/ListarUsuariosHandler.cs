using Espectaculos.Application.Abstractions.Repositories;
using Espectaculos.Application.DTOs;
using MediatR;

namespace Espectaculos.Application.Usuarios.Queries.ListarUsuarios
{

    public class ListarUsuariosHandler : IRequestHandler<ListarUsuariosQuery, List<UsuarioDto>>
    {
        private readonly IUsuarioRepository _repo;

        public ListarUsuariosHandler(IUsuarioRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<UsuarioDto>> Handle(ListarUsuariosQuery request, CancellationToken cancellationToken)
        {
            var usuarios = await _repo.ListAsync();

            return usuarios.Select(u => new UsuarioDto
            {
                UsuarioId = u.UsuarioId,
                Nombre = u.Nombre,
                Apellido = u.Apellido,
                Email = u.Email,
                Documento = u.Documento,
                Estado = u.Estado,
                Password = u.PasswordHash,
                CredencialId = u.CredencialId,
                RolesIDs = u.UsuarioRoles.Select(r => r.RolId).ToList(),
                DispositivosIDs = u.Dispositivos.Select(r => r.DispositivoId).ToList(),
                BeneficiosIDs = u.Beneficios.Select(r => r.BeneficioId).ToList(),
                CanjesIDs = u.Canjes.Select(r => r.CanjeId).ToList(),
            }).ToList();
        }
    }
}