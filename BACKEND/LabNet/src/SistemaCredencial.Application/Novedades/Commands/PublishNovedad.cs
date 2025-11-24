using Espectaculos.Application.Abstractions;
using MediatR;

namespace Espectaculos.Application.Novedades.Commands.PublishUnpublish
{
    public record PublishNovedadCommand(Guid NovedadId, DateTime? DesdeUtc = null, DateTime? HastaUtc = null) : IRequest;
    public record UnpublishNovedadCommand(Guid NovedadId) : IRequest;

    public class PublishNovedadHandler : IRequestHandler<PublishNovedadCommand>
    {
        private readonly IUnitOfWork _uow;
        public PublishNovedadHandler(IUnitOfWork uow) => _uow = uow;

        public async Task Handle(PublishNovedadCommand command, CancellationToken ct)
        {
            var n = await _uow.Novedades.GetByIdAsync(command.NovedadId, ct)
                    ?? throw new KeyNotFoundException("Novedad no encontrada");
            n.Publish(command.DesdeUtc, command.HastaUtc);
            await _uow.Novedades.UpdateAsync(n, ct);
            await _uow.SaveChangesAsync(ct);
        }
    }

    public class UnpublishNovedadHandler : IRequestHandler<UnpublishNovedadCommand>
    {
        private readonly IUnitOfWork _uow;
        public UnpublishNovedadHandler(IUnitOfWork uow) => _uow = uow;

        public async Task Handle(UnpublishNovedadCommand command, CancellationToken ct)
        {
            var n = await _uow.Novedades.GetByIdAsync(command.NovedadId, ct)
                    ?? throw new KeyNotFoundException("Novedad no encontrada");
            n.Unpublish();
            await _uow.Novedades.UpdateAsync(n, ct);
            await _uow.SaveChangesAsync(ct);
        }
    }
}
