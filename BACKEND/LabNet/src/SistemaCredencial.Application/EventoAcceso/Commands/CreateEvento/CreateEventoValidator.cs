using FluentValidation;

namespace Espectaculos.Application.EventoAcceso.Commands.CreateEvento
{
    public class CreateEventoValidator : AbstractValidator<CreateEventoCommand>
    {
        public CreateEventoValidator()
        {
            RuleFor(x => x.MomentoDeAcceso)
                .NotEmpty()
                .WithMessage("El MomentoDeAcceso es obligatorio.");

            RuleFor(x => x.CredencialId)
                .NotEmpty()
                .WithMessage("El CredencialId es obligatorio.");
            
            RuleFor(x => x.EspacioId)
                .NotEmpty()
                .WithMessage("El EspacioId es obligatorio.");

            RuleFor(x => x.Resultado)
                .IsInEnum()
                .WithMessage("Resultado inválido.");
            
            RuleFor(x => x.Modo)
                .IsInEnum()
                .WithMessage("Modo inválido.");
        }
    }
}