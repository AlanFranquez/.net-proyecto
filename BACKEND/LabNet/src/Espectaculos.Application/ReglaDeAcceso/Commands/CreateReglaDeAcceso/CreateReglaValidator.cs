using FluentValidation;

namespace Espectaculos.Application.ReglaDeAcceso.Commands.CreateReglaDeAcceso
{
    public class CreateReglaValidator : AbstractValidator<CreateReglaCommand>
    {
        public CreateReglaValidator()
        {
            RuleFor(x => x.VentanaHoraria)
                .NotEmpty()
                .WithMessage("El atributo VentanaHoraria es obligatorio.")
                .MaximumLength(100);
            
            RuleFor(x => x.VigenciaInicio)
                .NotEmpty()
                .WithMessage("El atributo VigenciaInicio es obligatorio.");
            
            RuleFor(x => x.VigenciaFin)
                .NotEmpty()
                .WithMessage("El atributo VigenciaFin es obligatorio.");

            RuleFor(x => x.Prioridad).NotNull()
                .WithMessage("El atributo Prioridad es obligatorio.");

            RuleFor(x => x.RequiereBiometriaConfirmacion)
                .NotEmpty()
                .WithMessage("El atributo RequiereBiometriaConfirmacion es obligatorio.");


            RuleFor(x => x.Politica)
                .IsInEnum()
                .WithMessage("Tipo inválido.");
        }
    }
}