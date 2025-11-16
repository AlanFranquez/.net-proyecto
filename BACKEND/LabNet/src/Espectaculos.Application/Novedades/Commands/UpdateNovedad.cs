using Espectaculos.Application.Abstractions;
using Espectaculos.Domain.Enums;
using FluentValidation;
using MediatR;

namespace Espectaculos.Application.Novedades.Commands.UpdateNovedad;

public record UpdateNovedadCommand(
    Guid NovedadId,
    string Titulo,
    string? Contenido,
    NotificacionTipo Tipo,
    DateTime? PublicadoDesdeUtc,
    DateTime? PublicadoHastaUtc
) : IRequest;

public class UpdateNovedadValidator : AbstractValidator<UpdateNovedadCommand>
{
    public UpdateNovedadValidator()
    {
        RuleFor(x => x.NovedadId).NotEmpty();
        RuleFor(x => x.Titulo).NotEmpty().MaximumLength(200);
        RuleFor(x => x)
            .Must(x => !(x.PublicadoDesdeUtc.HasValue && x.PublicadoHastaUtc.HasValue && x.PublicadoDesdeUtc > x.PublicadoHastaUtc))
            .WithMessage("PublicadoDesde debe ser anterior o igual a PublicadoHasta");
        
    }
}

public class UpdateNovedadHandler : IRequestHandler<UpdateNovedadCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly IValidator<UpdateNovedadCommand> _validator;

    public UpdateNovedadHandler(IUnitOfWork uow, IValidator<UpdateNovedadCommand> validator)
    {
        _uow = uow;
        _validator = validator;
    }

    public async Task Handle(UpdateNovedadCommand command, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAsync(command, ct);

        var repo = _uow.Novedades;
        var n = await repo.GetByIdAsync(command.NovedadId, ct)
                ?? throw new KeyNotFoundException("Novedad no encontrada");

        n.Titulo = command.Titulo.Trim();
        n.Contenido = command.Contenido;
        n.Tipo = command.Tipo;
        n.PublicadoDesdeUtc = command.PublicadoDesdeUtc;
        n.PublicadoHastaUtc = command.PublicadoHastaUtc;

        await repo.UpdateAsync(n, ct);
        await _uow.SaveChangesAsync(ct);
        // no returns
    }
}