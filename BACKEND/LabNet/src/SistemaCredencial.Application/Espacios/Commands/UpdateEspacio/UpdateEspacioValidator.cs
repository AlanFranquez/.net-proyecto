using FluentValidation;

namespace Espectaculos.Application.Espacios.Commands.UpdateEspacio;

public class UpdateEspacioValidator : AbstractValidator<UpdateEspacioCommand>
{
    public UpdateEspacioValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("El ID es obligatorio.");
    }
}