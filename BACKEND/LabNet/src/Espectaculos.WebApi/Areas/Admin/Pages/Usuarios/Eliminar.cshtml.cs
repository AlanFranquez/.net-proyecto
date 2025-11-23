using Espectaculos.Application.Abstractions.Repositories;
using Espectaculos.Application.Usuarios.Commands.DeleteUsuario;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Espectaculos.WebApi.Areas.Admin.Pages.Usuarios
{
    public class EliminarModel : PageModel
    {
        private readonly IUsuarioRepository _repo;
        private readonly IMediator _mediator;

        public EliminarModel(IUsuarioRepository repo, IMediator mediator)
        {
            _repo = repo;
            _mediator = mediator;
        }

        public Guid UsuarioId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Documento { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var u = await _repo.GetByIdAsync(id);
            if (u is null) { TempData["Error"] = "Usuario no encontrado."; return RedirectToPage("/Usuarios/Index"); }

            UsuarioId = u.UsuarioId;
            NombreCompleto = $"{u.Nombre} {u.Apellido}";
            Email = u.Email;
            Documento = u.Documento;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid id)
        {
            try
            {
                await _mediator.Send(new DeleteUsuarioCommand { UsuarioId = id });
                TempData["Ok"] = "Usuario eliminado.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToPage("/Usuarios/Index");
        }
    }
}