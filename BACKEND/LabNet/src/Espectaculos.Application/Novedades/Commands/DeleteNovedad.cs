using Espectaculos.Application.Abstractions.Repositories;
using MediatR;

public record DeleteNovedadCommand(Guid Id) : IRequest;

public class DeleteNovedadHandler : IRequestHandler<DeleteNovedadCommand>
{
    private readonly INovedadRepository _repo;
    public DeleteNovedadHandler(INovedadRepository repo) => _repo = repo;

    public async Task<Unit> Handle(DeleteNovedadCommand r, CancellationToken ct)
    {
        var n = await _repo.GetAsync(r.Id, ct) ?? throw new KeyNotFoundException();
        await _repo.DeleteAsync(n, ct);
        return Unit.Value;
    }
}