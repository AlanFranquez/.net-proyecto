using Espectaculos.Application.Abstractions;
using Espectaculos.Application.DTOs;
using FluentValidation;
using MediatR;

namespace Espectaculos.Application.EventoAcceso.Queries.ListarEventos
{
    public class ListarEventosHandler : IRequestHandler<ListarEventosQuery, List<EventoAccesoDTO>>
    {
        private readonly IUnitOfWork _uow;

        public ListarEventosHandler(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<List<EventoAccesoDTO>> Handle(ListarEventosQuery query, CancellationToken ct)
        {
            var eventos = await _uow.EventosAccesos.ListAsync(ct);

            return eventos.Select(e => new EventoAccesoDTO
            {
                EventoId = e.EventoId,
                MomentoDeAcceso = e.MomentoDeAcceso,
                CredencialId = e.CredencialId,
                EspacioId = e.EspacioId,
                Resultado = e.Resultado,
                Motivo = e.Motivo?.Trim(),
                Modo = e.Modo,
                Firma = e.Firma
            }).ToList();
        }
    }
}