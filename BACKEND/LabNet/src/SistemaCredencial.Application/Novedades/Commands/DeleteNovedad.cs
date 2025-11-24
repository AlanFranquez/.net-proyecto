using Espectaculos.Application.Abstractions;
using MediatR;
namespace Espectaculos.Application.Novedades.Commands.DeleteNovedad;

public record DeleteNovedadCommand(Guid Id) : IRequest;

public class DeleteNovedadHandler : IRequestHandler<DeleteNovedadCommand>
{
    private readonly IUnitOfWork _uow;
    public DeleteNovedadHandler(IUnitOfWork uow) => _uow = uow;

    public async Task Handle(DeleteNovedadCommand r, CancellationToken ct)
    {
        var n = await _uow.Novedades.GetByIdAsync(r.Id, ct) ?? throw new KeyNotFoundException("Novedad no encontrada");
        await _uow.Novedades.DeleteAsync(n, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
