using FluentValidation;

namespace Espectaculos.Application.Credenciales.Commands.DeleteCredencial;

public class DeleteCredencialValidator : AbstractValidator<DeleteCredencialCommand>
{
    public DeleteCredencialValidator()
    {
        RuleFor(x => x.CredencialId)
            .NotEmpty()
            .WithMessage("El ID es obligatorio.");
    }
}