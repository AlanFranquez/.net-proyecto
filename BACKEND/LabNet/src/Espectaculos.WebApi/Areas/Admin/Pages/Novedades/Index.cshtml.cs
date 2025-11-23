using Espectaculos.Application.Common;
using Espectaculos.Application.DTOs; 
using Espectaculos.Application.Novedades.Queries;
using Espectaculos.Application.Novedades.Commands.PublishUnpublish;
using Espectaculos.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Espectaculos.WebApi.Areas.Admin.Pages.Novedades
{
    public class IndexModel : PageModel
    {
        private readonly IMediator _mediator;

        public IndexModel(IMediator mediator) => _mediator = mediator;

        // Filtros
        [BindProperty(SupportsGet = true)] public string? Q { get; set; }
        [BindProperty(SupportsGet = true)] public NotificacionTipo? Tipo { get; set; }
        [BindProperty(SupportsGet = true)] public bool OnlyPublished { get; set; } = false;
        [BindProperty(SupportsGet = true)] public bool OnlyActive { get; set; } = false;

        // Paginación
        [BindProperty(SupportsGet = true)] public int Page { get; set; } = 1;
        [BindProperty(SupportsGet = true)] public int PageSize { get; set; } = 10;

        public List<SelectListItem> TipoOptions { get; } =
            Enum.GetValues(typeof(NotificacionTipo))
                .Cast<NotificacionTipo>()
                .Select(t => new SelectListItem(t.ToString(), t.ToString()))
                .ToList();

        public PagedResult<NovedadDto> Paged { get; private set; } =
            new PagedResult<NovedadDto>(Array.Empty<NovedadDto>(), 0, 1, 10);

        public async Task OnGet() => await LoadAsync();

        public async Task<IActionResult> OnPostBuscarAsync()
        {
            Page = 1;
            await LoadAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostPublicarAsync(Guid id)
        {
            try
            {
                await _mediator.Send(new PublishNovedadCommand(id)); // desde ahora/indefinido
                TempData["Ok"] = "Novedad publicada.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToPage(new { Q, Tipo, OnlyPublished, OnlyActive, Page, PageSize });
        }

        public async Task<IActionResult> OnPostDespublicarAsync(Guid id)
        {
            try
            {
                await _mediator.Send(new UnpublishNovedadCommand(id));
                TempData["Ok"] = "Novedad despublicada.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToPage(new { Q, Tipo, OnlyPublished, OnlyActive, Page, PageSize });
        }

        private async Task LoadAsync()
        {
            var (items, total) = await _mediator.Send(new ListarNovedadesQuery(
                Q, Tipo, OnlyPublished, OnlyActive, Math.Max(1, Page), Math.Max(1, PageSize)));

            Paged = new PagedResult<NovedadDto>(items, total, Math.Max(1, Page), Math.Max(1, PageSize));
        }

        // Helpers
        public string GetVigenciaText(DateTime? desdeUtc, DateTime? hastaUtc)
        {
            string desde = desdeUtc?.ToLocalTime().ToString("yyyy-MM-dd HH:mm") ?? "—";
            string hasta = hastaUtc?.ToLocalTime().ToString("yyyy-MM-dd HH:mm") ?? "—";
            return $"Desde {desde} · Hasta {hasta}";
        }
    }
}
