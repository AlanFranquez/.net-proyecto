using Espectaculos.Application.Abstractions;
using Espectaculos.Application.DTOs;
using MediatR;

namespace Espectaculos.Application.Usuarios.Queries.GetUsuarioByEmail;

public class GetUsuarioByEmailHandler 
    : IRequestHandler<GetUsuarioByEmailQuery, UsuarioDto?>
{
    private readonly IUnitOfWork _uow;

    public GetUsuarioByEmailHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<UsuarioDto?> Handle(GetUsuarioByEmailQuery request, CancellationToken ct)
    {
        var usuario = await _uow.Usuarios.GetByEmailAsync(request.Email, ct);
        if (usuario is null) return null;

        var roles = usuario.UsuarioRoles
            .Select(ur => ur.Rol.Tipo)
            .ToList();

        return new UsuarioDto
        {
            UsuarioId = usuario.UsuarioId,
            Email = usuario.Email,
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            RolesIDs = usuario.UsuarioRoles.Select(ur => ur.RolId).ToList(), // si querés ids
            Estado = usuario.Estado
        };
    }
}