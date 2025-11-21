using System.ComponentModel.DataAnnotations;
using System.Linq;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Espectaculos.Application.Roles.Commands.UpdateRol;
using Espectaculos.Application.Roles.Queries.GetRolById;
using Espectaculos.Application.Usuarios.Queries.ListarUsuarios;
using ValidationException = FluentValidation.ValidationException;

namespace Espectaculos.WebApi.Areas.Admin.Pages.Roles;

public class EditarModel : PageModel
{
    private readonly IMediator _mediator;
    public EditarModel(IMediator mediator) => _mediator = mediator;

    [BindProperty] public VmRole Vm { get; set; } = new();

    public IEnumerable<SelectListItem> UsuarioOptions { get; set; } = Enumerable.Empty<SelectListItem>();

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct)
    {
        var dto = await _mediator.Send(new GetRolByIdQuery(id), ct);
        if (dto is null) return RedirectToPage("/Roles/Index", new { area = "Admin" });

        Vm.RolId = dto.RolId;
        Vm.Tipo = dto.Tipo;
        Vm.Prioridad = dto.Prioridad;

        if (dto.FechaAsignado.HasValue)
        {
            var dt = dto.FechaAsignado.Value;
            if (dt.Kind == DateTimeKind.Unspecified)
                dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);

            Vm.FechaAsignado = DateOnly.FromDateTime(dt.ToLocalTime());
        }

        Vm.UsuariosIDs = dto.UsuariosIDs?.ToList() ?? new List<Guid>();

        await LoadUsuarioOptionsAsync(ct);
        return Page();
    }

    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        await LoadUsuarioOptionsAsync(ct);

        if (!ModelState.IsValid) 
            return Page();

        try
        {
            DateTime? fechaUtc = null;
            if (Vm.FechaAsignado.HasValue)
            {
                var d = Vm.FechaAsignado.Value;
                fechaUtc = new DateTime(d.Year, d.Month, d.Day, 0, 0, 0, DateTimeKind.Utc);
            }

            await _mediator.Send(new UpdateRolCommand
            {
                RolId         = Vm.RolId,
                Tipo          = Vm.Tipo?.Trim(),
                Prioridad     = Vm.Prioridad,
                FechaAsignado = fechaUtc,
                UsuariosIDs   = Vm.UsuariosIDs ?? new List<Guid>()
            }, ct);

            TempData["Ok"] = "Rol actualizado.";
            return RedirectToPage("/Roles/Index", new { area = "Admin" });
        }
        catch (ValidationException vex)
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

    private async Task LoadUsuarioOptionsAsync(CancellationToken ct)
    {
        var usuarios = await _mediator.Send(new ListarUsuariosQuery(), ct);

        UsuarioOptions = usuarios
            .Select(u => new SelectListItem
            {
                Value = u.UsuarioId.ToString(),
                Text  = $"{u.Nombre} {u.Apellido} ({u.Email})"
            })
            .ToList();
    }

    public class VmRole
    {
        [Required] public Guid RolId { get; set; }

        [Required, MaxLength(100)]
        public string? Tipo { get; set; }

        [Required]
        public int? Prioridad { get; set; }

        [Required]
        public DateOnly? FechaAsignado { get; set; }

        public List<Guid>? UsuariosIDs { get; set; } = new();
    }
}
