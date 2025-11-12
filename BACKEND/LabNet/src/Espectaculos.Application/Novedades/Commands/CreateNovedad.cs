using Espectaculos.Domain.Entities;
using Espectaculos.Domain.Enums;
using Espectaculos.Application.Abstractions;
using FluentValidation;
using MediatR;

namespace Espectaculos.Application.Novedades.Commands.CreateNovedad;

public record CreateNovedadCommand(string Titulo, string? Contenido, NotificacionTipo Tipo,
    DateTime? DesdeUtc = null, DateTime? HastaUtc = null) : IRequest<Guid>;

public class CreateNovedadValidator : AbstractValidator<CreateNovedadCommand>
{
    public CreateNovedadValidator()
    {
        RuleFor(x => x.Titulo).NotEmpty().MaximumLength(200);
        RuleFor(x => x)
            .Must(x => !(x.DesdeUtc.HasValue && x.HastaUtc.HasValue && x.DesdeUtc > x.HastaUtc))
            .WithMessage("PublicadoDesde debe ser anterior o igual a PublicadoHasta");
    }
}

public class CreateNovedadHandler : IRequestHandler<CreateNovedadCommand, Guid>
{
    private readonly IUnitOfWork _uow;

    public CreateNovedadHandler(IUnitOfWork uow)
        => _uow = uow;

    public async Task<Guid> Handle(CreateNovedadCommand c, CancellationToken ct)
    {
        var n = Novedad.Create(c.Titulo, c.Contenido, c.Tipo, c.DesdeUtc, c.HastaUtc);

        await _uow.Novedades.AddAsync(n, ct);
        await _uow.SaveChangesAsync(ct);

        return n.NovedadId;
    }
}