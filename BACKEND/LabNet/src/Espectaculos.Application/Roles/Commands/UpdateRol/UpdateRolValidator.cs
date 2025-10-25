using FluentValidation;

namespace Espectaculos.Application.Roles.Commands.UpdateRol;

public class UpdateRolValidator : AbstractValidator<UpdateRolCommand>
{
    public UpdateRolValidator()
    {
        RuleFor(x => x.RolId)
            .NotEmpty()
            .WithMessage("El ID es obligatorio.");
    }
}