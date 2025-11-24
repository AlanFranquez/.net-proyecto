using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Espectaculos.Domain.Entities;
using Espectaculos.Application.Abstractions.Repositories;

namespace Espectaculos.Application.Abstractions.Repositories
{
    public interface IBeneficioUsuarioRepository : IRepository<BeneficioUsuario, (Guid BeneficioId, Guid UsuarioId)>
    {
        Task<IReadOnlyList<BeneficioUsuario>> ListByBeneficioAsync(Guid beneficioId, CancellationToken ct = default);
        Task<IReadOnlyList<BeneficioUsuario>> ListByUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
    }
}