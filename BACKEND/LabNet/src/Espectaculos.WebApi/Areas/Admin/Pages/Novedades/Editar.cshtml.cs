using Espectaculos.Application.Novedades.Commands.PublishUnpublish;
using Espectaculos.Application.Novedades.Commands.UpdateNovedad;
using Espectaculos.Application.Novedades.Queries;
using Espectaculos.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Espectaculos.Backoffice.Areas.Admin.Pages.Novedades
{
    public class EditarModel : PageModel
    {
        private readonly IMediator _mediator;
        public EditarModel(IMediator mediator) => _mediator = mediator;

        [BindProperty(SupportsGet = true)] public Guid Id { get; set; }
        public bool Publicado { get; private set; }

        [BindProperty] public InputModel Input { get; set; } = new();

        public List<SelectListItem> TipoOptions { get; private set; } =
            Enum.GetValues(typeof(NotificacionTipo))
                .Cast<NotificacionTipo>()
                .Select(t => new SelectListItem(t.ToString(), t.ToString()))
                .ToList();

        public async Task<IActionResult> OnGet()
        {
            var (items, _) = await _mediator.Send(new ListarNovedadesQuery(null, null, false, false, 1, 1000));
            var novedad = items.FirstOrDefault(x => x.Id == Id);
            if (novedad is null) return NotFound();

            Publicado = novedad.Publicado;

            Input = new InputModel
            {
                Titulo = novedad.Titulo,
                Contenido = novedad.Contenido,
                Tipo = novedad.Tipo,
                // Mostrar tal cual en el input (Unspecified). Al postear se marca como UTC.
                PublicadoDesdeUtc = novedad.DesdeUtc,
                PublicadoHastaUtc = novedad.HastaUtc
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            DateTime? ToUtc(DateTime? dt) =>
                dt.HasValue ? DateTime.SpecifyKind(dt.Value, DateTimeKind.Utc) : null;

            var desde = ToUtc(Input.PublicadoDesdeUtc);
            var hasta = ToUtc(Input.PublicadoHastaUtc);

            // ✅ Validación de rango ANTES del Mediator
            if (desde.HasValue && hasta.HasValue && desde > hasta)
            {
                ModelState.AddModelError(string.Empty, "PublicadoDesde debe ser anterior o igual a PublicadoHasta");
                // Devolver la página con los valores corregidos a UTC para que el usuario los vea
                Input.PublicadoDesdeUtc = desde;
                Input.PublicadoHastaUtc = hasta;
                return Page();
            }

            await _mediator.Send(new UpdateNovedadCommand(
                Id,
                Input.Titulo,
                Input.Contenido,
                Input.Tipo,
                desde,
                hasta
            ));

            TempData["Ok"] = "Cambios guardados.";
            return RedirectToPage("/Novedades/Editar", new { area = "Admin", id = Id });
        }

        public async Task<IActionResult> OnPostPublicarAsync()
        {
            DateTime? ToUtc(DateTime? dt) =>
                dt.HasValue ? DateTime.SpecifyKind(dt.Value, DateTimeKind.Utc) : null;

            var desde = ToUtc(Input.PublicadoDesdeUtc);
            var hasta = ToUtc(Input.PublicadoHastaUtc);
            if (!hasta.HasValue && desde.HasValue)
                hasta = desde;
            // ✅ También validar rango acá
            if (desde.HasValue && hasta.HasValue && desde > hasta)
            {
                ModelState.AddModelError(string.Empty, "PublicadoDesde debe ser anterior o igual a PublicadoHasta");
                Input.PublicadoDesdeUtc = desde;
                Input.PublicadoHastaUtc = hasta;
                return Page();
            }

            await _mediator.Send(new PublishNovedadCommand(Id, desde, hasta));
            TempData["Ok"] = "Novedad publicada.";
            return RedirectToPage("/Novedades/Editar", new { area = "Admin", id = Id });
        }

        public async Task<IActionResult> OnPostDespublicarAsync()
        {
            await _mediator.Send(new UnpublishNovedadCommand(Id));
            TempData["Ok"] = "Novedad despublicada.";
            return RedirectToPage("/Novedades/Editar", new { area = "Admin", id = Id });
        }

        public class InputModel
        {
            public string Titulo { get; set; } = string.Empty;
            public string? Contenido { get; set; }
            public NotificacionTipo Tipo { get; set; }
            public DateTime? PublicadoDesdeUtc { get; set; }
            public DateTime? PublicadoHastaUtc { get; set; }
        }
    }
}
