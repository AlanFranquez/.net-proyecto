using FluentValidation;

namespace Espectaculos.Application.Sincronizaciones.Commands.UpdateSincronizacion;

public class UpdateSincronizacionValidator : AbstractValidator<UpdateSincronizacionCommand>
{
    public UpdateSincronizacionValidator()
    {
        RuleFor(x => x.SincronizacionId)
            .NotEmpty()
            .WithMessage("El ID es obligatorio.");
    }
    
}