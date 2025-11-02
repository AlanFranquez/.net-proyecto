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

    public void OnGet()
    {
        Politicas = Enum.GetValues(typeof(AccesoTipo))
            .Cast<AccesoTipo>()
            .Select(v => new SelectListItem { Value = v.ToString(), Text = v.ToString() });
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            OnGet(); // recargar combos
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

            var fin = Vm.VigenciaFin;
            var vigFinUtc = new DateTime(fin.Year, fin.Month, fin.Day, 23, 59, 59, DateTimeKind.Utc);

            var espacios = string.IsNullOrWhiteSpace(Vm.EspaciosIDsComma)
                ? new List<Guid>()
                : Vm.EspaciosIDsComma.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Guid.Parse(x.Trim()))
                    .ToList();

            await _mediator.Send(new CreateReglaCommand
            {
                VentanaHoraria = Vm.VentanaHoraria!,
                VigenciaInicio = vigIniUtc,
                VigenciaFin = vigFinUtc,
                Prioridad = Vm.Prioridad,
                Politica = Vm.Politica,
                RequiereBiometriaConfirmacion = Vm.RequiereBiometriaConfirmacion,
                EspaciosIDs = espacios
            });

            TempData["Ok"] = "Regla creada.";
            return RedirectToPage("/Reglas/Index");
        }
        catch (ValidationException vex)
        {
            foreach (var e in vex.Errors)
                ModelState.AddModelError(e.PropertyName ?? string.Empty, e.ErrorMessage);

            OnGet();
            return Page();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            OnGet();
            return Page();
        }
    }

    public class VmRegla
    {
        [BindProperty] public string? VentanaHoraria { get; set; }
        [BindProperty] public DateTime? VigenciaInicio { get; set; }
        [BindProperty] public DateTime VigenciaFin { get; set; }
        [BindProperty] public int Prioridad { get; set; }
        [BindProperty] public AccesoTipo Politica { get; set; }
        [BindProperty] public bool RequiereBiometriaConfirmacion { get; set; }
        [BindProperty] public string? EspaciosIDsComma { get; set; }
    }
}
