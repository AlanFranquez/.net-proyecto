using System;

namespace Espectaculos.WebApi.Endpoints.Dtos;

public record CreateBeneficioDto(string Nombre, object Tipo, string? Descripcion, DateTime? VigenciaInicio, DateTime? VigenciaFin, int? CupoTotal);

public record UpdateBeneficioDto(Guid Id, string Nombre, object? Tipo, string? Descripcion, DateTime? VigenciaInicio, DateTime? VigenciaFin, int? CupoTotal);
