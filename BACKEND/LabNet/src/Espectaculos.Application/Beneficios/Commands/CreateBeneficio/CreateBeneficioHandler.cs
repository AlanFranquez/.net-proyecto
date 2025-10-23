using Espectaculos.Application.Abstractions;
using Espectaculos.Domain.Entities;
using MediatR;

namespace Espectaculos.Application.Beneficios.Commands.CreateBeneficio;

public class CreateBeneficioHandler : IRequestHandler<CreateBeneficioCommand, Guid>
{
    private readonly IUnitOfWork _uow;

    public CreateBeneficioHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Guid> Handle(CreateBeneficioCommand request, CancellationToken cancellationToken)
    {
        var b = Beneficio.Create(
            nombre: request.Nombre,
            tipo: request.Tipo,
            vigenciaInicio: request.VigenciaInicio,
            vigenciaFin: request.VigenciaFin,
            cupoTotal: request.CupoTotal);

        await _uow.Beneficios.AddAsync(b, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return b.BeneficioId;
    }
}
