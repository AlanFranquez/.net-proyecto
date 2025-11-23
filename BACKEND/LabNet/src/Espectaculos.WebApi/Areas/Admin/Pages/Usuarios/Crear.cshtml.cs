using System.ComponentModel.DataAnnotations;
using Espectaculos.Application.Usuarios.Commands.CreateUsuario;
using Espectaculos.Application.Roles.Queries.ListarRoles; // NEW
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Espectaculos.WebApi.Areas.Admin.Pages.Usuarios;

public class CrearModel : PageModel
{
    private readonly IMediator _mediator;
    public CrearModel(IMediator mediator) => _mediator = mediator;

    [BindProperty] public Vm ModelVm { get; set; } = new();

    public IEnumerable<SelectListItem> RolesOptions { get; set; } = Enumerable.Empty<SelectListItem>();

    public async Task OnGetAsync(CancellationToken ct)
    {
        await LoadRolesAsync(ct);
    }

    private async Task LoadRolesAsync(CancellationToken ct)
    {
        var roles = await _mediator.Send(new ListarRolesQuery(), ct);

        RolesOptions = roles.Select(r => new SelectListItem
        {
            Value = r.RolId.ToString(),
            Text = $"{r.Tipo} (prio {r.Prioridad})"
        })
        .ToList();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        await LoadRolesAsync(ct);
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
                RolesIDs  = ModelVm.RolesIDs ?? new List<Guid>()
            }, ct);

            TempData["Ok"] = "Usuario creado.";
            return RedirectToPage("/Usuarios/Index", new { area = "Admin" });
        }
        catch (FluentValidation.ValidationException vex)
        {
            foreach (var e in vex.Errors)
                ModelState.AddModelError(e.PropertyName ?? string.Empty, e.ErrorMessage);

            return Page();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }

    public class Vm
    {
        [Required, MaxLength(100)] public string? Nombre { get; set; }
        [Required] public string? Apellido { get; set; }
        [Required, EmailAddress] public string? Email { get; set; }
        [Required] public string? Documento { get; set; }
        [Required, MinLength(6)] public string? Password { get; set; }

        // NEW - no more comma text!
        public List<Guid>? RolesIDs { get; set; } = new();
    }
}
