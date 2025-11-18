using System.ComponentModel.DataAnnotations;
using Espectaculos.Application.Abstractions.Repositories;
using Espectaculos.Application.Usuarios.Commands.UpdateUsuario;
using Espectaculos.Application.Roles.Queries.ListarRoles; 
using Espectaculos.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Espectaculos.Backoffice.Areas.Admin.Pages.Usuarios;

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

    public IEnumerable<SelectListItem> RolesOptions { get; set; } = Enumerable.Empty<SelectListItem>();

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct)
    {
        var u = await _repo.GetByIdAsync(id, ct);
        if (u is null) 
        { 
            TempData["Error"] = "Usuario no encontrado."; 
            return RedirectToPage("/Usuarios/Index", new { area = "Admin" }); 
        }

        ModelVm = new Vm
        {
            UsuarioId = u.UsuarioId,
            Nombre = u.Nombre,
            Apellido = u.Apellido,
            Email = u.Email,
            Documento = u.Documento,
            Estado = u.Estado,
            RolesIDs = u.UsuarioRoles?.Select(ur => ur.RolId).ToList() ?? new()
        };

        await LoadRolesAsync(ct);
        return Page();
    }

    private async Task LoadRolesAsync(CancellationToken ct)
    {
        var roles = await _mediator.Send(new ListarRolesQuery(), ct);

        RolesOptions = roles
            .Select(r => new SelectListItem
            {
                Value = r.RolId.ToString(),
                Text = $"{r.Tipo} (prio {r.Prioridad})"
            })
            .ToList();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        await LoadRolesAsync(ct);

        if (!ModelState.IsValid) 
            return Page();

        try
        {
            await _mediator.Send(new UpdateUsuarioCommand
            {
                UsuarioId = ModelVm.UsuarioId,
                Nombre     = ModelVm.Nombre?.Trim(),
                Apellido   = ModelVm.Apellido?.Trim(),
                Email      = ModelVm.Email?.Trim(),
                Documento  = ModelVm.Documento?.Trim(),
                Password   = string.IsNullOrWhiteSpace(ModelVm.Password) ? null : ModelVm.Password!.Trim(),
                Estado     = ModelVm.Estado,
                RolesIDs   = ModelVm.RolesIDs ?? new List<Guid>()
            }, ct);

            TempData["Ok"] = "Usuario actualizado.";
            return RedirectToPage("/Usuarios/Index", new { area = "Admin" });
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

        // NEW
        public List<Guid>? RolesIDs { get; set; } = new();
    }
}
