using MediatR;
using Espectaculos.Application.DTOs;

namespace Espectaculos.Application.Espacios.Queries.GetEspacioById;

public record GetEspacioByIdQuery(Guid Id) : IRequest<EspacioDTO?>;