using Espectaculos.Application.Espacios.Queries.ListarEspacios;
using Espectaculos.Application.ReglaDeAcceso.Commands.CreateReglaDeAcceso;
using Espectaculos.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Espectaculos.Backoffice.Areas.Admin.Pages.Reglas;

public class CrearModel : PageModel
{
    private readonly IMediator _mediator;
    public CrearModel(IMediator mediator) => _mediator = mediator;

    [BindProperty] public VmRegla Vm { get; set; } = new();

    public IEnumerable<SelectListItem> Politicas { get; set; } = Enumerable.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> EspaciosOptions { get; set; } = Enumerable.Empty<SelectListItem>();

    public async Task OnGetAsync(CancellationToken ct)
    {
        await LoadCombosAsync(ct);
    }

    private async Task LoadCombosAsync(CancellationToken ct)
    {
        // Combo de políticas (enum)
        Politicas = Enum.GetValues(typeof(AccesoTipo))
            .Cast<AccesoTipo>()
            .Select(v => new SelectListItem { Value = v.ToString(), Text = v.ToString() })
            .ToList();

        // Combo / multiselect de espacios desde la BD
        var espacios = await _mediator.Send(new ListarEspaciosQuery(), ct);
        EspaciosOptions = espacios
            .Select(e => new SelectListItem
            {
                Value = e.Id.ToString(),
                Text = e.Nombre
            })
            .ToList();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await LoadCombosAsync(ct);
            return Page();
        }

        try
        {
            DateTime? vigIniUtc = null;
            if (Vm.VigenciaInicio.HasValue)
            {
                var d = Vm.VigenciaInicio.Value;
                vigIniUtc = new DateTime(d.Year, d.Month, d.Day, 0, 0, 0, DateTimeKind.Utc);
            }

            DateTime? vigFinUtc = null;
            if (Vm.VigenciaFin.HasValue)
            {
                var d = Vm.VigenciaFin.Value;
                vigFinUtc = new DateTime(d.Year, d.Month, d.Day, 23, 59, 59, DateTimeKind.Utc);
            }

            await _mediator.Send(new CreateReglaCommand
            {
                VentanaHoraria = Vm.VentanaHoraria!,
                VigenciaInicio = vigIniUtc,
                VigenciaFin = vigFinUtc,
                Prioridad = Vm.Prioridad,
                Politica = Vm.Politica,
                Rol = Vm.Rol,
                RequiereBiometriaConfirmacion = Vm.RequiereBiometriaConfirmacion,
                EspaciosIDs = Vm.EspaciosIDs ?? new List<Guid>()
            }, ct);

            TempData["Ok"] = "Regla creada.";
            return RedirectToPage("/Reglas/Index", new { area = "Admin" });
        }
        catch (ValidationException vex)
        {
            foreach (var e in vex.Errors)
                ModelState.AddModelError(e.PropertyName ?? string.Empty, e.ErrorMessage);

            await LoadCombosAsync(ct);
            return Page();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadCombosAsync(ct);
            return Page();
        }
    }

    public class VmRegla
    {
        [BindProperty] public string? VentanaHoraria { get; set; }
        [BindProperty] public DateTime? VigenciaInicio { get; set; }
        [BindProperty] public DateTime? VigenciaFin { get; set; }
        [BindProperty] public int Prioridad { get; set; }
        [BindProperty] public AccesoTipo Politica { get; set; }
        [BindProperty] public bool RequiereBiometriaConfirmacion { get; set; }
        [BindProperty] public string? Rol { get; set; }

        // Lista de GUIDs para los espacios seleccionados
        [BindProperty] public List<Guid>? EspaciosIDs { get; set; } = new();
    }
}
