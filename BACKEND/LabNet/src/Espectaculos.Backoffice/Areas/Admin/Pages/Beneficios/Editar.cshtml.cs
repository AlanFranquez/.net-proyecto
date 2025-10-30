using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Espectaculos.Application.Beneficios.Queries.GetBeneficioById;
using Espectaculos.Application.Beneficios.Commands.UpdateBeneficio;

namespace Espectaculos.Backoffice.Areas.Admin.Pages.Beneficios
{
    public class EditarModel : PageModel
    {
        private readonly IMediator _mediator;
        public EditarModel(IMediator mediator) => _mediator = mediator;

        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        [BindProperty]
        public EditarVm Vm { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(CancellationToken ct)
        {
            var b = await _mediator.Send(new GetBeneficioByIdQuery(Id), ct);
            if (b is null)
            {
                TempData["Error"] = "Beneficio no encontrado";
                return RedirectToPage("/Beneficios/Index", new { area = "Admin" });
            }

            Vm = new EditarVm
            {
                Id = b.BeneficioId,
                Tipo = b.Tipo,
                Nombre = b.Nombre,
                Descripcion = b.Descripcion,
                VigenciaInicio = b.VigenciaInicio,
                VigenciaFin = b.VigenciaFin,
                CupoTotal = b.CupoTotal,
                CupoPorUsuario = b.CupoPorUsuario,
                RequiereBiometria = b.RequiereBiometria,
                CriterioElegibilidad = b.CriterioElegibilidad
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return Page();

            var ok = await _mediator.Send(new UpdateBeneficioCommand
            {
                Id = Vm.Id,
                Tipo = Vm.Tipo,
                Nombre = Vm.Nombre,
                Descripcion = Vm.Descripcion,
                VigenciaInicio = Vm.VigenciaInicio,
                VigenciaFin = Vm.VigenciaFin,
                CupoTotal = Vm.CupoTotal,
                CupoPorUsuario = Vm.CupoPorUsuario,
                RequiereBiometria = Vm.RequiereBiometria,
                CriterioElegibilidad = Vm.CriterioElegibilidad
            }, ct);

            if (!ok)
            {
                TempData["Error"] = "No se pudo actualizar";
                return Page();
            }

            TempData["Ok"] = "Beneficio actualizado";
            return RedirectToPage(new { id = Vm.Id });
        }

        public class EditarVm
        {
            [Required] public Guid Id { get; set; }
            public Espectaculos.Domain.Enums.BeneficioTipo? Tipo { get; set; }
            [Required] public string? Nombre { get; set; }
            public string? Descripcion { get; set; }
            [DataType(DataType.Date)] public DateTime? VigenciaInicio { get; set; }
            [DataType(DataType.Date)] public DateTime? VigenciaFin { get; set; }
            [Range(0, int.MaxValue)] public int? CupoTotal { get; set; }
            [Range(0, int.MaxValue)] public int? CupoPorUsuario { get; set; }
            public bool? RequiereBiometria { get; set; }
            public string? CriterioElegibilidad { get; set; }
        }
    }
}
