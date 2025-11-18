using System.Linq;
using Espectaculos.Application.Espacios.Queries.ListarEspacios;
using Espectaculos.Application.ReglaDeAcceso.Commands.UpdateReglaDeAcceso;
using Espectaculos.Application.ReglaDeAcceso.Queries.ListarReglasDeAcceso;
using Espectaculos.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Espectaculos.Backoffice.Areas.Admin.Pages.Reglas;

public class EditarModel : PageModel
{
    private readonly IMediator _mediator;
    public EditarModel(IMediator mediator) => _mediator = mediator;

    [BindProperty] public VmRegla Vm { get; set; } = new();

    public IEnumerable<SelectListItem> Politicas { get; set; } = Enumerable.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> EspaciosOptions { get; set; } = Enumerable.Empty<SelectListItem>();

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct)
    {
        var regla = (await _mediator.Send(new ListarReglasQuery(), ct))
            .FirstOrDefault(r => r.ReglaId == id);

        if (regla is null)
            return NotFound();

        Vm = new VmRegla
        {
            ReglaId = regla.ReglaId,
            VentanaHoraria = regla.VentanaHoraria,
            VigenciaInicio = regla.VigenciaInicio,
            VigenciaFin = regla.VigenciaFin,
            Prioridad = regla.Prioridad,
            Politica = regla.Politica,
            RequiereBiometriaConfirmacion = regla.RequiereBiometriaConfirmacion,
            EspaciosIDs = regla.EspaciosIDs?.ToList() ?? new List<Guid>()
        };

        await LoadCombosAsync(ct);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        await LoadCombosAsync(ct);

        if (!ModelState.IsValid)
            return Page();

        try
        {
            if (!Vm.VigenciaFin.HasValue)
            {
                ModelState.AddModelError(nameof(Vm.VigenciaFin), "Vigencia fin es obligatoria.");
                return Page();
            }

            DateTime? vigIniUtc = null;
            if (Vm.VigenciaInicio.HasValue)
            {
                var d = Vm.VigenciaInicio.Value;
                vigIniUtc = new DateTime(d.Year, d.Month, d.Day, 0, 0, 0, DateTimeKind.Utc);
            }

            var f = Vm.VigenciaFin.Value;
            var finUtc = new DateTime(f.Year, f.Month, f.Day, 23, 59, 59, DateTimeKind.Utc);

            await _mediator.Send(new UpdateReglaCommand
            {
                ReglaId = Vm.ReglaId,
                VentanaHoraria = Vm.VentanaHoraria,
                VigenciaInicio = vigIniUtc,
                VigenciaFin = finUtc,
                Prioridad = Vm.Prioridad,
                Politica = Vm.Politica,
                RequiereBiometriaConfirmacion = Vm.RequiereBiometriaConfirmacion,
                EspaciosIDs = Vm.EspaciosIDs ?? new List<Guid>()
            }, ct);

            TempData["Ok"] = "Regla actualizada.";
            return RedirectToPage("/Reglas/Index", new { area = "Admin" });
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

    private async Task LoadCombosAsync(CancellationToken ct)
    {
        Politicas = Enum.GetValues(typeof(AccesoTipo))
            .Cast<AccesoTipo>()
            .Select(v => new SelectListItem { Value = v.ToString(), Text = v.ToString() })
            .ToList();

        var espacios = await _mediator.Send(new ListarEspaciosQuery(), ct);
        EspaciosOptions = espacios
            .Select(e => new SelectListItem
            {
                Value = e.Id.ToString(),
                Text = e.Nombre
            })
            .ToList();
    }

    public class VmRegla
    {
        public Guid ReglaId { get; set; }

        [BindProperty] public string? VentanaHoraria { get; set; }
        [BindProperty] public DateTime? VigenciaInicio { get; set; }
        [BindProperty] public DateTime? VigenciaFin { get; set; }
        [BindProperty] public int Prioridad { get; set; }
        [BindProperty] public AccesoTipo Politica { get; set; }
        [BindProperty] public bool RequiereBiometriaConfirmacion { get; set; }

        // Lista de GUIDs de espacios seleccionados
        [BindProperty] public List<Guid>? EspaciosIDs { get; set; } = new();
    }
}
