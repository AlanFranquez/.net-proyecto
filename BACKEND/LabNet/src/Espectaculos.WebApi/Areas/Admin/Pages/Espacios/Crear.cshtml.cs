using System.ComponentModel.DataAnnotations;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Espectaculos.Domain.Enums;
using Espectaculos.Application.Espacios.Commands.CreateEspacio;
using Espectaculos.Application.ReglaDeAcceso.Queries.ListarReglasDeAcceso;
using Espectaculos.Application.Beneficios.Queries.ListBeneficios;
using ValidationException = FluentValidation.ValidationException;

namespace Espectaculos.WebApi.Areas.Admin.Pages.Espacios;

public class CrearModel : PageModel
{
    private readonly IMediator _mediator;
    public CrearModel(IMediator mediator) => _mediator = mediator;

    [BindProperty] public VmEspacio Vm { get; set; } = new();

    public IEnumerable<SelectListItem> Tipos { get; private set; } = default!;
    public IEnumerable<SelectListItem> Modos { get; private set; } = default!;
    public IEnumerable<SelectListItem> ReglaOptions { get; private set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> BeneficioOptions { get; private set; } = new List<SelectListItem>();

    public async Task OnGetAsync(CancellationToken ct)
    {
        await LoadCombosAsync(ct);
    }

    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        await LoadCombosAsync(ct);

        if (!ModelState.IsValid) return Page();

        try
        {
            await _mediator.Send(new CreateEspacioCommand
            {
                Nombre = Vm.Nombre!.Trim(),
                Activo = Vm.Activo,
                Tipo   = Vm.Tipo,
                Modo   = Vm.Modo,

                ReglaIds     = Vm.ReglaIds,
                BeneficioIds = Vm.BeneficioIds
            }, ct);

            TempData["Ok"] = "Espacio creado.";
            return RedirectToPage("/Espacios/Index", new { area = "Admin" });
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
        Tipos = GetEnumItems<EspacioTipo>();
        Modos = GetEnumItems<Modo>();

        var reglas = await _mediator.Send(new ListarReglasQuery(), ct);
        ReglaOptions = reglas
            .Select(r => new SelectListItem
            {
                Value = r.ReglaId.ToString(),
                Text  = !string.IsNullOrEmpty(r.VentanaHoraria)
                        ? $"{r.Politica} ({r.VentanaHoraria}, prio {r.Prioridad})"
                        : $"{r.Politica} (prio {r.Prioridad})"
            })
            .ToList();

        var beneficios = await _mediator.Send(new ListBeneficiosQuery(), ct);
        BeneficioOptions = beneficios
            .Select(b => new SelectListItem
            {
                Value = b.Id.ToString(),
                Text  = string.IsNullOrWhiteSpace(b.Nombre) ? $"Beneficio {b.Id}" : b.Nombre
            })
            .ToList();
    }

    private static IEnumerable<SelectListItem> GetEnumItems<TEnum>() where TEnum : Enum =>
        Enum.GetValues(typeof(TEnum)).Cast<TEnum>()
            .Select(v => new SelectListItem { Value = v.ToString(), Text = v.ToString() });

    public class VmEspacio
    {
        [Required, MaxLength(100)] public string? Nombre { get; set; }
        public bool Activo { get; set; } = true;
        [Required] public EspacioTipo Tipo { get; set; }
        [Required] public Modo Modo { get; set; }

        public List<Guid> ReglaIds { get; set; } = new();
        public List<Guid> BeneficioIds { get; set; } = new();
    }
}
