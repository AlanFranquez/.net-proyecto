using FluentValidation;

namespace Espectaculos.Application.Usuarios.Commands.UpdateUsuario;

public class UpdateUsuarioValidator : AbstractValidator<UpdateUsuarioCommand>
{
    public UpdateUsuarioValidator()
    {
        RuleFor(x => x.UsuarioId)
            .NotEmpty()
            .WithMessage("El ID es obligatorio.");
    }
    
}