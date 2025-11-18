using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Espectaculos.Application.Roles.Commands.CreateRol;
using System.ComponentModel.DataAnnotations;
using ValidationException = FluentValidation.ValidationException;
namespace Espectaculos.Backoffice.Areas.Admin.Pages.Roles;

public class CrearModel : PageModel
{
    private readonly IMediator _mediator;
    public CrearModel(IMediator mediator) => _mediator = mediator;
    [BindProperty] public VmRole Vm { get; set; } = new();
    public void OnGet() { }
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        try
        {
            var tipo = Vm.Tipo?.Trim();
            var fechaUtc = new DateTime(
                Vm.FechaAsignado.Year,
                Vm.FechaAsignado.Month,
                Vm.FechaAsignado.Day,
                0, 0, 0,
                DateTimeKind.Utc
            );
            await _mediator.Send(new CreateRolCommand
            {
                Tipo          = tipo!,
                Prioridad     = Vm.Prioridad,
                FechaAsignado = fechaUtc
            });
            TempData["Ok"] = "Rol creado.";
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
    public class VmRole
    {
        [Required, MaxLength(100)]
        public string? Tipo { get; set; }
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Prioridad debe ser un número positivo.")] 
        public int Prioridad { get; set; }
        [Required]
        public DateOnly FechaAsignado { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    }
}
