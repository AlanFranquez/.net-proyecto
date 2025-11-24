using FluentValidation;

namespace Espectaculos.Application.Credenciales.Commands.CreateCredencial
{
    public class CreateCredencialValidator : AbstractValidator<CreateCredencialCommand>
    {
        public CreateCredencialValidator()
        {
            RuleFor(x => x.Tipo)
                .IsInEnum()
                .WithMessage("Tipo inválido.");

            RuleFor(x => x.Estado)
                .IsInEnum()
                .WithMessage("Modo inválido.");
            
            RuleFor(x => x.FechaEmision)
                .NotEmpty()
                .WithMessage("Fecha inválida.");
            RuleFor(x => x.UsuarioId)
                .NotEmpty()
                .WithMessage("El UsuarioID es obligatorio.");
        }
    }
}