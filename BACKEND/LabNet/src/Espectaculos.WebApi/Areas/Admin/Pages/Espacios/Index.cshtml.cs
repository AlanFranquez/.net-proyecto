using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Espectaculos.Application.Espacios.Queries.ListarEspacios;
using Espectaculos.Application.Espacios.Commands.DeleteEspacio;

namespace Espectaculos.WebApi.Areas.Admin.Pages.Espacios;

public class IndexModel : PageModel
{
    private readonly IMediator _mediator;
    public IndexModel(IMediator mediator) => _mediator = mediator;

    public List<Espectaculos.Application.DTOs.EspacioDTO> Items { get; set; } = [];

    public async Task OnGet() => Items = await _mediator.Send(new ListarEspaciosQuery());

    public async Task<IActionResult> OnPostDelete(Guid id)
    {
        try
        {
            await _mediator.Send(new DeleteEspacioCommand { Id = id });
            TempData["Ok"] = "Espacio eliminado.";
        }
        catch (Exception ex)
        {
            TempData["Ok"] = $"No se pudo eliminar: {ex.Message}";
        }
        return RedirectToPage();
    }
}