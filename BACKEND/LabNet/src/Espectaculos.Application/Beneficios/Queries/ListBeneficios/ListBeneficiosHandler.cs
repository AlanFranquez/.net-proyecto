using Espectaculos.Application.Abstractions;
using Espectaculos.Application.DTOs;
using Espectaculos.Domain.Entities;
using MediatR;

namespace Espectaculos.Application.Beneficios.Queries.ListBeneficios;

public class ListBeneficiosHandler : IRequestHandler<ListBeneficiosQuery, IReadOnlyList<BeneficioDTO>>
{
    private readonly IUnitOfWork _uow;
    public ListBeneficiosHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<IReadOnlyList<BeneficioDTO>> Handle(ListBeneficiosQuery request, CancellationToken cancellationToken)
    {
        var espacios = await _uow.Beneficios.ListAsync(cancellationToken);

        return espacios.Select(e => new BeneficioDTO
        {
            Id = e.BeneficioId,
            Tipo = e.Tipo,
            Nombre = e.Nombre,
            Descripcion = e.Descripcion,
            VigenciaInicio = e.VigenciaInicio,
            VigenciaFin = e.VigenciaFin,
            CupoTotal = e.CupoTotal,
            CupoPorUsuario = e.CupoPorUsuario,
            RequiereBiometria = e.RequiereBiometria,
            CriterioElegibilidad = e.CriterioElegibilidad,
            EspaciosIDs = e.Espacios.Select(b => b.EspacioId).ToList(),
            UsuariosIDs = e.Usuarios.Select(a => a.UsuarioId).ToList()
        }).ToList();
    }
}
