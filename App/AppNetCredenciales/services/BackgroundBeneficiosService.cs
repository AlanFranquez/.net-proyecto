using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using AppNetCredenciales.Data;

namespace AppNetCredenciales.Services
{
    /// <summary>
    /// Servicio que se ejecuta en segundo plano para verificar nuevos beneficios
    /// como backup del BeneficiosWatcherService, ejecutándose cada 5 minutos
    /// </summary>
    public class BackgroundBeneficiosService : IDisposable
    {
        private readonly LocalDBService _localDB;
        private readonly PushNotificationService _pushService;
        private readonly System.Timers.Timer _timer;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5); // Verificar cada 5 minutos
        private DateTime _lastCheckTime = DateTime.MinValue;
        private int _lastBeneficiosCount = 0;
        private bool _isRunning = false;

        public BackgroundBeneficiosService(LocalDBService localDB, PushNotificationService pushService)
        {
            _localDB = localDB ?? throw new ArgumentNullException(nameof(localDB));
            _pushService = pushService ?? throw new ArgumentNullException(nameof(pushService));
            
            _timer = new System.Timers.Timer(_checkInterval.TotalMilliseconds)
            {
                AutoReset = true,
                Enabled = false
            };
            _timer.Elapsed += OnTimerElapsed;

            Debug.WriteLine("[BackgroundBeneficios] ?? Servicio de segundo plano inicializado");
        }

        /// <summary>
        /// Inicia el servicio de verificación en segundo plano
        /// </summary>
        public async Task StartAsync()
        {
            try
            {
                if (_isRunning) return;

                Debug.WriteLine("[BackgroundBeneficios] ?? Iniciando servicio de segundo plano...");

                // Obtener estado inicial
                await InitializeBaselineAsync();

                _timer.Start();
                _isRunning = true;

                Debug.WriteLine($"[BackgroundBeneficios] ? Servicio iniciado. Verificando cada {_checkInterval.TotalMinutes} minutos");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BackgroundBeneficios] ? Error iniciando servicio: {ex.Message}");
            }
        }

        /// <summary>
        /// Inicializa el estado base para detectar cambios
        /// </summary>
        private async Task InitializeBaselineAsync()
        {
            try
            {
                var beneficios = await _localDB.GetBeneficiosAsync();
                _lastBeneficiosCount = beneficios?.Count ?? 0;
                _lastCheckTime = DateTime.Now;

                Debug.WriteLine($"[BackgroundBeneficios] ?? Estado inicial: {_lastBeneficiosCount} beneficios");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BackgroundBeneficios] ? Error inicializando baseline: {ex.Message}");
            }
        }

        /// <summary>
        /// Se ejecuta cada vez que el timer se dispara
        /// </summary>
        private async void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            try
            {
                Debug.WriteLine($"[BackgroundBeneficios] ?? Verificando nuevos beneficios... (última verificación: {_lastCheckTime:HH:mm:ss})");

                await CheckForNewBeneficiosAsync();
                
                _lastCheckTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BackgroundBeneficios] ? Error en verificación periódica: {ex.Message}");
            }
        }

        /// <summary>
        /// Verifica si hay nuevos beneficios disponibles
        /// </summary>
        private async Task CheckForNewBeneficiosAsync()
        {
            try
            {
                // Sincronizar beneficios desde el API
                var beneficios = await _localDB.SincronizarBeneficiosFromBack();
                
                if (beneficios == null)
                {
                    Debug.WriteLine("[BackgroundBeneficios] ?? No se pudieron sincronizar beneficios");
                    return;
                }

                int currentCount = beneficios.Count;
                Debug.WriteLine($"[BackgroundBeneficios] ?? Beneficios actuales: {currentCount}, Anteriores: {_lastBeneficiosCount}");

                // Verificar si hay nuevos beneficios
                if (currentCount > _lastBeneficiosCount)
                {
                    int newBeneficiosCount = currentCount - _lastBeneficiosCount;
                    Debug.WriteLine($"[BackgroundBeneficios] ?? ¡{newBeneficiosCount} nuevo(s) beneficio(s) detectado(s)!");

                    // Obtener los nuevos beneficios (los más recientes)
                    var newBeneficios = beneficios
                        .OrderByDescending(b => b.BeneficioId)
                        .Take(newBeneficiosCount)
                        .ToList();

                    // Mostrar notificación para cada nuevo beneficio
                    foreach (var beneficio in newBeneficios)
                    {
                        await ShowNewBeneficioNotificationAsync(beneficio);
                    }

                    _lastBeneficiosCount = currentCount;
                }
                else if (currentCount < _lastBeneficiosCount)
                {
                    // Algunos beneficios fueron eliminados, actualizar contador
                    Debug.WriteLine($"[BackgroundBeneficios] ?? Algunos beneficios fueron eliminados. Actualizando contador.");
                    _lastBeneficiosCount = currentCount;
                }
                else
                {
                    Debug.WriteLine("[BackgroundBeneficios] ? No hay nuevos beneficios");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BackgroundBeneficios] ? Error verificando nuevos beneficios: {ex.Message}");
            }
        }

        /// <summary>
        /// Muestra notificación de nuevo beneficio
        /// </summary>
        private async Task ShowNewBeneficioNotificationAsync(AppNetCredenciales.models.Beneficio beneficio)
        {
            try
            {
                Debug.WriteLine($"[BackgroundBeneficios] ?? Mostrando notificación para beneficio: {beneficio.Nombre}");

                // Crear notificación local
                string title = "?? ¡Nuevo Beneficio Disponible!";
                
                // Crear descripción truncada
                int maxLength = Math.Min(50, beneficio.Descripcion?.Length ?? 0);
                string truncatedDescription = maxLength > 0 
                    ? beneficio.Descripcion?.Substring(0, maxLength) ?? ""
                    : "";
                    
                string body = $"{beneficio.Nombre}";
                if (!string.IsNullOrEmpty(truncatedDescription))
                {
                    body += $" - {truncatedDescription}...";
                }

                body += "\n?? Detectado desde sincronización local";

                await _pushService.ShowNewBeneficioNotificationAsync(title, body, beneficio.BeneficioId);

                Debug.WriteLine("[BackgroundBeneficios] ? Notificación mostrada");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BackgroundBeneficios] ? Error mostrando notificación: {ex.Message}");
            }
        }

        /// <summary>
        /// Fuerza una verificación inmediata
        /// </summary>
        public async Task CheckNowAsync()
        {
            try
            {
                Debug.WriteLine("[BackgroundBeneficios] ? Verificación manual solicitada...");
                await CheckForNewBeneficiosAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BackgroundBeneficios] ? Error en verificación manual: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene estadísticas del servicio de background
        /// </summary>
        public string ObtenerEstadisticas()
        {
            var info = new System.Text.StringBuilder();
            info.AppendLine("?? BACKGROUND SERVICE STATS");
            info.AppendLine("???????????????????????????");
            info.AppendLine($"?? Último conteo: {_lastBeneficiosCount} beneficios");
            info.AppendLine($"?? Intervalo: {_checkInterval.TotalMinutes} minutos");
            info.AppendLine($"?? Servidor: https://ec07fc17d79e.ngrok-free.app/api/");
            info.AppendLine($"?? Estado: {(_isRunning ? "Activo" : "Detenido")}");
            info.AppendLine($"? Última verificación: {_lastCheckTime:dd/MM HH:mm:ss}");
            info.AppendLine($"?? Fuente: LocalDB + Sincronización API");
            info.AppendLine($"?? Optimizado para batería: ?");

            return info.ToString();
        }

        /// <summary>
        /// Detiene el servicio
        /// </summary>
        public void Stop()
        {
            try
            {
                if (_isRunning)
                {
                    _timer.Stop();
                    _isRunning = false;
                    Debug.WriteLine("[BackgroundBeneficios] ?? Servicio detenido");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BackgroundBeneficios] ? Error deteniendo servicio: {ex.Message}");
            }
        }

        /// <summary>
        /// Reinicia el servicio
        /// </summary>
        public void Start()
        {
            try
            {
                if (!_isRunning)
                {
                    _timer.Start();
                    _isRunning = true;
                    Debug.WriteLine("[BackgroundBeneficios] ?? Servicio reiniciado");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BackgroundBeneficios] ? Error reiniciando servicio: {ex.Message}");
            }
        }

        /// <summary>
        /// Libera recursos del servicio
        /// </summary>
        public void Dispose()
        {
            try
            {
                Stop();
                _timer?.Dispose();
                
                Debug.WriteLine("[BackgroundBeneficios] ?? Recursos liberados");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BackgroundBeneficios] ? Error liberando recursos: {ex.Message}");
            }
        }
    }
}