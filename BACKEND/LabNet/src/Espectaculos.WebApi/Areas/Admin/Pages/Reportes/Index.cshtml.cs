using Espectaculos.Application.DTOs;
using Espectaculos.Application.EventoAcceso.Queries.ListarEventos;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Espectaculos.Backoffice.Areas.Admin.Pages.Reportes
{
    public class IndexModel : PageModel
    {
        private readonly IMediator _mediator;

        public IndexModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Histórico completo (o podrías luego paginarlo)
        public IReadOnlyList<EventoAccesoDTO> Historico { get; private set; } =
            Array.Empty<EventoAccesoDTO>();

        public async Task OnGet()
        {
            var eventos = await _mediator.Send(new ListarEventosQuery());
            Historico = eventos;
        }
    }
}