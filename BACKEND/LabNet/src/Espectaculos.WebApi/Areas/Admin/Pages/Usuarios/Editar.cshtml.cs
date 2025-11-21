// File: Espectaculos.Backoffice/Areas/Admin/Pages/Usuarios/Editar.cshtml.cs
using System.ComponentModel.DataAnnotations;
using Espectaculos.Application.Abstractions.Repositories;
using Espectaculos.Application.Usuarios.Commands.UpdateUsuario;
using Espectaculos.Application.Roles.Queries.ListarRoles;
using Espectaculos.Application.Beneficios.Queries.ListBeneficios;
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

    [BindProperty] 
    public Vm ModelVm { get; set; } = new();

    public IEnumerable<SelectListItem> RolesOptions { get; set; } = Enumerable.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> BeneficiosOptions { get; set; } = Enumerable.Empty<SelectListItem>();

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
            Nombre    = u.Nombre,
            Apellido  = u.Apellido,
            Email     = u.Email,
            Documento = u.Documento,
            Estado    = u.Estado,
            RolesIDs  = u.UsuarioRoles?.Select(ur => ur.RolId).ToList() ?? new(),

            // Navegación: Usuario.Beneficios : ICollection<BeneficioUsuario>
            BeneficiosIDs = u.Beneficios?.Select(bu => bu.BeneficioId).ToList() ?? new()
        };

        await LoadRolesAsync(ct);
        await LoadBeneficiosAsync(ct);

        return Page();
    }

    private async Task LoadRolesAsync(CancellationToken ct)
    {
        var roles = await _mediator.Send(new ListarRolesQuery(), ct);
        var seleccionados = ModelVm?.RolesIDs?.ToHashSet() ?? new HashSet<Guid>();

        RolesOptions = roles
            .Select(r => new SelectListItem
            {
                Value    = r.RolId.ToString(),
                Text     = $"{r.Tipo} (prio {r.Prioridad})",
                Selected = seleccionados.Contains(r.RolId)
            })
            .ToList();
    }

    private async Task LoadBeneficiosAsync(CancellationToken ct)
    {
        // ListBeneficiosQuery devuelve IReadOnlyList<BeneficioDTO>
        var beneficios = await _mediator.Send(new ListBeneficiosQuery(), ct);
        var seleccionados = ModelVm?.BeneficiosIDs?.ToHashSet() ?? new HashSet<Guid>();

        BeneficiosOptions = beneficios
            .Select(b => new SelectListItem
            {
                Value    = b.Id.ToString(),   // del BeneficioDTO
                Text     = b.Nombre,
                Selected = seleccionados.Contains(b.Id)
            })
            .ToList();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        await LoadRolesAsync(ct);
        await LoadBeneficiosAsync(ct);

        if (!ModelState.IsValid) 
            return Page();

        try
        {
            await _mediator.Send(new UpdateUsuarioCommand
            {
                UsuarioId     = ModelVm.UsuarioId,
                Nombre        = ModelVm.Nombre?.Trim(),
                Apellido      = ModelVm.Apellido?.Trim(),
                Email         = ModelVm.Email?.Trim(),
                Documento     = ModelVm.Documento?.Trim(),
                Password      = string.IsNullOrWhiteSpace(ModelVm.Password) ? null : ModelVm.Password!.Trim(),
                Estado        = ModelVm.Estado,
                RolesIDs      = ModelVm.RolesIDs ?? new List<Guid>(),
                BeneficiosIDs = ModelVm.BeneficiosIDs ?? new List<Guid>()
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

        public List<Guid>? RolesIDs { get; set; } = new();

        public List<Guid>? BeneficiosIDs { get; set; } = new();
    }
}
