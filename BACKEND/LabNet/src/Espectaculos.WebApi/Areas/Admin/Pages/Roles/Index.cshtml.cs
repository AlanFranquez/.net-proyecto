using Espectaculos.Application.Roles.Queries.ListarRoles;
using Espectaculos.Application.Roles.Commands.DeleteRol;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Espectaculos.WebApi.Areas.Admin.Pages.Roles;

public class IndexModel : PageModel
{
    private readonly IMediator _mediator;
    public IndexModel(IMediator mediator) => _mediator = mediator;

    public List<Espectaculos.Application.DTOs.RolDTO> Roles { get; set; } = [];

    public async Task OnGet()
    {
        Roles = await _mediator.Send(new ListarRolesQuery());
    }

    public async Task<IActionResult> OnPostDelete(Guid id)
    {
        try
        {
            await _mediator.Send(new DeleteRolCommand { RolId = id });
            TempData["Ok"] = "Rol eliminado.";
        }
        catch (Exception ex)
        {
            TempData["Ok"] = $"No se pudo eliminar: {ex.Message}";
        }
        return RedirectToPage();
    }
}