using FluentValidation;

namespace Espectaculos.Application.Usuarios.Commands.DeleteUsuario;

public class DeleteUsuarioValidator : AbstractValidator<DeleteUsuarioCommand>
{
    public DeleteUsuarioValidator()
    {
        RuleFor(x => x.UsuarioId)
            .NotEmpty()
            .WithMessage("El ID es obligatorio.");
    }
    
}