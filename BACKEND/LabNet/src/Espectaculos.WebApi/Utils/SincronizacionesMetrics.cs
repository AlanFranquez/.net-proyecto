using System.Diagnostics.Metrics;
using Espectaculos.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Espectaculos.WebApi.Utils;

public class SincronizacionesMetrics : BackgroundService
{
    private readonly IMeterFactory _meterFactory;
    private readonly IServiceProvider _services;
    private long _backlog;

    public SincronizacionesMetrics(IMeterFactory meterFactory, IServiceProvider services)
    {
        _meterFactory = meterFactory;
        _services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var meter = _meterFactory.Create("espectaculos.metrics");
        // Prometheus exige nombres de métricas con [a-zA-Z_:][a-zA-Z0-9_:]*, por eso usamos guiones bajos
        meter.CreateObservableGauge("app_sincronizaciones_backlog",
            () => _backlog,
            unit: "items",
            description: "Cantidad de sincronizaciones pendientes");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _services.CreateScope();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                _backlog = await uow.Sincronizaciones.CountPendientesAsync(stoppingToken);
            }
            catch
            {
                // No interrumpir el servicio por errores de métrica
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Cancelación solicitada: salir sin propagar excepción
                break;
            }
        }
    }
}
