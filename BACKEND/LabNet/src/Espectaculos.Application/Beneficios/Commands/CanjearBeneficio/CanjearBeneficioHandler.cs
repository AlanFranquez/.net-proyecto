using Espectaculos.Application.Abstractions;
using Espectaculos.Domain.Entities;
using MediatR;

namespace Espectaculos.Application.Beneficios.Commands.CanjearBeneficio;

public class CanjearBeneficioHandler : IRequestHandler<CanjearBeneficioCommand, Guid>
{
    private readonly IUnitOfWork _uow;

    public CanjearBeneficioHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Guid> Handle(CanjearBeneficioCommand request, CancellationToken cancellationToken)
    {
        var beneficio = await _uow.Beneficios.GetByIdAsync(request.BeneficioId, cancellationToken);
        if (beneficio == null) throw new KeyNotFoundException("Beneficio no encontrado");

    var now = DateTime.UtcNow;
    if (!beneficio.IsVigente(now)) throw new InvalidOperationException("Beneficio no vigente");
    if (!beneficio.HasCupoTotalDisponible(1)) throw new InvalidOperationException("Cupo insuficiente");

        // decrementar cupo en dominio
        beneficio.DecrementCupoTotal(1);

        // crear canje
    var canje = Espectaculos.Domain.Entities.Canje.CreatePending(beneficioId: beneficio.BeneficioId, usuarioId: request.UsuarioId, fechaUtc: now);
        canje.Confirm(); // confirmar inmediatamente para este MVP

        await _uow.Canjes.AddAsync(canje, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return canje.CanjeId;
    }
}
