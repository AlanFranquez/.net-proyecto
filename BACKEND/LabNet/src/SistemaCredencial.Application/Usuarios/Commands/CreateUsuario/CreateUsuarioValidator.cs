using FluentValidation;

namespace Espectaculos.Application.Usuarios.Commands.CreateUsuario
{
    public class CreateUsuarioValidator : AbstractValidator<CreateUsuarioCommand>
    {
        public CreateUsuarioValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty()
                .WithMessage("El nombre es obligatorio.")
                .MaximumLength(100);

            RuleFor(x => x.Apellido)
                .NotEmpty()
                .WithMessage("El atributo Apellido es obligatorio.");
            
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("El atributo Email es obligatorio.");
            
            RuleFor(x => x.Documento)
                .NotEmpty()
                .WithMessage("El atributo Documento es obligatorio.");
            
            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("El atributo Password es obligatorio.");
        }
    }
}