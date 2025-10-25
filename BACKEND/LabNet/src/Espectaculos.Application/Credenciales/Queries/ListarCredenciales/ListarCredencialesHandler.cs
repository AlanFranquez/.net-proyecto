using Espectaculos.Application.Abstractions;
using Espectaculos.Application.DTOs;
using FluentValidation;
using MediatR;

namespace Espectaculos.Application.Credenciales.Queries.ListarCredenciales
{
    public class ListarCredencialesHandler : IRequestHandler<ListarCredencialesQuery, List<CredencialDTO>>
    {
        private readonly IUnitOfWork _uow;

        public ListarCredencialesHandler(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<List<CredencialDTO>> Handle(ListarCredencialesQuery query, CancellationToken ct)
        {
            var credenciales = await _uow.Credenciales.ListAsync(ct);

            return credenciales.Select(e => new CredencialDTO
            {
                CredencialId = e.CredencialId,
                Tipo = e.Tipo,
                Estado = e.Estado,
                IdCriptografico = e.IdCriptografico.Trim(),
                FechaEmision = e.FechaEmision,
                FechaExpiracion = e.FechaExpiracion,
                EventoAccesoIds = e.EventosAcceso.Select(a => a.EventoId).ToList()
            }).ToList();
        }
    }
}