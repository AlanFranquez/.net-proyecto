using Espectaculos.Application.Abstractions.Repositories;
using Espectaculos.Application.Common;
using Espectaculos.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Espectaculos.Backoffice.Areas.Admin.Pages.Usuarios
{
    public class IndexModel : PageModel
    {
        private readonly IUsuarioRepository _repo;

        public IndexModel(IUsuarioRepository repo) => _repo = repo;

        [BindProperty(SupportsGet = true)] public string? Q { get; set; }
        [BindProperty(SupportsGet = true)] public int Page { get; set; } = 1;
        [BindProperty(SupportsGet = true)] public int PageSize { get; set; } = 10;

        public PagedResult<UsuarioDto> Paged { get; private set; } =
            new PagedResult<UsuarioDto>(Array.Empty<UsuarioDto>(), 0, 1, 10);

        public async Task OnGet()
        {
            await LoadAsync();
        }

        public async Task<IActionResult> OnPostBuscarAsync()
        {
            Page = 1;
            await LoadAsync();
            return Page();
        }

        private async Task LoadAsync()
        {
            var term = string.IsNullOrWhiteSpace(Q) ? null : Q!.Trim();
            var (items, total) = await _repo.SearchAsync(term, Math.Max(1, Page), Math.Max(1, PageSize));

            var dtos = items.Select(u => new UsuarioDto
            {
                UsuarioId = u.UsuarioId,
                Nombre = u.Nombre,
                Apellido = u.Apellido,
                Email = u.Email,
                Documento = u.Documento,
                Estado = u.Estado,
                CredencialId = u.CredencialId,
                RolesIDs = u.UsuarioRoles.Select(r => r.RolId).ToList(),
                DispositivosIDs = u.Dispositivos.Select(d => d.DispositivoId).ToList(),
                BeneficiosIDs = u.Beneficios.Select(b => b.BeneficioId).ToList(),
                CanjesIDs = u.Canjes.Select(c => c.CanjeId).ToList()
            }).ToList();

            Paged = new PagedResult<UsuarioDto>(dtos, total, Math.Max(1, Page), Math.Max(1, PageSize));
        }
    }
}
