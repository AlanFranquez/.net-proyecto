using MediatR;
using Espectaculos.Application.Abstractions;
using Espectaculos.Application.DTOs;

namespace Espectaculos.Application.Espacios.Queries.ListarEspacios;

public class ListarEspaciosHandler : IRequestHandler<ListarEspaciosQuery, List<EspacioDTO>>
{
    private readonly IUnitOfWork _uow;
    public ListarEspaciosHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<List<EspacioDTO>> Handle(ListarEspaciosQuery q, CancellationToken ct)
    {
        var espacios = await _uow.Espacios.ListAsync(ct);

        return espacios.Select(e => new EspacioDTO
        {
            Id = e.Id,
            Nombre = e.Nombre,
            Activo = e.Activo,
            Tipo = e.Tipo.ToString(),
            Modo = e.Modo.ToString(),
            ReglasCount = e.Reglas.Count,
            BeneficiosCount = e.Beneficios.Count,
            EventosCount = e.EventoAccesos.Count,
            ReglaIds = e.Reglas.Select(r => r.ReglaId).ToList(),
            BeneficioIds = e.Beneficios.Select(b => b.BeneficioId).ToList(),
            EventoIds = e.EventoAccesos.Select(ev => ev.EventoId).ToList(),
        }).ToList();
    }
}