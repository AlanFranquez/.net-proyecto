using System.ComponentModel.DataAnnotations;
using Espectaculos.Application.Beneficios.Commands.UpdateBeneficio;
using Espectaculos.Application.Beneficios.Queries.GetBeneficioById;
using Espectaculos.Application.Espacios.Queries.ListarEspacios;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

        public IEnumerable<SelectListItem> EspaciosOptions { get; set; } = Enumerable.Empty<SelectListItem>();

        public async Task<IActionResult> OnGetAsync(CancellationToken ct)
        {
            var b = await _mediator.Send(new GetBeneficioByIdQuery(Id), ct);
            if (b is null)
            {
                TempData["Error"] = "Beneficio no encontrado";
                return RedirectToPage("/Beneficios/Index", new { area = "Admin" });
            }

            // DateTime? -> DateOnly?
            DateOnly? ToDateOnly(DateTime? dt) =>
                dt.HasValue ? DateOnly.FromDateTime(dt.Value) : (DateOnly?)null;

            Vm = new EditarVm
            {
                Id                   = b.Id,
                Tipo                 = b.Tipo,
                Nombre               = b.Nombre,
                Descripcion          = b.Descripcion,
                VigenciaInicio       = ToDateOnly(b.VigenciaInicio),
                VigenciaFin          = ToDateOnly(b.VigenciaFin),
                CupoTotal            = b.CupoTotal,
                CupoPorUsuario       = b.CupoPorUsuario,
                RequiereBiometria    = b.RequiereBiometria,
                CriterioElegibilidad = b.CriterioElegibilidad,
                // 👇 IDs de espacios ya relacionados con este beneficio
                EspaciosIDs          = b.EspaciosIDs?.ToList() ?? new List<Guid>()
            };

            await LoadEspaciosAsync(ct);
            return Page();
        }

        private async Task LoadEspaciosAsync(CancellationToken ct)
        {
            var espacios      = await _mediator.Send(new ListarEspaciosQuery(), ct);
            var seleccionados = Vm.EspaciosIDs ?? new List<Guid>();

            EspaciosOptions = espacios
                .Select(e => new SelectListItem
                {
                    Value    = e.Id.ToString(),
                    Text     = e.Nombre,
                    // 👇 esto hace que se vean marcados los espacios previamente seleccionados
                    Selected = seleccionados.Contains(e.Id)
                })
                .ToList();
        }

        public async Task<IActionResult> OnPostAsync(CancellationToken ct)
        {
            // Si hay error de validación, esto mantiene la selección de espacios
            await LoadEspaciosAsync(ct);

            if (!ModelState.IsValid)
                return Page();

            DateTime? ToUtcDateTime(DateOnly? d) => d.HasValue
                ? DateTime.SpecifyKind(d.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc)
                : (DateTime?)null;

            var ok = await _mediator.Send(new UpdateBeneficioCommand
            {
                Id                   = Vm.Id,
                Tipo                 = Vm.Tipo,
                Nombre               = Vm.Nombre,
                Descripcion          = Vm.Descripcion,
                VigenciaInicio       = ToUtcDateTime(Vm.VigenciaInicio),
                VigenciaFin          = ToUtcDateTime(Vm.VigenciaFin),
                CupoTotal            = Vm.CupoTotal,
                CupoPorUsuario       = Vm.CupoPorUsuario,
                RequiereBiometria    = Vm.RequiereBiometria,
                CriterioElegibilidad = Vm.CriterioElegibilidad,
                // 👇 selección actual de espacios
                EspaciosIDs          = Vm.EspaciosIDs ?? new List<Guid>()
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
            [Required]
            public Guid Id { get; set; }

            public Espectaculos.Domain.Enums.BeneficioTipo? Tipo { get; set; }

            [Required]
            public string? Nombre { get; set; }

            public string? Descripcion { get; set; }

            [DataType(DataType.Date)]
            public DateOnly? VigenciaInicio { get; set; }

            [DataType(DataType.Date)]
            public DateOnly? VigenciaFin { get; set; }

            [Range(0, int.MaxValue)]
            public int? CupoTotal { get; set; }

            [Range(0, int.MaxValue)]
            public int? CupoPorUsuario { get; set; }

            public bool?   RequiereBiometria    { get; set; }
            public string? CriterioElegibilidad { get; set; }

            // 👇 multi-select de espacios
            public List<Guid>? EspaciosIDs { get; set; } = new();
        }
    }
}
