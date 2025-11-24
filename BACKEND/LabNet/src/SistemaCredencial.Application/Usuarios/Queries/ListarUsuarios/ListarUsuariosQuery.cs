using MediatR;
using Espectaculos.Application.DTOs;
using System.Collections.Generic;

namespace Espectaculos.Application.Usuarios.Queries.ListarUsuarios
{
    public record ListarUsuariosQuery() : IRequest<List<UsuarioDto>>;
}