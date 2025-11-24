using Espectaculos.Application.Abstractions;
using Espectaculos.Application.DTOs;
using MediatR;

namespace Espectaculos.Application.Dispositivos.Queries.ListarDispositivos
{
    public class ListarDispositivosHandler : IRequestHandler<ListarDispositivosQuery, List<DispositivoDTO>>
    {
        private readonly IUnitOfWork _uow;

        public ListarDispositivosHandler(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<List<DispositivoDTO>> Handle(ListarDispositivosQuery query, CancellationToken ct)
        {
            var dispositivos = await _uow.Dispositivos.ListAsync(ct);

            return dispositivos.Select(e => new DispositivoDTO
            {
                DispositivoId = e.DispositivoId,
                NumeroTelefono = e.NumeroTelefono,
                Plataforma = e.Plataforma,
                HuellaDispositivo = e.HuellaDispositivo,
                NavegadorNombre = e.NavegadorNombre,
                NavegadorVersion = e.NavegadorVersion,

                BiometriaHabilitada = e.BiometriaHabilitada,
                Estado = e.Estado,
                UsuarioId = e.UsuarioId,

                NotificacionesIds = e.Notificaciones?.Select(n => n.NotificacionId).ToList(),
                SincronizacionesIds = e.Sincronizaciones?.Select(s => s.SincronizacionId).ToList(),
            }).ToList();
        }
    }
}