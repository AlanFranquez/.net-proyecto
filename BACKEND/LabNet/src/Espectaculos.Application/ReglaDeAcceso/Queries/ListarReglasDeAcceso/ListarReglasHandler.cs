using Espectaculos.Application.Abstractions;
using Espectaculos.Application.DTOs;
using FluentValidation;
using MediatR;

namespace Espectaculos.Application.ReglaDeAcceso.Queries.ListarReglasDeAcceso
{
    public class ListarReglasHandler : IRequestHandler<ListarReglasQuery, List<ReglaDeAccesoDTO>>
    {
        private readonly IUnitOfWork _uow;

        public ListarReglasHandler(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<List<ReglaDeAccesoDTO>> Handle(ListarReglasQuery query, CancellationToken ct)
        {
            var espacios = await _uow.Reglas.ListAsync(ct);

            return espacios.Select(r => new ReglaDeAccesoDTO
            {
                ReglaId = r.ReglaId,
                VentanaHoraria = r.VentanaHoraria.Trim(),
                VigenciaInicio = r.VigenciaInicio,
                VigenciaFin = r.VigenciaFin,
                Prioridad = r.Prioridad,
                Politica = r.Politica,
                RequiereBiometriaConfirmacion = r.RequiereBiometriaConfirmacion.Equals(true),
                EspaciosIDs = r.Espacios.Select(r => r.EspacioId).ToList()
            }).ToList();
            
        }
        
    }
    
}