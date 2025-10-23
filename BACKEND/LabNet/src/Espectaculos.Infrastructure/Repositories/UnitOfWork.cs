using Espectaculos.Application.Abstractions;
using Espectaculos.Application.Abstractions.Repositories;
using Espectaculos.Infrastructure.Persistence;

namespace Espectaculos.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly EspectaculosDbContext _db;

    public UnitOfWork(EspectaculosDbContext db,
                      IEventoRepository eventos,
                      IEntradaRepository entradas,
                      IOrdenRepository ordenes,
                      IUsuarioRepository usuarios,
                      IEspacioRepository espacios,
                      IReglaDeAccesoRepository reglas,
                      IBeneficioRepository beneficios,
                      IBeneficioUsuarioRepository beneficioUsuarios,
                      IBeneficioEspacioRepository beneficioEspacios,
                      ICanjeRepository canjes)
    {
        _db = db;
        Eventos = eventos;
        Entradas = entradas;
        Ordenes = ordenes;
        Usuarios = usuarios;
        Espacios = espacios;
        Reglas = reglas;
        Beneficios = beneficios;
        BeneficioUsuarios = beneficioUsuarios;
        BeneficioEspacios = beneficioEspacios;
        Canjes = canjes;
    }

    public IEventoRepository Eventos { get; }
    public IEntradaRepository Entradas { get; }
    public IOrdenRepository Ordenes { get; }
    public IUsuarioRepository Usuarios { get; }
    public IEspacioRepository Espacios { get; }
    public IReglaDeAccesoRepository Reglas { get; }
    public IBeneficioRepository Beneficios { get; }
    public IBeneficioUsuarioRepository BeneficioUsuarios { get; }
    public IBeneficioEspacioRepository BeneficioEspacios { get; }
    public ICanjeRepository Canjes { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return _db.SaveChangesAsync(cancellationToken);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException ex)
        {
            // map to application-level concurrency exception
            throw new Espectaculos.Application.Common.Exceptions.ConcurrencyException("Concurrency conflict during SaveChanges", ex);
        }
    }
}
