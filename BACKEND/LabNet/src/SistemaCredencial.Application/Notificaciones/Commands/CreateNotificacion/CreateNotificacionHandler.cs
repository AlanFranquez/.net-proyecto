using Espectaculos.Application.Abstractions;
using Espectaculos.Domain.Entities;
using MediatR;

namespace Espectaculos.Application.Notificaciones.Commands.CreateNotificacion;

public class CreateNotificacionHandler : IRequestHandler<CreateNotificacionCommand, Guid>
{
    private readonly IUnitOfWork _uow;

    public CreateNotificacionHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Guid> Handle(CreateNotificacionCommand request, CancellationToken cancellationToken)
    {
        DateTime? programadaUtc = null;
        if (request.ProgramadaParaUtc.HasValue)
        {
            var dt = request.ProgramadaParaUtc.Value;
            programadaUtc = dt.Kind switch
            {
                DateTimeKind.Utc => dt,
                DateTimeKind.Local => dt.ToUniversalTime(),
                _ => DateTime.SpecifyKind(dt, DateTimeKind.Utc)
            };
        }

    var entity = Notificacion.Create(request.Tipo, request.Titulo, request.Cuerpo, programadaUtc, request.Audiencia);

        await _uow.Notificaciones.AddAsync(entity, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return entity.NotificacionId;
    }
}
