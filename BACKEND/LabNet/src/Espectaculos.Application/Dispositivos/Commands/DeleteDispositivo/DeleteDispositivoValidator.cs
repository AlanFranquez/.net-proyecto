using FluentValidation;

namespace Espectaculos.Application.Dispositivos.Commands.DeleteDispositivo;

public class DeleteDispositivoValidator : AbstractValidator<DeleteDispositivoCommand>
{
    public DeleteDispositivoValidator()
    {
        RuleFor(x => x.DispositivoId)
            .NotEmpty()
            .WithMessage("El ID es obligatorio.");
    }
    
}