using System.ComponentModel.DataAnnotations;
using Espectaculos.Application.Credenciales.Queries.ListarCredenciales;
using Espectaculos.Application.Credenciales.Commands.UpdateCredencial;
using Espectaculos.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ValidationException = FluentValidation.ValidationException;

namespace Espectaculos.WebApi.Areas.Admin.Pages.Credenciales;

public class EditarModel : PageModel
{
    private readonly IMediator _mediator;

    public EditarModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    [BindProperty] public VmCredencial Vm { get; set; } = new();

    public string UsuarioDisplay { get; set; } = "";

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct)
    {
        var lista = await _mediator.Send(new ListarCredencialesQuery(), ct);
        var dto = lista.FirstOrDefault(c => c.CredencialId == id);
        if (dto is null)
        {
            TempData["Ok"] = "La credencial no existe.";
            return RedirectToPage("/Credenciales/Index", new { area = "Admin" });
        }

        Vm.CredencialId    = dto.CredencialId;
        Vm.Tipo            = dto.Tipo   ?? CredencialTipo.Campus;
        Vm.Estado          = dto.Estado ?? CredencialEstado.Emitida;
        Vm.IdCriptografico = dto.IdCriptografico ?? string.Empty;
        Vm.FechaEmision    = dto.FechaEmision ?? DateTime.UtcNow;
        Vm.FechaExpiracion = dto.FechaExpiracion;
        Vm.UsuarioId       = dto.UsuarioId;

        UsuarioDisplay = dto.UsuarioId.ToString(); // si tienes DTO con Nombre/Email, puedes mostrarlo mejor

        return Page();
    }

    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return Page();

        try
        {
            // Normalizar fechas a UTC (igual que en Crear)
            var emision = Vm.FechaEmision;
            if (emision.Kind == DateTimeKind.Unspecified)
                emision = DateTime.SpecifyKind(emision, DateTimeKind.Local);
            var emisionUtc = emision.ToUniversalTime();

            DateTime? expiracionUtc = null;
            if (Vm.FechaExpiracion.HasValue)
            {
                var exp = Vm.FechaExpiracion.Value;
                if (exp.Kind == DateTimeKind.Unspecified)
                    exp = DateTime.SpecifyKind(exp, DateTimeKind.Local);
                expiracionUtc = exp.ToUniversalTime();
            }

            await _mediator.Send(new UpdateCredencialCommand
            {
                CredencialId   = Vm.CredencialId,
                Tipo           = Vm.Tipo,
                Estado         = Vm.Estado,
                IdCriptografico = Vm.IdCriptografico,
                FechaEmision   = emisionUtc,
                FechaExpiracion = expiracionUtc
                // No tocamos UsuarioId ni EventoAccesoIds aquí
            }, ct);

            TempData["Ok"] = "Credencial actualizada.";
            return RedirectToPage("/Credenciales/Index", new { area = "Admin" });
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

    public class VmCredencial
    {
        [Required] public Guid CredencialId { get; set; }

        [Required] public CredencialTipo Tipo { get; set; }
        [Required] public CredencialEstado Estado { get; set; }

        [Required, MaxLength(100)]
        public string? IdCriptografico { get; set; }

        [Required]
        public DateTime FechaEmision { get; set; }

        public DateTime? FechaExpiracion { get; set; }

        [Required]
        public Guid UsuarioId { get; set; }
    }
}
