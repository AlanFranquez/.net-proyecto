using System.ComponentModel.DataAnnotations;
using Espectaculos.Application.Beneficios.Commands.CreateBeneficio;
using Espectaculos.Application.Espacios.Queries.ListarEspacios;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Espectaculos.WebApi.Areas.Admin.Pages.Beneficios;

public class CrearModel : PageModel
{
    private readonly IMediator _mediator;
    public CrearModel(IMediator mediator) => _mediator = mediator;

    [BindProperty]
    public CrearVm Vm { get; set; } = new();

    public IEnumerable<SelectListItem> EspaciosOptions { get; set; } = Enumerable.Empty<SelectListItem>();

    public async Task OnGetAsync(CancellationToken ct)
    {
        await LoadEspaciosAsync(ct);
    }

    private async Task LoadEspaciosAsync(CancellationToken ct)
    {
        var espacios = await _mediator.Send(new ListarEspaciosQuery(), ct);

        EspaciosOptions = espacios
            .Select(e => new SelectListItem
            {
                Value = e.Id.ToString(),
                Text  = e.Nombre
            })
            .ToList();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        await LoadEspaciosAsync(ct); // 👈 para que el combo no se rompa si hay errores

        if (!ModelState.IsValid) return Page();

        // Validación de rango de fechas a nivel UI
        if (Vm.VigenciaInicio.HasValue && Vm.VigenciaFin.HasValue &&
            Vm.VigenciaInicio.Value > Vm.VigenciaFin.Value)
        {
            ModelState.AddModelError(nameof(Vm.VigenciaInicio),
                "La vigencia de inicio debe ser anterior o igual a la vigencia de fin.");
            return Page();
        }

        DateTime? ToUtcDateTime(DateOnly? d) => d.HasValue
            ? DateTime.SpecifyKind(d.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc)
            : (DateTime?)null;

        try
        {
            var id = await _mediator.Send(new CreateBeneficioCommand(
                Nombre:        Vm.Nombre!,
                Tipo:          Vm.Tipo!.Value,
                Descripcion:   Vm.Descripcion,
                VigenciaInicio: ToUtcDateTime(Vm.VigenciaInicio),
                VigenciaFin:    ToUtcDateTime(Vm.VigenciaFin),
                CupoTotal:     Vm.CupoTotal,
                EspaciosIDs:   Vm.EspaciosIDs ?? new List<Guid>()  // 👈 AHORA SÍ
            ), ct);

            TempData["Ok"] = "Beneficio creado";
            return RedirectToPage("/Beneficios/Editar", new { area = "Admin", id });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }

    public class CrearVm
    {
        [Required] public string? Nombre { get; set; }
        [Required] public Espectaculos.Domain.Enums.BeneficioTipo? Tipo { get; set; }
        public string? Descripcion { get; set; }

        [DataType(DataType.Date)] public DateOnly? VigenciaInicio { get; set; }
        [DataType(DataType.Date)] public DateOnly? VigenciaFin { get; set; }

        [Range(0, int.MaxValue)] public int? CupoTotal { get; set; }

        // multi-select
        public List<Guid>? EspaciosIDs { get; set; } = new();
    }
}
