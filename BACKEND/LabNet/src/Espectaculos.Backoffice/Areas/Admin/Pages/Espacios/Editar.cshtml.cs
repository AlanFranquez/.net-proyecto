using System.ComponentModel.DataAnnotations;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Espectaculos.Application.Espacios.Queries.GetEspacioById;
using Espectaculos.Application.Espacios.Commands.UpdateEspacio;
using Espectaculos.Domain.Enums;
using ValidationException = FluentValidation.ValidationException;
namespace Espectaculos.Backoffice.Areas.Admin.Pages.Espacios;

public class EditarModel : PageModel
{
    private readonly IMediator _mediator;
    public EditarModel(IMediator mediator) => _mediator = mediator;

    [BindProperty] public VmEspacio Vm { get; set; } = new();

    public IEnumerable<SelectListItem> Tipos { get; private set; } = default!;
    public IEnumerable<SelectListItem> Modos { get; private set; } = default!;

    public async Task<IActionResult> OnGet(Guid id)
    {
        Tipos = GetEnumItems<EspacioTipo>();
        Modos = GetEnumItems<Modo>();

        var dto = await _mediator.Send(new GetEspacioByIdQuery(id));
        if (dto is null) return RedirectToPage("/Espacios/Index");

        Vm.Id = dto.Id;
        Vm.Nombre = dto.Nombre;
        Vm.Activo = dto.Activo;
        Vm.Tipo = Enum.Parse<EspacioTipo>(dto.Tipo);
        Vm.Modo = Enum.Parse<Modo>(dto.Modo);
        Vm.ReglaIdsComma     = dto.ReglaIds?.Count > 0     ? string.Join(", ", dto.ReglaIds) : null;
        Vm.BeneficioIdsComma = dto.BeneficioIds?.Count > 0 ? string.Join(", ", dto.BeneficioIds) : null;
        Vm.EventoIdsComma    = dto.EventoIds?.Count > 0    ? string.Join(", ", dto.EventoIds) : null;

        return Page();
    }

    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OnPostAsync()
    {
        Tipos = GetEnumItems<EspacioTipo>();
        Modos = GetEnumItems<Modo>();
        if (!ModelState.IsValid) return Page();

        try
        {
            await _mediator.Send(new UpdateEspacioCommand
            {
                Id = Vm.Id,
                Nombre = Vm.Nombre?.Trim(),
                Activo = Vm.Activo,
                Tipo = Vm.Tipo,
                Modo = Vm.Modo,
                ReglaIds = ParseGuids(Vm.ReglaIdsComma),
                BeneficioIds = ParseGuids(Vm.BeneficioIdsComma),
                EventoAccesoIds = ParseGuids(Vm.EventoIdsComma)
            });

            TempData["Ok"] = "Espacio actualizado.";
            return RedirectToPage("/Espacios/Index");
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

    private static IEnumerable<SelectListItem> GetEnumItems<TEnum>() where TEnum : Enum =>
        Enum.GetValues(typeof(TEnum)).Cast<TEnum>()
            .Select(v => new SelectListItem { Value = v.ToString(), Text = v.ToString() });

    private static IEnumerable<Guid>? ParseGuids(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var list = new List<Guid>();
        foreach (var s in raw.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            if (Guid.TryParse(s, out var g)) list.Add(g);
        return list.Count > 0 ? list : null;
    }

    public class VmEspacio
    {
        [Required] public Guid Id { get; set; }
        [Required, MaxLength(100)] public string? Nombre { get; set; }
        public bool Activo { get; set; }
        [Required] public EspacioTipo Tipo { get; set; }
        [Required] public Modo Modo { get; set; }
        public string? ReglaIdsComma { get; set; }
        public string? BeneficioIdsComma { get; set; }
        public string? EventoIdsComma { get; set; }
    }
}
