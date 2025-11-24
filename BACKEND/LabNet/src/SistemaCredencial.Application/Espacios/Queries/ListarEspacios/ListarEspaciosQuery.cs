using MediatR;
using Espectaculos.Application.DTOs;

namespace Espectaculos.Application.Espacios.Queries.ListarEspacios;

public record ListarEspaciosQuery() : IRequest<List<EspacioDTO>>;