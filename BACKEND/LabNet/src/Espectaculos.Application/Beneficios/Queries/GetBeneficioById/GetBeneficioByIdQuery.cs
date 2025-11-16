using Espectaculos.Application.DTOs;
using MediatR;

namespace Espectaculos.Application.Beneficios.Queries.GetBeneficioById;

public record GetBeneficioByIdQuery(Guid Id) : IRequest<BeneficioDTO?>;