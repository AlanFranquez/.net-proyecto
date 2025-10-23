using Espectaculos.Application.Abstractions;
using Espectaculos.Domain.Entities;
using MediatR;

namespace Espectaculos.Application.Beneficios.Queries.GetBeneficioById;

public class GetBeneficioByIdHandler : IRequestHandler<GetBeneficioByIdQuery, Beneficio?>
{
    private readonly IUnitOfWork _uow;
    public GetBeneficioByIdHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Beneficio?> Handle(GetBeneficioByIdQuery request, CancellationToken cancellationToken)
        => await _uow.Beneficios.GetByIdAsync(request.Id, cancellationToken);
}
