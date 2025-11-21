using Espectaculos.Application.Credenciales.Commands.DeleteCredencial;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ValidationException = FluentValidation.ValidationException;

namespace Espectaculos.WebApi.Areas.Admin.Pages.Credenciales;

public class EliminarModel : PageModel
{
    private readonly IMediator _mediator;

    public EliminarModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    // No necesitamos OnGet: el POST se hace directo desde el listado.

    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OnPostAsync(Guid id, CancellationToken ct)
    {
        try
        {
            await _mediator.Send(new DeleteCredencialCommand
            {
                CredencialId = id
            }, ct);

            TempData["Ok"] = "Credencial eliminada.";
        }
        catch (ValidationException vex)
        {
            TempData["Ok"] = string.Join(" | ", vex.Errors.Select(e => e.ErrorMessage));
        }
        catch (Exception ex)
        {
            TempData["Ok"] = ex.Message;
        }

        return RedirectToPage("/Credenciales/Index", new { area = "Admin" });
    }
}