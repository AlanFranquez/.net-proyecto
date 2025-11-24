using FluentValidation;

namespace Espectaculos.Application.ReglaDeAcceso.Commands.UpdateReglaDeAcceso;

public class UpdateReglaValidator: AbstractValidator<UpdateReglaCommand>
{
    public UpdateReglaValidator()
    {
        RuleFor(x => x.ReglaId)
            .NotEmpty()
            .WithMessage("El ID es obligatorio.");
    }
}