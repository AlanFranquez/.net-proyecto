using FluentValidation;

namespace Espectaculos.Application.ReglaDeAcceso.Commands.DeleteReglaDeAcceso;

public class DeleteReglaValidator: AbstractValidator<DeleteReglaCommand>
    {
    public DeleteReglaValidator()
    {
        RuleFor(x => x.ReglaId)
            .NotEmpty()
            .WithMessage("El ID es obligatorio.");
    }
}