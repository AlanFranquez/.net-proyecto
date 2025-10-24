using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Espectaculos.Infrastructure.Persistence.Seed;

public class DbSeeder
{
    private readonly EspectaculosDbContext _db;
    private readonly ILogger<DbSeeder> _log;

    public DbSeeder(EspectaculosDbContext db, ILogger<DbSeeder> log)
    {
        _db = db;
        _log = log;
    }

    /// <summary>
    /// Si forceResetAndLoadAll=true → borra tablas de dominio y carga TODO el dataset actual.
    /// </summary>
    public async Task SeedAsync(bool forceResetAndLoadAll = false, CancellationToken ct = default)
    {
        if (!forceResetAndLoadAll)
        {
            _log.LogInformation("SeedAsync llamado sin reset → nada que hacer.");
            return;
        }

        if (!await _db.Database.CanConnectAsync(ct))
        {
            _log.LogWarning("No se pudo conectar a la DB para seed.");
            return;
        }

        // Borrar en orden de FK (snake_case)
        _log.LogInformation("Borrando datos de dominio (orden_items → ordenes → entradas → eventos)...");
        await _db.Database.ExecuteSqlRawAsync(
            @"TRUNCATE TABLE beneficio, beneficio_espacio, beneficio_usuario, canje, credencial, dispositivo, entrada, espacio, espacio_regla_de_acceso, evento_acceso, evento, notificacion, orden, orden_item, regla_de_acceso, rol, sincronizacion, usuario, usuario_rol  RESTART IDENTITY CASCADE;",
            ct);

        // Cargar dataset completo
        //var eventos = SeedData.GetEventosSeed().ToList();
        //_log.LogInformation("Insertando {Count} eventos (con sus entradas)…", eventos.Count);
        //await _db.Eventos.AddRangeAsync(eventos, ct);
        await _db.SaveChangesAsync(ct);

        _log.LogInformation("Seed OK.");
    }
}