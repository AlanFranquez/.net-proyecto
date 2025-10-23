using MediatR;
using System;
using Espectaculos.Domain.Entities;

namespace Espectaculos.Application.Beneficios.Queries.GetBeneficioById;

public record GetBeneficioByIdQuery(Guid Id) : IRequest<Beneficio?>;
