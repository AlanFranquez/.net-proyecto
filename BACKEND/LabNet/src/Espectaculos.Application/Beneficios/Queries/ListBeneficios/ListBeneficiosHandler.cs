using Espectaculos.Application.Abstractions;
using Espectaculos.Application.DTOs;
using Espectaculos.Domain.Entities;
using MediatR;

namespace Espectaculos.Application.Beneficios.Queries.ListBeneficios;

public class ListBeneficiosHandler : IRequestHandler<ListBeneficiosQuery, IReadOnlyList<BeneficioDTO>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICacheService _cache;
    public ListBeneficiosHandler(IUnitOfWork uow, ICacheService cache)
    {
        _cache = cache;
        _uow = uow;
    }


    public async Task<IReadOnlyList<BeneficioDTO>> Handle(ListBeneficiosQuery request, CancellationToken cancellationToken)
    {
        string key = "shows:list";

        // Check cache
        var cached = await _cache.GetAsync<List<BeneficioDTO>>(key);
        if (cached != null)
            return cached;
        
        // DB fallback
        var beneficios = await _uow.Beneficios.ListAsync(cancellationToken);

        var DTbeneficios = beneficios.Select(e => new BeneficioDTO
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
        
        // Store in cache 60 seconds
        await _cache.SetAsync(key, DTbeneficios, TimeSpan.FromSeconds(60));

        return DTbeneficios;
    }
}
