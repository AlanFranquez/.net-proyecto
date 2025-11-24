using FluentValidation;

namespace Espectaculos.Application.Espacios.Commands.DeleteEspacio;

public class DeleteEspacioValidator : AbstractValidator<DeleteEspacioCommand>
{
    public DeleteEspacioValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("El ID es obligatorio.");
    }
    
}