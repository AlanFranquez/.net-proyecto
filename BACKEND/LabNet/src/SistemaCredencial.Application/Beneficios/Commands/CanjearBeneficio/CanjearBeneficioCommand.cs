using MediatR;
using System;

namespace Espectaculos.Application.Beneficios.Commands.CanjearBeneficio;

public record CanjearBeneficioCommand(Guid BeneficioId, Guid UsuarioId) : IRequest<Guid>;
