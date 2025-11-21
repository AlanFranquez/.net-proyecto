using Espectaculos.Application.Novedades.Commands.CreateNovedad;
using Espectaculos.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Espectaculos.WebApi.Areas.Admin.Pages.Novedades
{
    public class CrearModel : PageModel
    {
        private readonly IMediator _mediator;
        public CrearModel(IMediator mediator) => _mediator = mediator;

        [BindProperty] public InputModel Input { get; set; } = new();

        public List<SelectListItem> TipoOptions { get; private set; } =
            Enum.GetValues(typeof(NotificacionTipo))
                .Cast<NotificacionTipo>()
                .Select(t => new SelectListItem(t.ToString(), t.ToString()))
                .ToList();

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            DateTime? ToUtc(DateTime? dt) =>
                dt.HasValue ? DateTime.SpecifyKind(dt.Value, DateTimeKind.Utc) : null;

            var desde = ToUtc(Input.DesdeUtc);
            var hasta = ToUtc(Input.HastaUtc);
            if (!hasta.HasValue && desde.HasValue)
                hasta = desde;
            // ✅ Validación de rango ANTES del Mediator
            if (desde.HasValue && hasta.HasValue && desde > hasta)
            {
                ModelState.AddModelError(string.Empty, "PublicadoDesde debe ser anterior o igual a PublicadoHasta");
                Input.DesdeUtc = desde;
                Input.HastaUtc = hasta;
                return Page();
            }

            var id = await _mediator.Send(new CreateNovedadCommand(
                Input.Titulo, Input.Contenido, Input.Tipo, desde, hasta));

            TempData["Ok"] = "Novedad creada.";
            return RedirectToPage("/Novedades/Editar", new { area = "Admin", id });
        }

        public class InputModel
        {
            public string Titulo { get; set; } = string.Empty;
            public string? Contenido { get; set; }
            public NotificacionTipo Tipo { get; set; }
            public DateTime? DesdeUtc { get; set; }
            public DateTime? HastaUtc { get; set; }
        }
    }
}
