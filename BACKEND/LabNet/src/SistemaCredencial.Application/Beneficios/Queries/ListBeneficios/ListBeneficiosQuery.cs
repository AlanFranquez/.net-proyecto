using MediatR;
using System.Collections.Generic;
using Espectaculos.Application.DTOs;

namespace Espectaculos.Application.Beneficios.Queries.ListBeneficios;

public record ListBeneficiosQuery() : IRequest<IReadOnlyList<BeneficioDTO>>;
