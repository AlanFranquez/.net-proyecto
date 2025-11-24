using FluentValidation;

namespace Espectaculos.Application.Roles.Commands.DeleteRol;

public class DeleteRolValidator : AbstractValidator<DeleteRolCommand>
{
    public DeleteRolValidator()
    {
        RuleFor(x => x.RolId)
            .NotEmpty()
            .WithMessage("El ID es obligatorio.");
    }
}