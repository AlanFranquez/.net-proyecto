using Espectaculos.Application.Abstractions;
using Espectaculos.Domain.Entities;
using MediatR;

namespace Espectaculos.Application.Beneficios.Commands.UpdateBeneficio;

public class UpdateBeneficioHandler : IRequestHandler<UpdateBeneficioCommand, bool>
{
    private readonly IUnitOfWork _uow;

    public UpdateBeneficioHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<bool> Handle(UpdateBeneficioCommand request, CancellationToken cancellationToken)
    {
    var b = await _uow.Beneficios.GetByIdAsync(request.Id, cancellationToken);
        if (b is null) return false;

        b.Nombre = request.Nombre;
        b.Descripcion = request.Descripcion;
        b.VigenciaInicio = request.VigenciaInicio;
        b.VigenciaFin = request.VigenciaFin;
        if (request.CupoTotal.HasValue)
        {
            // adjust total cupo; domain stores CupoTotal
            b.CupoTotal = request.CupoTotal.Value;
        }

        // Note: RowVersion removed - concurrency managed by DB/EF automatically in this simplified flow

    _uow.Beneficios.Update(b);
        await _uow.SaveChangesAsync(cancellationToken);
        return true;
    }
}
