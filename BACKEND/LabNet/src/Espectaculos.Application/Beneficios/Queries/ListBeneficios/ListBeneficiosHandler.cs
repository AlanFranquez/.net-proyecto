using Espectaculos.Application.Abstractions;
using Espectaculos.Domain.Entities;
using MediatR;

namespace Espectaculos.Application.Beneficios.Queries.ListBeneficios;

public class ListBeneficiosHandler : IRequestHandler<ListBeneficiosQuery, IReadOnlyList<Beneficio>>
{
    private readonly IUnitOfWork _uow;
    public ListBeneficiosHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<IReadOnlyList<Beneficio>> Handle(ListBeneficiosQuery request, CancellationToken cancellationToken)
    {
        return await _uow.Beneficios.ListAsync(cancellationToken);
    }
}
