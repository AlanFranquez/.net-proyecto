using System.ComponentModel.DataAnnotations;
using Espectaculos.Application.Usuarios.Commands.CreateUsuario;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Espectaculos.Backoffice.Areas.Admin.Pages.Usuarios
{
    public class CrearModel : PageModel
    {
        private readonly IMediator _mediator;
        public CrearModel(IMediator mediator) => _mediator = mediator;

        [BindProperty] public Vm ModelVm { get; set; } = new();

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            try
            {
                await _mediator.Send(new CreateUsuarioCommand
                {
                    Nombre    = ModelVm.Nombre!.Trim(),
                    Apellido  = ModelVm.Apellido!.Trim(),
                    Email     = ModelVm.Email!.Trim(),
                    Documento = ModelVm.Documento!.Trim(),
                    Password  = ModelVm.Password!.Trim(),
                    RolesIDs  = ParseGuids(ModelVm.RolesComma)
                });

                TempData["Ok"] = "Usuario creado.";
                return RedirectToPage("/Usuarios/Index");
            }
            catch (FluentValidation.ValidationException vex)
            {
                foreach (var failure in vex.Errors)
                    ModelState.AddModelError(failure.PropertyName ?? string.Empty, failure.ErrorMessage);

                return Page();
            }
            catch (Exception ex)
            {
                // opcional: loguea completo para ver stack y constraint exacta
                // _logger.LogError(ex, "Error creando usuario");
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
        }
        private static IEnumerable<Guid>? ParseGuids(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            var list = new List<Guid>();
            foreach (var s in raw.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                if (Guid.TryParse(s, out var g)) list.Add(g);
            return list.Count > 0 ? list : null;
        }

        public class Vm
        {
            [Required, MaxLength(100)] public string? Nombre { get; set; }
            [Required] public string? Apellido { get; set; }
            [Required, EmailAddress] public string? Email { get; set; }
            [Required] public string? Documento { get; set; }
            [Required, MinLength(6)] public string? Password { get; set; }
            public string? RolesComma { get; set; }
        }
    }
}
