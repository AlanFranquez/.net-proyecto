using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Espectaculos.Domain.Entities;
using Espectaculos.Application.Abstractions.Repositories;

namespace Espectaculos.Application.Abstractions.Repositories
{
    public interface IUsuarioRolRepository : IRepository<UsuarioRol, (Guid UsuarioId, Guid RolId)>
    {
        Task<UsuarioRol?> GetAsync(Guid usuarioId, Guid rolId, CancellationToken ct = default);
        Task<bool> AssignAsync(Guid usuarioId, Guid rolId, DateTime? fechaAsignado = null, CancellationToken ct = default);
        Task<bool> RemoveAsync(Guid usuarioId, Guid rolId, CancellationToken ct = default);
        Task<IReadOnlyList<Rol>> GetRolesForUserAsync(Guid usuarioId, CancellationToken ct = default);
        Task<IReadOnlyList<Usuario>> GetUsersForRoleAsync(Guid rolId, CancellationToken ct = default);

        
        
    }
}