using FluentValidation;

namespace Espectaculos.Application.Usuarios.Commands.UpdateUsuario;

public class UpdateUsuarioValidator : AbstractValidator<UpdateUsuarioCommand>
{
    public UpdateUsuarioValidator()
    {
        RuleFor(x => x.UsuarioId)
            .NotEmpty()
            .WithMessage("El ID es obligatorio.");
        RuleFor(x => x.Email!)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Nombre)
            .MaximumLength(100)
            .When(x => x.Nombre != null);
    }
    
}