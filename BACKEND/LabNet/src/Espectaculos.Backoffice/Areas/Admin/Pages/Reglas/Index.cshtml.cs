using Espectaculos.Application.ReglaDeAcceso.Queries.ListarReglasDeAcceso;
using Espectaculos.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Espectaculos.Backoffice.Areas.Admin.Pages.Reglas;

public class IndexModel : PageModel
{
    private readonly IMediator _mediator;
    public IndexModel(IMediator mediator) => _mediator = mediator;

    public List<ReglaDeAccesoDTO> Items { get; set; } = new();

    public async Task OnGetAsync()
    {
        Items = await _mediator.Send(new ListarReglasQuery());
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        await _mediator.Send(new Espectaculos.Application.ReglaDeAcceso.Commands.DeleteReglaDeAcceso.DeleteReglaCommand
        {
            ReglaId = id
        });
        TempData["Ok"] = "Regla eliminada.";
        return RedirectToPage();
    }
}