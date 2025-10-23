using MediatR;
using System;
using Espectaculos.Domain.Enums;

namespace Espectaculos.Application.Beneficios.Commands.CreateBeneficio;

public record CreateBeneficioCommand(string Nombre, BeneficioTipo Tipo, string? Descripcion, DateTime? VigenciaInicio, DateTime? VigenciaFin, int? CupoTotal) : IRequest<Guid>;
