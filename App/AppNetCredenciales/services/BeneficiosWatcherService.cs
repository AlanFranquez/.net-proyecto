using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AppNetCredenciales.Services
{
    /// <summary>
    /// Servicio que monitorea la creación de nuevos beneficios desde el servidor
    /// y envía notificaciones automáticamente cada 30 segundos
    /// </summary>
    public class BeneficiosWatcherService : IDisposable
    {
        private readonly ApiService _apiService;
        private readonly PushNotificationService _pushService;
        private readonly Timer _timer;
        private readonly HashSet<string> _beneficiosConocidos;
        private bool _isFirstRun = true;
        private bool _isRunning = false;

        public BeneficiosWatcherService(ApiService apiService, PushNotificationService pushService)
        {
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            _pushService = pushService ?? throw new ArgumentNullException(nameof(pushService));
            _beneficiosConocidos = new HashSet<string>();
            
            Debug.WriteLine("[BeneficiosWatcher] ?? Servicio de monitoreo iniciado");
            
            // Timer que ejecuta cada 30 segundos
            _timer = new Timer(CheckForNewBeneficios, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
            _isRunning = true;
        }

        /// <summary>
        /// Verifica si hay nuevos beneficios y envía notificaciones
        /// </summary>
        private async void CheckForNewBeneficios(object? state)
        {
            try
            {
                Debug.WriteLine("[BeneficiosWatcher] ?? Verificando nuevos beneficios...");

                var beneficios = await _apiService.GetBeneficiosAsync();
                
                if (beneficios == null || beneficios.Count == 0)
                {
                    Debug.WriteLine("[BeneficiosWatcher] ?? No hay beneficios disponibles");
                    return;
                }

                // En la primera ejecución, solo cargar los IDs existentes
                if (_isFirstRun)
                {
                    foreach (var beneficio in beneficios)
                    {
                        if (!string.IsNullOrEmpty(beneficio.Id))
                            _beneficiosConocidos.Add(beneficio.Id);
                    }
                    _isFirstRun = false;
                    Debug.WriteLine($"[BeneficiosWatcher] ?? Inicializado con {_beneficiosConocidos.Count} beneficios existentes");
                    return;
                }

                // Verificar nuevos beneficios
                var nuevosBeneficios = beneficios
                    .Where(b => !string.IsNullOrEmpty(b.Id) && !_beneficiosConocidos.Contains(b.Id))
                    .ToList();

                if (nuevosBeneficios.Any())
                {
                    Debug.WriteLine($"[BeneficiosWatcher] ?? ¡{nuevosBeneficios.Count} nuevo(s) beneficio(s) detectado(s)!");

                    foreach (var beneficio in nuevosBeneficios)
                    {
                        // Agregar a la lista de conocidos
                        _beneficiosConocidos.Add(beneficio.Id);

                        // Enviar notificación
                        await EnviarNotificacionBeneficio(beneficio);
                    }
                }
                else
                {
                    Debug.WriteLine("[BeneficiosWatcher] ? No hay nuevos beneficios");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BeneficiosWatcher] ? Error verificando beneficios: {ex.Message}");
            }
        }

        /// <summary>
        /// Envía notificación para un beneficio específico
        /// </summary>
        private async Task EnviarNotificacionBeneficio(ApiService.BeneficioDto beneficio)
        {
            try
            {
                Debug.WriteLine($"[BeneficiosWatcher] ?? Enviando notificación para beneficio: {beneficio.Id} - {beneficio.Nombre}");

                // Inicializar servicio de notificaciones si es necesario
                await _pushService.InitializeAsync();

                // Convertir ID de manera segura
                int notificationId = 1000;
                if (int.TryParse(beneficio.Id, out int parsed))
                {
                    notificationId = parsed;
                }
                else
                {
                    // Si no es numérico, usar hash del ID
                    notificationId = Math.Abs(beneficio.Id.GetHashCode()) % 9000 + 1000; // Entre 1000-9999
                }

                // Crear mensaje de notificación
                var titulo = $"?? ¡Nuevo Beneficio: {beneficio.Nombre}!";
                var mensaje = $"?? {beneficio.Descripcion}";

                // Agregar fecha de vigencia si está disponible
                if (beneficio.VigenciaFin != default)
                {
                    mensaje += $"\n?? Válido hasta: {beneficio.VigenciaFin:dd/MM/yyyy}";
                }

                mensaje += "\n?? Detectado automáticamente";

                // Enviar notificación
                await _pushService.ShowNewBeneficioNotificationAsync(titulo, mensaje, notificationId);

                Debug.WriteLine($"[BeneficiosWatcher] ? Notificación enviada exitosamente para beneficio {beneficio.Id}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BeneficiosWatcher] ? Error enviando notificación: {ex.Message}");
            }
        }

        /// <summary>
        /// Fuerza una verificación inmediata de nuevos beneficios
        /// </summary>
        public async Task ForzarVerificacionAsync()
        {
            try
            {
                Debug.WriteLine("[BeneficiosWatcher] ? Verificación forzada solicitada");
                await Task.Run(() => CheckForNewBeneficios(null));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BeneficiosWatcher] ? Error en verificación forzada: {ex.Message}");
            }
        }

        /// <summary>
        /// Reinicia el contador de beneficios conocidos
        /// </summary>
        public void ResetBeneficiosConocidos()
        {
            try
            {
                _beneficiosConocidos.Clear();
                _isFirstRun = true;
                Debug.WriteLine("[BeneficiosWatcher] ?? Lista de beneficios conocidos reseteada");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BeneficiosWatcher] ? Error reseteando: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene estadísticas del servicio
        /// </summary>
        public string ObtenerEstadisticas()
        {
            var info = new System.Text.StringBuilder();
            info.AppendLine("?? BENEFICIOS WATCHER STATS");
            info.AppendLine("???????????????????????????");
            info.AppendLine($"?? Beneficios conocidos: {_beneficiosConocidos.Count}");
            info.AppendLine($"?? Intervalo de verificación: 30 segundos");
            info.AppendLine($"?? Servidor: https://ec07fc17d79e.ngrok-free.app/api/");
            info.AppendLine($"?? Estado: {(_isRunning ? "Activo" : "Detenido")}");
            info.AppendLine($"?? Primera ejecución: {(_isFirstRun ? "Sí" : "No")}");
            info.AppendLine($"? Última verificación: {DateTime.Now:HH:mm:ss}");
            
            if (_beneficiosConocidos.Count > 0)
            {
                info.AppendLine($"?? IDs conocidos: {string.Join(", ", _beneficiosConocidos.Take(5))}");
                if (_beneficiosConocidos.Count > 5)
                {
                    info.AppendLine($"   ... y {_beneficiosConocidos.Count - 5} más");
                }
            }

            return info.ToString();
        }

        /// <summary>
        /// Detiene el servicio de monitoreo
        /// </summary>
        public void Stop()
        {
            try
            {
                if (_isRunning)
                {
                    _timer?.Change(Timeout.Infinite, Timeout.Infinite);
                    _isRunning = false;
                    Debug.WriteLine("[BeneficiosWatcher] ?? Servicio detenido");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BeneficiosWatcher] ? Error deteniendo servicio: {ex.Message}");
            }
        }

        /// <summary>
        /// Reinicia el servicio de monitoreo
        /// </summary>
        public void Start()
        {
            try
            {
                if (!_isRunning)
                {
                    _timer?.Change(TimeSpan.Zero, TimeSpan.FromSeconds(30));
                    _isRunning = true;
                    Debug.WriteLine("[BeneficiosWatcher] ?? Servicio reiniciado");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BeneficiosWatcher] ? Error reiniciando servicio: {ex.Message}");
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
                _beneficiosConocidos.Clear();
                
                Debug.WriteLine("[BeneficiosWatcher] ?? Recursos liberados");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BeneficiosWatcher] ? Error liberando recursos: {ex.Message}");
            }
        }
    }
}