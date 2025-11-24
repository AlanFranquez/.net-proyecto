using Espectaculos.Application.Abstractions;
using Espectaculos.Application.Notificaciones.Dtos;
using MediatR;

namespace Espectaculos.Application.Usuarios.Queries.GetUsuarioByEmail;

public class GetUsuarioByEmailHandler : IRequestHandler<GetUsuarioByEmailQuery, Object?>
{
    private readonly IUnitOfWork _uow;

    public GetUsuarioByEmailHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Object?> Handle(GetUsuarioByEmailQuery request, CancellationToken cancellationToken)
    {
        var usuario = await _uow.Usuarios.GetByEmailAsync(request.Email, cancellationToken);
        if (usuario is null) return null;
        
        var roles = new List<string>();
        if (!usuario.UsuarioRoles.Any())
        {
            roles = usuario.UsuarioRoles.Select(ur => ur.Rol.Tipo).ToList();
        }
        var dto = new
        {
            Id = usuario.UsuarioId,
            Email = usuario.Email,
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            Roles = roles,
            Estado = usuario.Estado.ToString()
        };

        return dto;
    }
}
