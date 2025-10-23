using Espectaculos.Application.Abstractions.Repositories;
using Espectaculos.Application.Usuarios.Commands.DeleteUsuario;


using MediatR;

namespace Espectaculos.Application.Usuarios.Commands.DeleteUsuario
{
    public class DeleteUsuarioHandler : IRequestHandler<DeleteUsuarioCommand>
    {
        private readonly IUsuarioRepository _repo;

        public DeleteUsuarioHandler(IUsuarioRepository repo)
        {
            _repo = repo;
        }

        public async Task Handle(DeleteUsuarioCommand request, CancellationToken ct)
        {
            await _repo.DeleteAsync(request.UsuarioId, ct);
            await _repo.SaveChangesAsync(ct);
        }
    }
}
