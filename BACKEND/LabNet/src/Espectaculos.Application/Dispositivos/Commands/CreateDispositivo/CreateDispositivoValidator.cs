using FluentValidation;

namespace Espectaculos.Application.Dispositivos.Commands.CreateDispositivo
{
    public class CreateDispositivoValidator : AbstractValidator<CreateDispositivoCommand>
    {
        public CreateDispositivoValidator()
        {
            RuleFor(x => x.BiometriaHabilitada)
                .NotNull()
                .WithMessage("El atributo BiometriaHabilitada es obligatorio.");

            RuleFor(x => x.Plataforma)
                .IsInEnum()
                .WithMessage("Plataforma inválida.");

            RuleFor(x => x.Estado)
                .IsInEnum()
                .WithMessage("Estado inválido.");
            
            RuleFor(x => x.UsuarioId)
                .NotNull()
                .WithMessage("El atributo UsuarioId es obligatorio.");
        }
    }
}