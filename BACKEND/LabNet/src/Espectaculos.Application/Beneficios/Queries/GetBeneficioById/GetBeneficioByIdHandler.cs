using Espectaculos.Application.Abstractions;
using Espectaculos.Application.DTOs;
using MediatR;

namespace Espectaculos.Application.Beneficios.Queries.GetBeneficioById;

public class GetBeneficioByIdHandler : IRequestHandler<GetBeneficioByIdQuery, BeneficioDTO?>
{
    private readonly IUnitOfWork _uow;
    public GetBeneficioByIdHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<BeneficioDTO?> Handle(GetBeneficioByIdQuery request, CancellationToken cancellationToken)
    {
        var b = await _uow.Beneficios.GetByIdAsync(request.Id, cancellationToken);
        if (b is null) return null;

        return new BeneficioDTO
        {
            Id = b.BeneficioId,
            Tipo = b.Tipo,
            Nombre = b.Nombre,
            Descripcion = b.Descripcion,
            VigenciaInicio = b.VigenciaInicio,
            VigenciaFin = b.VigenciaFin,
            CupoTotal = b.CupoTotal,
            CupoPorUsuario = b.CupoPorUsuario,
            RequiereBiometria = b.RequiereBiometria,
            CriterioElegibilidad = b.CriterioElegibilidad,
            EspaciosIDs = b.Espacios.Select(x => x.EspacioId).ToList(),
            UsuariosIDs = b.Usuarios.Select(x => x.UsuarioId).ToList()
        };
    }
}