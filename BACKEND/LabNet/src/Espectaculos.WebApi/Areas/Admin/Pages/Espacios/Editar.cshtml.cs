using System.ComponentModel.DataAnnotations;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Espectaculos.Application.Espacios.Queries.GetEspacioById;
using Espectaculos.Application.Espacios.Commands.UpdateEspacio;
using Espectaculos.Application.ReglaDeAcceso.Queries.ListarReglasDeAcceso;
using Espectaculos.Application.Beneficios.Queries.ListBeneficios;
using Espectaculos.Domain.Enums;
using ValidationException = FluentValidation.ValidationException;

namespace Espectaculos.WebApi.Areas.Admin.Pages.Espacios;

public class EditarModel : PageModel
{
    private readonly IMediator _mediator;
    public EditarModel(IMediator mediator) => _mediator = mediator;

    [BindProperty] public VmEspacio Vm { get; set; } = new();

    public IEnumerable<SelectListItem> Tipos { get; private set; } = default!;
    public IEnumerable<SelectListItem> Modos { get; private set; } = default!;
    public IEnumerable<SelectListItem> ReglaOptions { get; private set; } = Enumerable.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> BeneficioOptions { get; private set; } = Enumerable.Empty<SelectListItem>();

    public async Task<IActionResult> OnGet(Guid id, CancellationToken ct)
    {
        await LoadCombosAsync(ct);

        var dto = await _mediator.Send(new GetEspacioByIdQuery(id), ct);
        if (dto is null)
        {
            TempData["Error"] = "Espacio no encontrado.";
            return RedirectToPage("/Espacios/Index", new { area = "Admin" });
        }

        Vm.Id      = dto.Id;
        Vm.Nombre  = dto.Nombre;
        Vm.Activo  = dto.Activo;
        Vm.Tipo    = Enum.Parse<EspacioTipo>(dto.Tipo);
        Vm.Modo    = Enum.Parse<Modo>(dto.Modo);

        Vm.ReglaIds     = dto.ReglaIds ?? new List<Guid>();
        Vm.BeneficioIds = dto.BeneficioIds ?? new List<Guid>();
        // 👇 EventoIds ya no se editan en el backoffice

        return Page();
    }

    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        await LoadCombosAsync(ct);
        if (!ModelState.IsValid) return Page();

        try
        {
            await _mediator.Send(new UpdateEspacioCommand
            {
                Id     = Vm.Id,
                Nombre = Vm.Nombre?.Trim(),
                Activo = Vm.Activo,
                Tipo   = Vm.Tipo,
                Modo   = Vm.Modo,

                // Many-to-many:
                ReglaIds      = Vm.ReglaIds,
                BeneficioIds  = Vm.BeneficioIds,
                // EventoAccesoIds = null; // se dejan para la lógica de eventos, no desde el UI
            }, ct);

            TempData["Ok"] = "Espacio actualizado.";
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

        // Reglas de acceso para el multiselect
        var reglas = await _mediator.Send(new ListarReglasQuery(), ct);
        ReglaOptions = reglas
            .Select(r => new SelectListItem
            {
                Value = r.ReglaId.ToString(),
                Text  = !string.IsNullOrWhiteSpace(r.VentanaHoraria)
                        ? $"{r.Politica} ({r.VentanaHoraria}, prio {r.Prioridad})"
                        : $"{r.Politica} (prio {r.Prioridad})"
            })
            .ToList();

        // Beneficios para el multiselect
        var beneficios = await _mediator.Send(new ListBeneficiosQuery(), ct);
        BeneficioOptions = beneficios
            .Select(b => new SelectListItem
            {
                Value = b.Id.ToString(),
                Text  = string.IsNullOrWhiteSpace(b.Nombre)
                        ? $"Beneficio {b.Id}"
                        : b.Nombre
            })
            .ToList();
    }

    private static IEnumerable<SelectListItem> GetEnumItems<TEnum>() where TEnum : Enum =>
        Enum.GetValues(typeof(TEnum)).Cast<TEnum>()
            .Select(v => new SelectListItem { Value = v.ToString(), Text = v.ToString() });

    public class VmEspacio
    {
        [Required] public Guid Id { get; set; }
        [Required, MaxLength(100)] public string? Nombre { get; set; }
        public bool Activo { get; set; }
        [Required] public EspacioTipo Tipo { get; set; }
        [Required] public Modo Modo { get; set; }
        public List<Guid> ReglaIds { get; set; } = new();
        public List<Guid> BeneficioIds { get; set; } = new();

    }
}
