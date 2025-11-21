using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Espectaculos.Application.Abstractions;
using Espectaculos.Application.DTOs;
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
            // 1) Traemos todos los eventos
            var eventos = await _uow.EventosAccesos.ListAsync(ct);

            // 2) Traemos todos los usuarios (o podrías optimizar más adelante)
            var usuarios = await _uow.Usuarios.ListAsync(ct);

            // 3) Mapeamos usuarios por CredencialId
            var usuariosPorCredencial = usuarios
                .Where(u => u.CredencialId.HasValue)
                .GroupBy(u => u.CredencialId!.Value)
                .ToDictionary(g => g.Key, g => g.First());

            // 4) Construimos los DTOs incluyendo nombre y email
            var dtos = eventos
                .Select(e =>
                {
                    Guid credId = e.CredencialId ?? Guid.Empty;
                    usuariosPorCredencial.TryGetValue(credId, out var usuario);

                    var nombreCompleto = usuario is null
                        ? null
                        : $"{usuario.Nombre} {usuario.Apellido}".Trim();

                    return new EventoAccesoDTO
                    {
                        EventoId        = e.EventoId,
                        MomentoDeAcceso = e.MomentoDeAcceso,
                        CredencialId    = e.CredencialId,
                        EspacioId       = e.EspacioId,
                        Resultado       = e.Resultado,
                        Motivo          = e.Motivo,
                        Modo            = e.Modo,
                        Firma           = e.Firma,
                        EspacioNombre   = e.Espacio?.Nombre,
                        UsuarioNombre   = nombreCompleto,
                        UsuarioEmail    = usuario?.Email
                    };
                })
                .OrderByDescending(e => e.MomentoDeAcceso)
                .ToList();

            return dtos;
        }
    }
}
