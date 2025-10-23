using FluentValidation;

namespace Espectaculos.Application.EventoAcceso.Commands.DeleteEvento;

public class DeleteEventoValidator : AbstractValidator<DeleteEventoCommand>
{
    public DeleteEventoValidator()
    {
        RuleFor(x => x.EventoId)
            .NotEmpty()
            .WithMessage("El ID es obligatorio.");
    }
    
}