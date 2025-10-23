using Espectaculos.Application.Abstractions;
using Espectaculos.Application.DTOs;
using FluentValidation;
using MediatR;

namespace Espectaculos.Application.Espacios.Queries.ListarEspacios
{
    public class ListarEspaciosHandler : IRequestHandler<ListarEspaciosQuery, List<EspacioDTO>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IValidator<ListarEspaciosQuery> _validator;

        public ListarEspaciosHandler(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<List<EspacioDTO>> Handle(ListarEspaciosQuery query, CancellationToken ct)
        {
            var espacios = await _uow.Espacios.ListAsync(ct);

            return espacios.Select(e => new EspacioDTO
            {
                Id = e.Id,
                Nombre = e.Nombre,
                Activo = e.Activo,
                Tipo = e.Tipo,
                Modo = e.Modo,
                ReglaIds = e.Reglas.Select(r => r.ReglaId).ToList(),
                BeneficioIds = e.Beneficios.Select(b => b.BeneficioId).ToList(),
                EventoAccesoIds = e.EventoAccesos.Select(a => a.EventoId).ToList()
            }).ToList();
        }
    }
}