using System.ComponentModel.DataAnnotations;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Espectaculos.Domain.Enums;
using Espectaculos.Application.Espacios.Commands.CreateEspacio;
using ValidationException = FluentValidation.ValidationException;

namespace Espectaculos.Backoffice.Areas.Admin.Pages.Espacios;

public class CrearModel : PageModel
{
    private readonly IMediator _mediator;
    public CrearModel(IMediator mediator) => _mediator = mediator;

    [BindProperty] public VmEspacio Vm { get; set; } = new();

    public IEnumerable<SelectListItem> Tipos { get; private set; } = default!;
    public IEnumerable<SelectListItem> Modos { get; private set; } = default!;

    public void OnGet()
    {
        Tipos = GetEnumItems<EspacioTipo>();
        Modos = GetEnumItems<Modo>();
    }

    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OnPostAsync()
    {
        Tipos = GetEnumItems<EspacioTipo>();
        Modos = GetEnumItems<Modo>();
        if (!ModelState.IsValid) return Page();

        try
        {
            await _mediator.Send(new CreateEspacioCommand
            {
                Nombre = Vm.Nombre!.Trim(),
                Activo = Vm.Activo,
                Tipo   = Vm.Tipo,
                Modo   = Vm.Modo
            });
            TempData["Ok"] = "Espacio creado.";
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

    public class VmEspacio
    {
        [Required, MaxLength(100)] public string? Nombre { get; set; }
        public bool Activo { get; set; } = true;
        [Required] public EspacioTipo Tipo { get; set; }
        [Required] public Modo Modo { get; set; }
    }
}
