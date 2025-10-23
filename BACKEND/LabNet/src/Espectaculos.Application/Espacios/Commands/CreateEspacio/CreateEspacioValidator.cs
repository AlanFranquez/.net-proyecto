using FluentValidation;

namespace Espectaculos.Application.Espacios.Commands.CreateEspacio
{
    public class CreateEspacioValidator : AbstractValidator<CreateEspacioCommand>
    {
        public CreateEspacioValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty()
                .WithMessage("El nombre es obligatorio.")
                .MaximumLength(100);

            RuleFor(x => x.Tipo)
                .IsInEnum()
                .WithMessage("Tipo inválido.");

            RuleFor(x => x.Modo)
                .IsInEnum()
                .WithMessage("Modo inválido.");
        }
    }
}