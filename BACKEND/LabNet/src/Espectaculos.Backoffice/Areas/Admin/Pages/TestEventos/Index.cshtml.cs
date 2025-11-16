using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Espectaculos.Application.EventoAcceso.Commands.CreateEvento;
using Espectaculos.Application.Usuarios.Queries.ListarUsuarios;
using Espectaculos.Application.Usuarios.Queries.GetUsuarioById;
using Espectaculos.Application.Espacios.Queries.ListarEspacios;
using Espectaculos.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Espectaculos.Backoffice.Areas.Admin.Pages.TestEventos
{
    public class IndexModel : PageModel
    {
        private readonly IMediator _mediator;

        public IndexModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public IEnumerable<SelectListItem> UsuarioOptions { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> EspacioOptions { get; set; } = Enumerable.Empty<SelectListItem>();

        [TempData]
        public string? Message { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync(CancellationToken ct)
        {
            await LoadOptionsAsync(ct);

            if (Input.MomentoDeAcceso == null)
                Input.MomentoDeAcceso = DateTime.UtcNow;
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync(CancellationToken ct)
        {
            await LoadOptionsAsync(ct);

            if (!ModelState.IsValid)
                return Page();

            // 🔹 Buscar usuario y obtener su credencial
            var usuario = await _mediator.Send(new GetUsuarioByIdQuery(Input.UsuarioId), ct);
            if (usuario is null)
            {
                ModelState.AddModelError(nameof(Input.UsuarioId), "Usuario no encontrado.");
                return Page();
            }

            if (!usuario.CredencialId.HasValue)
            {
                ModelState.AddModelError(nameof(Input.UsuarioId), "El usuario seleccionado no tiene una credencial asignada.");
                return Page();
            }

            var credencialId = usuario.CredencialId.Value;

            // 🔹 Normalizar fecha a UTC
            var momento = Input.MomentoDeAcceso ?? DateTime.UtcNow;
            if (momento.Kind == DateTimeKind.Unspecified)
                momento = DateTime.SpecifyKind(momento, DateTimeKind.Local);
            var momentoUtc = momento.ToUniversalTime();

            var command = new CreateEventoCommand
            {
                MomentoDeAcceso = momentoUtc,
                CredencialId    = credencialId,
                EspacioId       = Input.EspacioId,
                Resultado       = Input.Resultado,
                Motivo          = Input.Motivo,
                Modo            = Input.Modo,
                Firma           = Input.Firma
            };

            try
            {
                var id = await _mediator.Send(command, ct);
                Message = $"Evento creado correctamente. Id: {id}";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return Page();
            }
        }

        private async Task LoadOptionsAsync(CancellationToken ct)
        {
            // 🔹 Usuarios con credencial
            var usuarios = await _mediator.Send(new ListarUsuariosQuery(), ct);

            UsuarioOptions = usuarios
                .Where(u => u.CredencialId.HasValue)
                .Select(u => new SelectListItem
                {
                    Value = u.UsuarioId.ToString(),
                    Text  = $"{u.Nombre} {u.Apellido} ({u.Email})"
                })
                .ToList();

            var espacios = await _mediator.Send(new ListarEspaciosQuery(), ct);

            EspacioOptions = espacios
                .Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text  = e.Nombre
                })
                .ToList();
        }

        public class InputModel
        {
            [Display(Name = "Momento de acceso (UTC)")]
            public DateTime? MomentoDeAcceso { get; set; }

            [Required]
            [Display(Name = "Usuario")]
            public Guid UsuarioId { get; set; }

            [Required]
            [Display(Name = "Espacio")]
            public Guid EspacioId { get; set; }

            [Required]
            [Display(Name = "Resultado")]
            public AccesoTipo Resultado { get; set; }

            [Display(Name = "Motivo")]
            public string? Motivo { get; set; }

            [Required]
            [Display(Name = "Modo")]
            public Modo Modo { get; set; }

            [Display(Name = "Firma")]
            public string? Firma { get; set; }
        }
    }
}
