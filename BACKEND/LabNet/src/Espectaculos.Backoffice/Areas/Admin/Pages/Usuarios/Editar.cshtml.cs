using System.ComponentModel.DataAnnotations;
using Espectaculos.Application.Abstractions.Repositories;
using Espectaculos.Application.Usuarios.Commands.UpdateUsuario;
using Espectaculos.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Espectaculos.Backoffice.Areas.Admin.Pages.Usuarios
{
    public class EditarModel : PageModel
    {
        private readonly IUsuarioRepository _repo;
        private readonly IMediator _mediator;

        public EditarModel(IUsuarioRepository repo, IMediator mediator)
        {
            _repo = repo;
            _mediator = mediator;
        }

        [BindProperty] public Vm ModelVm { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var u = await _repo.GetByIdAsync(id);
            if (u is null) { TempData["Error"] = "Usuario no encontrado."; return RedirectToPage("/Usuarios/Index"); }

            ModelVm = new Vm
            {
                UsuarioId = u.UsuarioId,
                Nombre = u.Nombre,
                Apellido = u.Apellido,
                Email = u.Email,
                Documento = u.Documento,
                Estado = u.Estado
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            try
            {
                await _mediator.Send(new UpdateUsuarioCommand
                {
                    UsuarioId = ModelVm.UsuarioId,
                    Nombre = ModelVm.Nombre?.Trim(),
                    Apellido = ModelVm.Apellido?.Trim(),
                    Email = ModelVm.Email?.Trim(),
                    Documento = ModelVm.Documento?.Trim(),
                    Password = string.IsNullOrWhiteSpace(ModelVm.Password) ? null : ModelVm.Password!.Trim(),
                    Estado = ModelVm.Estado
                });

                TempData["Ok"] = "Usuario actualizado.";
                return RedirectToPage("/Usuarios/Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
        }

        public class Vm
        {
            [Required] public Guid UsuarioId { get; set; }
            [Required, MaxLength(100)] public string? Nombre { get; set; }
            [Required] public string? Apellido { get; set; }
            [Required, EmailAddress] public string? Email { get; set; }
            [Required] public string? Documento { get; set; }
            public string? Password { get; set; }
            public UsuarioEstado? Estado { get; set; }
        }
    }
}
