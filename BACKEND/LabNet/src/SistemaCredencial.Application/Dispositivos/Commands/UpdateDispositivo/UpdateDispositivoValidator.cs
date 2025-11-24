using FluentValidation;

namespace Espectaculos.Application.Dispositivos.Commands.UpdateDispositivo;

public class UpdateDispositivoValidator : AbstractValidator<UpdateDispositivoCommand>
{
    public UpdateDispositivoValidator()
    {
        RuleFor(x => x.DispositivoId)
            .NotEmpty()
            .WithMessage("El ID es obligatorio.");
    }
    
}