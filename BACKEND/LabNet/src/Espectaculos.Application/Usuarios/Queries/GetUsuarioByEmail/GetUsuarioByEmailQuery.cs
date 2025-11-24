using Espectaculos.Application.DTOs;
using MediatR;

namespace Espectaculos.Application.Usuarios.Queries.GetUsuarioByEmail;

public record GetUsuarioByEmailQuery(string Email)
    : IRequest<UsuarioDto?>;