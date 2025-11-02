using Espectaculos.Application.Notificaciones.Dtos;
using MediatR;

namespace Espectaculos.Application.Usuarios.Queries.GetUsuarioByEmail;

public record GetUsuarioByEmailQuery(string Email) : IRequest<Object?>;

