using MediatR;
using System;

namespace Espectaculos.Application.Beneficios.Commands.UpdateBeneficio;

public record UpdateBeneficioCommand(Guid Id, string Nombre, string? Descripcion, DateTime? VigenciaInicio, DateTime? VigenciaFin, int? CupoTotal) : IRequest<bool>;
