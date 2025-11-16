using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Espectaculos.Domain.Entities;
using Espectaculos.Application.Abstractions.Repositories;
using Espectaculos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Espectaculos.Infrastructure.Repositories
{
    public class UsuarioRepository : BaseEfRepository<Usuario, Guid>, IUsuarioRepository
    {
        public UsuarioRepository(EspectaculosDbContext db) : base(db) { }

        public async Task<Usuario?> GetByEmailAsync(string email, CancellationToken ct = default)
            => await _db.Set<Usuario>()
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.Email == email, ct);

        public async Task<Usuario?> GetWithRolesAsync(Guid usuarioId, CancellationToken ct = default)
            => await _db.Set<Usuario>()
                        .AsNoTracking()
                        .Include(u => u.UsuarioRoles)
                            .ThenInclude(ur => ur.Rol)
                        .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId, ct);

        
        public async Task<Usuario?> GetWithDispositivosAsync(Guid usuarioId, CancellationToken ct = default)
            => await _db.Set<Usuario>()
                        .AsNoTracking()
                        .Include(u => u.Dispositivos)
                        .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId, ct);

       
        public async Task<(IReadOnlyList<Usuario> Items, int Total)> SearchAsync(
            string? term, int page = 1, int pageSize = 20, CancellationToken ct = default)
        {
            var q = _db.Set<Usuario>().AsNoTracking();

            if (!string.IsNullOrWhiteSpace(term))
                q = q.Where(u => u.Nombre.Contains(term)
                              || u.Apellido.Contains(term)
                              || u.Email.Contains(term)
                              || u.Documento.Contains(term));

            var total = await q.CountAsync(ct);

            var items = await q.OrderBy(u => u.Apellido)
                               .ThenBy(u => u.Nombre)
                               .Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync(ct);

            return (items, total);
        }
        
        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _db.SaveChangesAsync(ct);
        }

        public virtual async Task<IReadOnlyList<Usuario>> ListAsync(CancellationToken ct = default)
            => await _set.AsNoTracking().Include(r => r.UsuarioRoles).Include(r => r.Beneficios).Include(r => r.Canjes).Include(r => r.Credencial).Include(r => r.Dispositivos).ToListAsync(ct);
        public async Task AddAsync(Usuario usuario, CancellationToken ct = default)
            => await _db.Set<Usuario>().AddAsync(usuario, ct);

    
        public async Task<Usuario?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _db.Usuario
                .AsNoTracking()
                .Include(u => u.UsuarioRoles)
                .Include(u => u.Beneficios)
                .Include(u => u.Dispositivos)
                .Include(u => u.Canjes)
                .Include(u => u.Credencial)
                .Include(u => u.Notificaciones)
                .FirstOrDefaultAsync(u => u.UsuarioId == id, ct);
        }

        
        public async Task UpdateAsync(Usuario usuario, CancellationToken ct = default)
        {
            _db.Set<Usuario>().Update(usuario);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _db.Set<Usuario>().FindAsync(new object?[] { id }, ct);
            if (entity != null)
                _db.Set<Usuario>().Remove(entity);
        }
        
        public async Task<IReadOnlyList<Usuario>> ListByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
        {
            var idArray = ids.Distinct().ToArray();
            return await _db.Set<Usuario>().Where(r => idArray.Contains(r.UsuarioId)).ToListAsync(ct);
        }
        
        public async Task RemoveRolesRelacionados(Guid id, CancellationToken ct = default)
        {
            var relaciones = await _db.Set<UsuarioRol>()
                .Where(era => era.UsuarioId == id)
                .ToListAsync(ct);

            if (relaciones.Any())
            {
                _db.Set<UsuarioRol>().RemoveRange(relaciones);
            }
        }
        
        public async Task RemoveBeneficiosRelacionados(Guid id, CancellationToken ct = default)
        {
            var relaciones = await _db.Set<BeneficioUsuario>()
                .Where(era => era.UsuarioId == id)
                .ToListAsync(ct);

            if (relaciones.Any())
            {
                _db.Set<BeneficioUsuario>().RemoveRange(relaciones);
            }
        }
    }
}
