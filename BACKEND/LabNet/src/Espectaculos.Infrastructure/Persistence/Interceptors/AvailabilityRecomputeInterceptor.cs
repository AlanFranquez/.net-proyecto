using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Espectaculos.Domain.Entities;
using Espectaculos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Espectaculos.Infrastructure.Persistence.Interceptors;

public class AvailabilityRecomputeInterceptor : SaveChangesInterceptor
{
    private static readonly AsyncLocal<bool> _reentrant = new();
    private static readonly ConditionalWeakTable<DbContext, HashSet<Guid>> _pendingByContext = new();

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        var ctx = eventData.Context;
        if (ctx != null)
        {
            /*var affectedIds = ctx.ChangeTracker.Entries<Entrada>()
                .Where(e => e.State == EntityState.Added ||
                            e.State == EntityState.Modified ||
                            e.State == EntityState.Deleted)
                .Select(e => e.Entity.EventoId)
                .Distinct()
                .ToList();

            if (affectedIds.Count > 0)
            {
                var set = _pendingByContext.GetOrCreateValue(ctx);
                set.Clear();
                foreach (var id in affectedIds)
                {
                    set.Add(id);
                }
            }
            */
        }

        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (result <= 0 || _reentrant.Value) return result;

        if (eventData.Context is not EspectaculosDbContext ctx)
            return result;

        if (!_pendingByContext.TryGetValue(ctx, out var pending) || pending.Count == 0)
            return result;

        var ids = pending.ToArray();
        _pendingByContext.Remove(ctx);

        try
        {
            _reentrant.Value = true;

            var now = DateTime.UtcNow;

            // Cargar sumatoria de stock por evento afectado (AsNoTracking para eficiencia)
            /*
            var stockPorEvento = await ctx.Entradas.AsNoTracking()
                .Where(en => ids.Contains(en.EventoId))
                .GroupBy(en => en.EventoId)
                .Select(g => new { EventoId = g.Key, Stock = g.Sum(x => x.StockDisponible) })
                .ToDictionaryAsync(x => x.EventoId, x => x.Stock, cancellationToken);

            // Cargar eventos afectados
            var eventos = await ctx.Eventos
                .Where(e => ids.Contains(e.Id))
                .ToListAsync(cancellationToken);

            var changed = false;
            foreach (var e in eventos)
            {
                var stock = stockPorEvento.TryGetValue(e.Id, out var s) ? s : 0;
                var nuevo = e.Fecha > now && stock > 0;
                if (e.Disponible != nuevo)
                {
                    e.Disponible = nuevo;
                    changed = true;
                }
            }

            if (changed)
            {
                await ctx.SaveChangesAsync(cancellationToken);
            }
            */
        }
        finally
        {
            _reentrant.Value = false;
        }

        return result;
    }
}
