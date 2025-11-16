using System;
using Espectaculos.Application.DTOs;
using MediatR;

namespace Espectaculos.Application.Usuarios.Queries.GetUsuarioById;

public record GetUsuarioByIdQuery(Guid UsuarioId) : IRequest<UsuarioDto?>;