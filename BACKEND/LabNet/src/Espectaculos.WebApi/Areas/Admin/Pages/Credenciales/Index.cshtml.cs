using Espectaculos.Application.Credenciales.Queries.ListarCredenciales;
using Espectaculos.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Espectaculos.Backoffice.Areas.Admin.Pages.Credenciales;

public class IndexModel : PageModel
{
    private readonly IMediator _mediator;

    public IndexModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public IList<VmCredencial> Credenciales { get; set; } = new List<VmCredencial>();

    public async Task OnGetAsync(CancellationToken ct)
    {
        var lista = await _mediator.Send(new ListarCredencialesQuery(), ct);

        Credenciales = lista
            .Select(c => new VmCredencial
            {
                CredencialId    = c.CredencialId,
                Tipo            = c.Tipo   ?? CredencialTipo.Campus,
                Estado          = c.Estado ?? CredencialEstado.Emitida,
                IdCriptografico = c.IdCriptografico ?? string.Empty,
                FechaEmision    = c.FechaEmision ?? DateTime.MinValue,
                FechaExpiracion = c.FechaExpiracion,
                UsuarioId       = c.UsuarioId,
                UsuarioNombre   = c.UsuarioNombre ?? "",
                UsuarioApellido = c.UsuarioApellido ?? ""
            })
            .ToList();

    }

    public class VmCredencial
    {
        public Guid CredencialId { get; set; }
        public CredencialTipo Tipo { get; set; }
        public CredencialEstado Estado { get; set; }
        public string? IdCriptografico { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime? FechaExpiracion { get; set; }
        public Guid UsuarioId { get; set; }
        public string UsuarioNombre { get; set; } = "";
        public string UsuarioApellido { get; set; } = "";
    }

}