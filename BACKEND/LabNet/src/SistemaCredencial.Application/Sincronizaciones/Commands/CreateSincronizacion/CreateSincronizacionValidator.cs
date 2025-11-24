using FluentValidation;

namespace Espectaculos.Application.Sincronizaciones.Commands.CreateSincronizacion
{
    public class CreateSincronizacionValidator : AbstractValidator<CreateSincronizacionCommand>
    {
        public CreateSincronizacionValidator()
        {
            RuleFor(x => x.CreadoEn)
                .NotEmpty()
                .WithMessage("El atributo CreadoEn es obligatorio.");

            RuleFor(x => x.CantidadItems)
                .NotNull()
                .WithMessage("El atributo CantidadItems es obligatorio.");
            
            RuleFor(x => x.DispositivoId)
                .NotEmpty()
                .WithMessage("El DispositivoId es obligatorio.");
            
        }
    }
}