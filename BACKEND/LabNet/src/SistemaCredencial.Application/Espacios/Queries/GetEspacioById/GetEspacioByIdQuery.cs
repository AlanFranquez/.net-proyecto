using MediatR;
using Espectaculos.Application.DTOs;
using System;

namespace Espectaculos.Application.Espacios.Queries.GetEspacioById
{
    public record GetEspacioByIdQuery(Guid Id) : IRequest<EspacioDTO?>;
}