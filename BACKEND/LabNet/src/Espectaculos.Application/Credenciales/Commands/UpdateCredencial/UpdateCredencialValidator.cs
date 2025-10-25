using FluentValidation;

namespace Espectaculos.Application.Credenciales.Commands.UpdateCredencial;

public class UpdateCredencialValidator : AbstractValidator<UpdateCredencialCommand>
{
    public UpdateCredencialValidator()
    {
        RuleFor(x => x.CredencialId)
            .NotEmpty()
            .WithMessage("El ID es obligatorio.");
    }
}