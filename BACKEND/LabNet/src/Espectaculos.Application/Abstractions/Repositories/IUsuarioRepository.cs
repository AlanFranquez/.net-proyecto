using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Espectaculos.Domain.Entities;
using Espectaculos.Application.Abstractions.Repositories;

namespace Espectaculos.Application.Abstractions.Repositories
{
    public interface IUsuarioRepository : IRepository<Usuario, Guid>
    {
        Task<Usuario?> GetByEmailAsync(string email, CancellationToken ct = default);
        Task<Usuario?> GetWithRolesAsync(Guid usuarioId, CancellationToken ct = default);
        Task<Usuario?> GetWithDispositivosAsync(Guid usuarioId, CancellationToken ct = default);
        Task<(IReadOnlyList<Usuario> Items, int Total)> SearchAsync(string? term, int page = 1, int pageSize = 20, CancellationToken ct = default);
        Task AddAsync(Usuario usuario, CancellationToken ct = default);
        

        Task SaveChangesAsync(CancellationToken ct = default);
        
        Task<IReadOnlyList<Usuario>> ListAsync(CancellationToken ct = default);
        Task<Usuario?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task UpdateAsync(Usuario usuario, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<Usuario>> ListByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
        Task RemoveRolesRelacionados(Guid id, CancellationToken ct = default);
        Task RemoveBeneficiosRelacionados(Guid id, CancellationToken ct = default);
        
    }
}