using Espectaculos.Application.Abstractions;
using Espectaculos.Domain.Entities;
using MediatR;

namespace Espectaculos.Application.Novedades.Commands.PublishUnpublish
{
    // 👉 Commands sin resultado: implementan IRequest (no IRequest<Unit>)
    public record PublishNovedadCommand(Guid NovedadId, DateTime? DesdeUtc = null, DateTime? HastaUtc = null) : IRequest;
    public record UnpublishNovedadCommand(Guid NovedadId) : IRequest;

    // 👉 Handlers implementan IRequestHandler<T> y devuelven Task (no Task<Unit>)
    public class PublishNovedadHandler : IRequestHandler<PublishNovedadCommand>
    {
        private readonly IUnitOfWork _uow;
        public PublishNovedadHandler(IUnitOfWork uow) => _uow = uow;

        public async Task Handle(PublishNovedadCommand command, CancellationToken ct)
        {
            var repo = _uow.GetRepository<Novedad>(); // ✅ repo genérico
            var n = await repo.GetByIdAsync(command.NovedadId, ct) ?? throw new KeyNotFoundException("Novedad no encontrada");

            n.Publish(command.DesdeUtc, command.HastaUtc);

            await repo.UpdateAsync(n, ct);
            await _uow.SaveChangesAsync(ct);
        }
    }

    public class UnpublishNovedadHandler : IRequestHandler<UnpublishNovedadCommand>
    {
        private readonly IUnitOfWork _uow;
        public UnpublishNovedadHandler(IUnitOfWork uow) => _uow = uow;

        public async Task Handle(UnpublishNovedadCommand command, CancellationToken ct)
        {
            var repo = _uow.GetRepository<Novedad>(); // ✅ repo genérico
            var n = await repo.GetByIdAsync(command.NovedadId, ct) ?? throw new KeyNotFoundException("Novedad no encontrada");

            n.Unpublish();

            await repo.UpdateAsync(n, ct);
            await _uow.SaveChangesAsync(ct);
        }
    }
}