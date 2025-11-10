using Espectaculos.Application.Abstractions.Repositories;
using Espectaculos.Domain.Entities;
using Espectaculos.Domain.Enums;
using MediatR;

public record CreateNovedadCommand(string Titulo, string? Contenido, NotificacionTipo Tipo,
    DateTime? DesdeUtc = null, DateTime? HastaUtc = null,
    bool PublicarAhora = false) : IRequest<Guid>;

public class CreateNovedadHandler : IRequestHandler<CreateNovedadCommand, Guid>
{
    private readonly INovedadRepository _repo;
    public CreateNovedadHandler(INovedadRepository repo) => _repo = repo;

    public async Task<Guid> Handle(CreateNovedadCommand r, CancellationToken ct)
    {
        var n = Novedad.Create(r.Titulo, r.Contenido, r.Tipo, r.DesdeUtc, r.HastaUtc);
        if (r.PublicarAhora) n.Publish(r.DesdeUtc, r.HastaUtc);

        await _repo.AddAsync(n, ct);
        return n.NovedadId;
    }
}
