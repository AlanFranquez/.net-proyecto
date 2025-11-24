using FluentValidation;

namespace Espectaculos.Application.EventoAcceso.Commands.UpdateEvento;

public class UpdateEventoValidator : AbstractValidator<UpdateEventoCommand>
{
    public UpdateEventoValidator()
    {
        RuleFor(x => x.EventoId)
            .NotEmpty()
            .WithMessage("El ID es obligatorio.");
    }
    
}