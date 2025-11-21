using Espectaculos.Application.Novedades.Commands.DeleteNovedad;
using Espectaculos.Application.Novedades.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Espectaculos.WebApi.Areas.Admin.Pages.Novedades
{
    public class EliminarModel : PageModel
    {
        private readonly IMediator _mediator;
        public EliminarModel(IMediator mediator) => _mediator = mediator;

        [BindProperty(SupportsGet = true)] public Guid Id { get; set; }
        public string Titulo { get; private set; } = string.Empty;

        public async Task<IActionResult> OnGet()
        {
            var (items, total) = await _mediator.Send(new ListarNovedadesQuery(null, null, false, false, 1, 1));
            var novedad = items.FirstOrDefault(x => x.Id == Id);
            if (novedad is null) return NotFound();

            Titulo = novedad.Titulo;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _mediator.Send(new DeleteNovedadCommand(Id));
            TempData["Ok"] = "Novedad eliminada.";
            return RedirectToPage("/Novedades/Index");
        }
    }
}