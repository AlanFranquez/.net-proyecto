using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Espectaculos.Application.Beneficios.Queries.ListBeneficios;
using Espectaculos.Application.DTOs;


namespace Espectaculos.WebApi.Areas.Admin.Pages.Beneficios
{
    public class IndexModel : PageModel
    {
        private readonly IMediator _mediator;
        public IndexModel(IMediator mediator) => _mediator = mediator;


        [BindProperty(SupportsGet = true)] public string? Q { get; set; }
        [BindProperty(SupportsGet = true)] public int Page { get; set; } = 1;
        [BindProperty(SupportsGet = true)] public int PageSize { get; set; } = 10;


        public PagedResult<BeneficioDTO> Paged { get; set; } = PagedResult<BeneficioDTO>.Empty();


        public async Task OnGetAsync()
        {
            var all = await _mediator.Send(new ListBeneficiosQuery());


            if (!string.IsNullOrWhiteSpace(Q))
            {
                var q = Q.Trim().ToLowerInvariant();
                all = all.Where(x =>
                        (x.Nombre ?? string.Empty).ToLowerInvariant().Contains(q) ||
                        (x.Descripcion ?? string.Empty).ToLowerInvariant().Contains(q))
                    .ToList();
            }


            Paged = PagedResult<BeneficioDTO>.Create(all, Page, PageSize);
        }
    }


// Minimal helper (drop in if you don't already have a shared one)
    public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
    {
        public int TotalPages => (int)System.Math.Ceiling((double)TotalCount / PageSize);
        public static PagedResult<T> Create(IReadOnlyList<T> source, int page, int pageSize)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 10 : pageSize;
            var total = source.Count;
            var items = source.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return new PagedResult<T>(items, page, pageSize, total);
        }
        public static PagedResult<T> Empty() => new(new List<T>(), 1, 10, 0);
    }
}