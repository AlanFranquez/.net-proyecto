using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Espectaculos.Application.Abstractions;
using Espectaculos.Application.DTOs;
using MediatR;

namespace Espectaculos.Application.Usuarios.Queries.GetUsuarioById;

public class GetUsuarioByIdHandler : IRequestHandler<GetUsuarioByIdQuery, UsuarioDto?>
{
    private readonly IUnitOfWork _uow;

    public GetUsuarioByIdHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<UsuarioDto?> Handle(GetUsuarioByIdQuery request, CancellationToken ct)
    {
        var u = await _uow.Usuarios.GetByIdAsync(request.UsuarioId, ct);
        if (u is null) return null;

        return new UsuarioDto
        {
            UsuarioId = u.UsuarioId,
            Documento = u.Documento,
            Nombre = u.Nombre,
            Apellido = u.Apellido,
            Email = u.Email,
            Estado = u.Estado,
            Password = "",
            CredencialId = u.CredencialId,

            RolesIDs = u.UsuarioRoles
                .Select(x => x.RolId)
                .ToList(),

            DispositivosIDs = u.Dispositivos
                .Select(x => x.DispositivoId)
                .ToList(),

            BeneficiosIDs = u.Beneficios
                .Select(x => x.BeneficioId)
                .ToList(),

            CanjesIDs = u.Canjes
                .Select(x => x.CanjeId)
                .ToList()
        };
    }
}