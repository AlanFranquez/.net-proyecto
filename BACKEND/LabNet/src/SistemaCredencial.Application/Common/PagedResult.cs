using System;
using System.Collections.Generic;
using System.Linq;

namespace Espectaculos.Application.Common
{
    public sealed class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; }
        public int TotalCount { get; }
        public int Page { get; }
        public int PageSize { get; }
        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

        public PagedResult(IEnumerable<T> items, int totalCount, int page, int pageSize)
        {
            Items = items?.ToList() ?? new List<T>();
            TotalCount = totalCount;
            Page = page;
            PageSize = pageSize;
        }
    }
}
