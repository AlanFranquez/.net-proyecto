using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Espectaculos.Application.Beneficios.Commands.CreateBeneficio;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Espectaculos.Backoffice.Areas.Admin.Pages.Beneficios
{
    public class CrearModel : PageModel
    {
        private readonly IMediator _mediator;
        public CrearModel(IMediator mediator) => _mediator = mediator;

        [BindProperty]
        public CrearVm Vm { get; set; } = new();

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync(CancellationToken ct)
        {
            if (!ModelState.IsValid) return Page();

            // UI (DateOnly?) -> DateTime? (UTC) para command/DB
            DateTime? ToUtcDateTime(DateOnly? d) => d.HasValue
                ? DateTime.SpecifyKind(d.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc)
                : (DateTime?)null;

            var id = await _mediator.Send(new CreateBeneficioCommand(
                Nombre: Vm.Nombre!,
                Tipo: Vm.Tipo!.Value,
                Descripcion: Vm.Descripcion,
                VigenciaInicio: ToUtcDateTime(Vm.VigenciaInicio),
                VigenciaFin: ToUtcDateTime(Vm.VigenciaFin),
                CupoTotal: Vm.CupoTotal
            ), ct);

            TempData["Ok"] = "Beneficio creado";
            return RedirectToPage("/Beneficios/Editar", new { area = "Admin", id });
        }

        public class CrearVm
        {
            [Required] public string? Nombre { get; set; }
            [Required] public Espectaculos.Domain.Enums.BeneficioTipo? Tipo { get; set; }
            public string? Descripcion { get; set; }
            [DataType(DataType.Date)] public DateOnly? VigenciaInicio { get; set; }
            [DataType(DataType.Date)] public DateOnly? VigenciaFin { get; set; }
            [Range(0, int.MaxValue)] public int? CupoTotal { get; set; }
        }
    }
}
