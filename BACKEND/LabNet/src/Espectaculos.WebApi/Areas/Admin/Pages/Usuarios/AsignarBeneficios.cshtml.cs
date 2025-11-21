using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Espectaculos.Domain.Entities;
using Espectaculos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Espectaculos.WebApi.Areas.Admin.Pages.Usuarios
{
    public class AsignarBeneficiosModel : PageModel
    {
        private readonly EspectaculosDbContext _db;

        public AsignarBeneficiosModel(EspectaculosDbContext db)
        {
            _db = db;
        }

        // ---- Datos del usuario ----
        [BindProperty]
        public Guid UsuarioId { get; set; }

        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // ---- VM para cada beneficio ----
        public class BeneficioCheckVm
        {
            public Guid BeneficioId { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public bool Asignado { get; set; }
        }

        [BindProperty]
        public List<BeneficioCheckVm> Beneficios { get; set; } = new();

        // GET /Usuarios/AsignarBeneficios?id={guid}
        public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct)
        {
            UsuarioId = id;
            await CargarModeloAsync(ct);
            if (!ModelState.IsValid) // por si el usuario no existe
                return NotFound();

            return Page();
        }

        // POST /Usuarios/AsignarBeneficios
        public async Task<IActionResult> OnPostAsync(CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                await CargarModeloAsync(ct); // recarga nombres para mostrar de nuevo la vista
                return Page();
            }

            // Cargo usuario + relaciones actuales
            var usuario = await _db.Usuario
                .Include(u => u.Beneficios)
                .FirstOrDefaultAsync(u => u.UsuarioId == UsuarioId, ct);

            if (usuario == null)
            {
                ModelState.AddModelError(string.Empty, "Usuario no encontrado.");
                return NotFound();
            }

            // IDs que el usuario seleccionó en el formulario
            var seleccionados = Beneficios
                .Where(b => b.Asignado)
                .Select(b => b.BeneficioId)
                .ToHashSet();

            // Mantengo solo los que siguen seleccionados
            usuario.Beneficios = usuario.Beneficios
                .Where(ub => seleccionados.Contains(ub.BeneficioId))
                .ToList();

            var actualesIds = usuario.Beneficios
                .Select(ub => ub.BeneficioId)
                .ToHashSet();

            // IDs nuevos a agregar
            var aAgregar = seleccionados
                .Except(actualesIds)
                .ToList();

            foreach (var beneficioId in aAgregar)
            {
                usuario.Beneficios.Add(new BeneficioUsuario
                {
                    UsuarioId = usuario.UsuarioId,
                    BeneficioId = beneficioId
                });
            }

            await _db.SaveChangesAsync(ct);

            TempData["Ok"] = "Beneficios actualizados correctamente.";
            return RedirectToPage("/Usuarios/Index");
        }

        /// <summary>
        /// Carga datos de usuario y lista de beneficios (marcando los asignados).
        /// </summary>
        private async Task CargarModeloAsync(CancellationToken ct)
        {
            var usuario = await _db.Usuario
                .Include(u => u.Beneficios)
                .ThenInclude(ub => ub.Beneficio)
                .FirstOrDefaultAsync(u => u.UsuarioId == UsuarioId, ct);

            if (usuario == null)
            {
                ModelState.AddModelError(string.Empty, "Usuario no encontrado.");
                return;
            }

            NombreCompleto = $"{usuario.Nombre} {usuario.Apellido}";
            Email = usuario.Email;

            var beneficios = await _db.Beneficios
                .OrderBy(b => b.Nombre)
                .ToListAsync(ct);

            var asignadosIds = usuario.Beneficios
                .Select(ub => ub.BeneficioId)
                .ToHashSet();

            Beneficios = beneficios
                .Select(b => new BeneficioCheckVm
                {
                    BeneficioId = b.BeneficioId,
                    Nombre = b.Nombre,
                    Asignado = asignadosIds.Contains(b.BeneficioId)
                })
                .ToList();
        }
    }
}
