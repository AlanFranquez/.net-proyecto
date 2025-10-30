using System.ComponentModel.DataAnnotations;
using System.Linq;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Espectaculos.Application.Roles.Commands.UpdateRol;
using Espectaculos.Application.Roles.Queries.GetRolById;
using ValidationException = FluentValidation.ValidationException;

namespace Espectaculos.Backoffice.Areas.Admin.Pages.Roles;

public class EditarModel : PageModel
{
    private readonly IMediator _mediator;
    public EditarModel(IMediator mediator) => _mediator = mediator;

    [BindProperty] public VmRole Vm { get; set; } = new();

    public async Task<IActionResult> OnGet(Guid id)
    {
        var dto = await _mediator.Send(new GetRolByIdQuery(id));
        if (dto is null) return RedirectToPage("/Roles/Index");

        Vm.RolId = dto.RolId;
        Vm.Tipo = dto.Tipo;
        Vm.Prioridad = dto.Prioridad;
        // dto.FechaAsignado viene como DateTime (UTC en DB). Lo mostramos como fecha local (solo fecha):
// OnGet
        if (dto.FechaAsignado.HasValue)
        {
            var dt = dto.FechaAsignado.Value;
            if (dt.Kind == DateTimeKind.Unspecified)
                dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
            Vm.FechaAsignado = DateOnly.FromDateTime(dt.ToLocalTime());
        }        Vm.UsuariosIDsComma = (dto.UsuariosIDs != null && dto.UsuariosIDs.Any())
            ? string.Join(", ", dto.UsuariosIDs)
            : null;

        return Page();
    }

    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        try
        {
            // Convertimos DateOnly a DateTime UTC (00:00:00)
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
                UsuariosIDs   = ParseGuids(Vm.UsuariosIDsComma)
            });

            TempData["Ok"] = "Rol actualizado.";
            return RedirectToPage("/Roles/Index");
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

    private static IEnumerable<Guid>? ParseGuids(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var list = new List<Guid>();
        foreach (var s in raw.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            if (Guid.TryParse(s, out var g)) list.Add(g);
        return list.Count > 0 ? list : null;
    }

    public class VmRole
    {
        [Required] public Guid RolId { get; set; }
        [Required, MaxLength(100)] public string? Tipo { get; set; }
        [Required] public int? Prioridad { get; set; }

        // DateOnly para bind limpio desde <input type="date">
        [Required] public DateOnly? FechaAsignado { get; set; }

        public string? UsuariosIDsComma { get; set; }
    }
}
