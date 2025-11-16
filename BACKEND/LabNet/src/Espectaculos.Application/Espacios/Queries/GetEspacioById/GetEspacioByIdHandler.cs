using MediatR;
using Espectaculos.Application.Abstractions;
using Espectaculos.Application.DTOs;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Espectaculos.Application.Espacios.Queries.GetEspacioById
{
    public class GetEspacioByIdHandler : IRequestHandler<GetEspacioByIdQuery, EspacioDTO?>
    {
        private readonly IUnitOfWork _uow;
        public GetEspacioByIdHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<EspacioDTO?> Handle(GetEspacioByIdQuery q, CancellationToken ct)
        {
            var e = await _uow.Espacios.GetByIdAsync(q.Id, ct);
            if (e is null) return null;

            return new EspacioDTO
            {
                Id              = e.Id,
                Nombre          = e.Nombre,
                Activo          = e.Activo,
                Tipo            = e.Tipo.ToString(),
                Modo            = e.Modo.ToString(),
                ReglasCount     = e.Reglas.Count,
                BeneficiosCount = e.Beneficios.Count,
                EventosCount    = e.EventoAccesos.Count,
                ReglaIds        = e.Reglas.Select(r => r.ReglaId).ToList(),
                BeneficioIds    = e.Beneficios.Select(b => b.BeneficioId).ToList(),
                EventoIds       = e.EventoAccesos.Select(ev => ev.EventoId).ToList(),
            };
        }
    }
}