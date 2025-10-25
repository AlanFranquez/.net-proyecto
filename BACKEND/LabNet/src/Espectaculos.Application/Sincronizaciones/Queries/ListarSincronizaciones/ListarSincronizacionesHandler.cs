using Espectaculos.Application.Abstractions;
using Espectaculos.Application.DTOs;
using MediatR;

namespace Espectaculos.Application.Sincronizaciones.Queries.ListarSincronizaciones
{
    public class ListarSincronizacionesHandler : IRequestHandler<ListarSincronizacionesQuery, List<SincronizacionDTO>>
    {
        private readonly IUnitOfWork _uow;

        public ListarSincronizacionesHandler(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<List<SincronizacionDTO>> Handle(ListarSincronizacionesQuery query, CancellationToken ct)
        {
            var sincronizaciones = await _uow.Sincronizaciones.ListAsync(ct);

            return sincronizaciones.Select(e => new SincronizacionDTO
            {
                SincronizacionId = e.SincronizacionId,
                CreadoEn = e.CreadoEn,
                CantidadItems = e.CantidadItems,
                Tipo = e.Tipo,
                Estado = e.Estado,
                DetalleError = e.DetalleError,
                Checksum = e.Checksum,
                DispositivoId = e.DispositivoId
            }).ToList();
        }
    }
}