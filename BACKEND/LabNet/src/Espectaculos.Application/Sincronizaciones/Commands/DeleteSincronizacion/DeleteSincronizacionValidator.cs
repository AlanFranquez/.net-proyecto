using FluentValidation;

namespace Espectaculos.Application.Sincronizaciones.Commands.DeleteSincronizacion;

public class DeleteSincronizacionValidator : AbstractValidator<DeleteSincronizacionCommand>
{
    public DeleteSincronizacionValidator()
    {
        RuleFor(x => x.SincronizacionId)
            .NotEmpty()
            .WithMessage("El ID es obligatorio.");
    }
    
}