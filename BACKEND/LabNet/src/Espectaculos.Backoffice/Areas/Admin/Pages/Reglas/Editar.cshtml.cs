using System.Linq;
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
    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var regla = (await _mediator.Send(new ListarReglasQuery()))
            .FirstOrDefault(r => r.ReglaId == id);
        if (regla is null) return NotFound();
        Vm = new VmRegla
        {
            ReglaId = regla.ReglaId,
            VentanaHoraria = regla.VentanaHoraria,
            VigenciaInicio = regla.VigenciaInicio, 
            VigenciaFin = regla.VigenciaFin,
            Prioridad = regla.Prioridad,
            Politica = regla.Politica,
            RequiereBiometriaConfirmacion = regla.RequiereBiometriaConfirmacion,
            EspaciosIDsComma = (regla.EspaciosIDs is not null && regla.EspaciosIDs.Any())
                ? string.Join(", ", regla.EspaciosIDs)
                : string.Empty
        };
        Politicas = GetPoliticas();
        return Page();
    }
    public async Task<IActionResult> OnPostAsync()
    {
        Politicas = GetPoliticas();
        if (!ModelState.IsValid) return Page();

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
                var ini = new DateTime(d.Year, d.Month, d.Day, 0, 0, 0, DateTimeKind.Utc);
                vigIniUtc = ini;
            }
            var f = Vm.VigenciaFin.Value;
            var finUtc = new DateTime(f.Year, f.Month, f.Day, 23, 59, 59, DateTimeKind.Utc);
            var espacios = string.IsNullOrWhiteSpace(Vm.EspaciosIDsComma)
                ? new List<Guid>()
                : Vm.EspaciosIDsComma.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Guid.Parse(x.Trim()))
                    .ToList();
            await _mediator.Send(new UpdateReglaCommand
            {
                ReglaId = Vm.ReglaId,
                VentanaHoraria = Vm.VentanaHoraria,
                VigenciaInicio = vigIniUtc,
                VigenciaFin = finUtc,
                Prioridad = Vm.Prioridad,
                Politica = Vm.Politica,
                RequiereBiometriaConfirmacion = Vm.RequiereBiometriaConfirmacion,
                EspaciosIDs = espacios
            });
            TempData["Ok"] = "Regla actualizada.";
            return RedirectToPage("/Reglas/Index");
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
    private static IEnumerable<SelectListItem> GetPoliticas() =>
        Enum.GetValues(typeof(AccesoTipo)).Cast<AccesoTipo>()
            .Select(v => new SelectListItem { Value = v.ToString(), Text = v.ToString() });
    public class VmRegla
    {
        public Guid ReglaId { get; set; }
        [BindProperty] public string? VentanaHoraria { get; set; }
        [BindProperty] public DateTime? VigenciaInicio { get; set; } 
        [BindProperty] public DateTime? VigenciaFin { get; set; } 
        [BindProperty] public int Prioridad { get; set; }
        [BindProperty] public AccesoTipo Politica { get; set; }
        [BindProperty] public bool RequiereBiometriaConfirmacion { get; set; }
        [BindProperty] public string? EspaciosIDsComma { get; set; }
    }
    
}
