using System.ComponentModel.DataAnnotations;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Espectaculos.Application.Usuarios.Queries.ListarUsuarios;
using Espectaculos.Application.Credenciales.Commands.CreateCredencial;
using Espectaculos.Domain.Enums;
using ValidationException = FluentValidation.ValidationException;

namespace Espectaculos.WebApi.Areas.Admin.Pages.Credenciales
{
    public class CrearModel : PageModel
    {
        private readonly IMediator _mediator;
        public CrearModel(IMediator mediator) => _mediator = mediator;

        [BindProperty] public VmCredencial Vm { get; set; } = new();

        public IEnumerable<SelectListItem> UsuarioOptions { get; set; } = Enumerable.Empty<SelectListItem>();

        public async Task OnGetAsync(CancellationToken ct)
        {
            await LoadUsuarioOptionsAsync(ct);
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync(CancellationToken ct)
        {
            await LoadUsuarioOptionsAsync(ct);

            if (!ModelState.IsValid)
                return Page();

            try
            {
                // 🔹 Normalizar FechaEmision a UTC
                var emision = Vm.FechaEmision;
                if (emision.Kind == DateTimeKind.Unspecified)
                {
                    // asumimos que el usuario ingresó hora local
                    emision = DateTime.SpecifyKind(emision, DateTimeKind.Local);
                }
                var emisionUtc = emision.ToUniversalTime();

                // 🔹 Normalizar FechaExpiracion a UTC (si existe)
                DateTime? expiracionUtc = null;
                if (Vm.FechaExpiracion.HasValue)
                {
                    var exp = Vm.FechaExpiracion.Value;
                    if (exp.Kind == DateTimeKind.Unspecified)
                    {
                        exp = DateTime.SpecifyKind(exp, DateTimeKind.Local);
                    }
                    expiracionUtc = exp.ToUniversalTime();
                }

                var id = await _mediator.Send(new CreateCredencialCommand
                {
                    Tipo            = Vm.Tipo,
                    Estado          = Vm.Estado,
                    IdCriptografico = Vm.IdCriptografico,
                    FechaEmision    = emisionUtc,
                    FechaExpiracion = expiracionUtc,
                    UsuarioId       = Vm.UsuarioId
                }, ct);

                TempData["Ok"] = $"Credencial creada ({id}).";
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
                ModelState.AddModelError("", ex.Message);
                return Page();
            }
        }


        private async Task LoadUsuarioOptionsAsync(CancellationToken ct)
        {
            var usuarios = await _mediator.Send(new ListarUsuariosQuery(), ct);

            UsuarioOptions = usuarios
                .Select(u => new SelectListItem
                {
                    Value = u.UsuarioId.ToString(),
                    Text  = $"{u.Nombre} {u.Apellido} ({u.Email})"
                })
                .ToList();
        }

        public class VmCredencial
        {
            [Required] public CredencialTipo Tipo { get; set; }
            [Required] public CredencialEstado Estado { get; set; }

            [Required, MaxLength(100)]
            public string? IdCriptografico { get; set; }

            [Required]
            public DateTime FechaEmision { get; set; } = DateTime.UtcNow;

            public DateTime? FechaExpiracion { get; set; }

            [Required]
            public Guid UsuarioId { get; set; }
        }
    }
}
