using FluentValidation;

namespace Espectaculos.Application.Roles.Commands.CreateRol
{
    public class CreateRolValidator : AbstractValidator<CreateRolCommand>
    {
        public CreateRolValidator()
        {
            RuleFor(x => x.Tipo)
                .NotEmpty()
                .WithMessage("El tipo es obligatorio.")
                .MaximumLength(100);

            RuleFor(x => x.Prioridad)
                .NotNull()
                .WithMessage("El atributo Prioridad es obligatorio.");

            RuleFor(x => x.FechaAsignado)
                .NotEmpty()
                .WithMessage("El atributo FechaAsignado es obligatorio.");
        }
    }
}