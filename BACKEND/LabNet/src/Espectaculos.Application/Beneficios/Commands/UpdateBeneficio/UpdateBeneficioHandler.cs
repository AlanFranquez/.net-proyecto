using Espectaculos.Application.Abstractions;
using Espectaculos.Domain.Entities;
using MediatR;

namespace Espectaculos.Application.Beneficios.Commands.UpdateBeneficio;

public class UpdateBeneficioHandler : IRequestHandler<UpdateBeneficioCommand, bool>
{
    private readonly IUnitOfWork _uow;
    public UpdateBeneficioHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<bool> Handle(UpdateBeneficioCommand request, CancellationToken cancellationToken)
    {
    var b = await _uow.Beneficios.GetByIdAsync(request.Id, cancellationToken);
        if (b is null) return false;

        if (request.Nombre is not null)
            b.Nombre = request.Nombre.Trim();
        
        if (request.Descripcion is not null)
            b.Descripcion = request.Descripcion.Trim();
        
        if (request.VigenciaInicio.HasValue)
            b.VigenciaInicio = request.VigenciaInicio.Value;
        
        if (request.VigenciaFin.HasValue)
            b.VigenciaFin = request.VigenciaFin.Value;
        
        if (request.CupoTotal.HasValue)
            b.CupoTotal = request.CupoTotal.Value;
        
        if (request.CupoPorUsuario.HasValue)
            b.CupoPorUsuario = request.CupoPorUsuario.Value;
        
        if (request.RequiereBiometria.HasValue)
            b.RequiereBiometria = request.RequiereBiometria.Value;
        
        if (request.CriterioElegibilidad is not null)
            b.CriterioElegibilidad = request.CriterioElegibilidad.Trim();
        
        if (request.EspaciosIDs is not null)
        {
            var espaciosExistentes = await _uow.Espacios.ListByIdsAsync(request.EspaciosIDs, cancellationToken);
            if (espaciosExistentes.Count() != request.EspaciosIDs.Distinct().Count())
                throw new KeyNotFoundException("Algún espacio enviado no existe.");
            
            await _uow.Beneficios.RemoveEspaciosRelacionados(b.BeneficioId, cancellationToken);

            b.Espacios = request.EspaciosIDs
                .Distinct()
                .Select(bid => new BeneficioEspacio
                {
                    EspacioId = bid,
                    BeneficioId = b.BeneficioId
                })
                .ToList();
        }


    _uow.Beneficios.Update(b);
        await _uow.SaveChangesAsync(cancellationToken);
        return true;
    }
}
