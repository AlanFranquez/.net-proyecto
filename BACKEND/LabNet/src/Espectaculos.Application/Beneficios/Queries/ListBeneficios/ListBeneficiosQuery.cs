using MediatR;
using System.Collections.Generic;
using Espectaculos.Domain.Entities;

namespace Espectaculos.Application.Beneficios.Queries.ListBeneficios;

public record ListBeneficiosQuery() : IRequest<IReadOnlyList<Beneficio>>;
