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
        // 1. Crear beneficio (dominio)
        var b = Beneficio.Create(
            nombre: request.Nombre,
            tipo: request.Tipo,
            vigenciaInicio: request.VigenciaInicio,
            vigenciaFin: request.VigenciaFin,
            cupoTotal: request.CupoTotal
        );

        // 2. Asignar descripción si existe
        b.Descripcion = request.Descripcion?.Trim();

        // 3. Asignar espacios (si envías IDs)
        if (request.EspaciosIDs != null)
        {
            foreach (var espId in request.EspaciosIDs)
            {
                b.Espacios.Add(new BeneficioEspacio
                {
                    BeneficioId = b.BeneficioId,
                    EspacioId   = espId
                });
            }
        }

        // 4. Persistencia
        await _uow.Beneficios.AddAsync(b, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return b.BeneficioId;
    }
}
