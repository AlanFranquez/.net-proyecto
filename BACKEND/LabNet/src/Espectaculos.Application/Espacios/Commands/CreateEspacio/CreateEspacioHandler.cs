using Espectaculos.Application.Abstractions;
using Espectaculos.Domain.Entities;
using FluentValidation;
using MediatR;

namespace Espectaculos.Application.Espacios.Commands.CreateEspacio
{
    public class CreateEspacioHandler : IRequestHandler<CreateEspacioCommand, Guid>
    {
        private readonly IUnitOfWork _uow;
        private readonly IValidator<CreateEspacioCommand> _validator;

        public CreateEspacioHandler(IUnitOfWork uow, IValidator<CreateEspacioCommand> validator)
        {
            _uow = uow;
            _validator = validator;
        }

        public async Task<Guid> Handle(CreateEspacioCommand command, CancellationToken ct)
        {
            await _validator.ValidateAndThrowAsync(command, ct);

            var e = new Espacio
            {
                Id = Guid.NewGuid(),
                Nombre = command.Nombre.Trim(),
                Activo = command.Activo,
                Tipo = command.Tipo,
                Modo = command.Modo,
                EventoAccesos = new List<Domain.Entities.EventoAcceso>(),
                Reglas = new List<EspacioReglaDeAcceso>(),
                Beneficios = new List<BeneficioEspacio>()
            };

            await _uow.Espacios.AddAsync(e, ct);
            await _uow.SaveChangesAsync(ct);

            return e.Id;
        }
    }
}