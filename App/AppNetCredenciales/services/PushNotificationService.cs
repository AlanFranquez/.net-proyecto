using System;
using System.Text;
using SysDebug = System.Diagnostics.Debug;
using System.Threading.Tasks;

#if ANDROID
using AndroidX.Core.App;
using Android.App;
using Android.Content;
using AndroidX.Core.Content;
using Android.OS;
#endif

namespace AppNetCredenciales.Services
{
    /// <summary>
    /// Servicio principal para mostrar notificaciones nativas de Android
    /// </summary>
    public class PushNotificationService
    {
        private const string CHANNEL_ID = "beneficios_channel";
        private const string CHANNEL_NAME = "Nuevos Beneficios";
        private bool _isInitialized = false;

        public PushNotificationService()
        {
            SysDebug.WriteLine("[PushNotificationService] Servicio inicializado");
        }

        /// <summary>
        /// Inicializa el servicio de notificaciones y configura canales
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                if (_isInitialized) return;

                SysDebug.WriteLine("[PushNotificationService] Inicializando servicio de notificaciones...");

#if ANDROID
                await CreateNotificationChannelAsync();
                await CheckAndRequestPermissionsAsync();
#endif
                
                _isInitialized = true;
                SysDebug.WriteLine("[PushNotificationService] Servicio inicializado exitosamente");
            }
            catch (Exception ex)
            {
                SysDebug.WriteLine($"[PushNotificationService] ERROR inicializando: {ex.Message}");
            }
        }

#if ANDROID
        /// <summary>
        /// Crea el canal de notificaciones para Android 8.0+
        /// </summary>
        private async Task CreateNotificationChannelAsync()
        {
            try
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    var context = Platform.CurrentActivity?.ApplicationContext ?? Android.App.Application.Context;
                    var notificationManager = NotificationManagerCompat.From(context);

                    var channel = new NotificationChannel(CHANNEL_ID, CHANNEL_NAME, NotificationImportance.High)
                    {
                        Description = "Notificaciones de nuevos beneficios disponibles"
                    };

                    // Configurar sonido y vibración
                    channel.EnableVibration(true);
                    channel.SetVibrationPattern(new long[] { 0, 250, 250, 250 });
                    channel.EnableLights(true);
                    channel.LightColor = Android.Graphics.Color.Blue;

                    notificationManager.CreateNotificationChannel(channel);
                    
                    SysDebug.WriteLine("[PushNotificationService] Canal de notificaciones creado");
                }

                await Task.Delay(50); // Pequeña pausa para asegurar creación
            }
            catch (Exception ex)
            {
                SysDebug.WriteLine($"[PushNotificationService] ERROR creando canal: {ex.Message}");
            }
        }

        /// <summary>
        /// Verifica y solicita permisos de notificación para Android 13+
        /// </summary>
        private async Task CheckAndRequestPermissionsAsync()
        {
            try
            {
                SysDebug.WriteLine("[PushNotificationService] Verificando permisos de notificacion...");

                // Android 13+ requiere permiso explícito
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
                {
                    var context = Platform.CurrentActivity ?? Android.App.Application.Context;
                    var permission = Android.Manifest.Permission.PostNotifications;
                    
                    if (ContextCompat.CheckSelfPermission(context, permission) != Android.Content.PM.Permission.Granted)
                    {
                        SysDebug.WriteLine("[PushNotificationService] Solicitando permiso POST_NOTIFICATIONS...");
                        
                        // Si estamos en una Activity, solicitar permiso
                        if (Platform.CurrentActivity is AndroidX.Activity.ComponentActivity activity)
                        {
                            ActivityCompat.RequestPermissions(activity, new string[] { permission }, 1001);
                        }
                        else
                        {
                            SysDebug.WriteLine("[PushNotificationService] No se puede solicitar permiso: Activity no disponible");
                        }
                    }
                    else
                    {
                        SysDebug.WriteLine("[PushNotificationService] Permiso POST_NOTIFICATIONS ya concedido");
                    }
                }

                // Verificar si las notificaciones están habilitadas
                var context2 = Platform.CurrentActivity?.ApplicationContext ?? Android.App.Application.Context;
                var notificationManager = (NotificationManager?)context2.GetSystemService(Context.NotificationService);
                
                if (notificationManager != null)
                {
                    bool areEnabled = notificationManager.AreNotificationsEnabled();
                    SysDebug.WriteLine($"[PushNotificationService] Notificaciones habilitadas: {areEnabled}");
                    
                    if (!areEnabled)
                    {
                        SysDebug.WriteLine("[PushNotificationService] Las notificaciones estan deshabilitadas en configuracion del sistema");
                    }
                }

                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                SysDebug.WriteLine($"[PushNotificationService] ERROR verificando permisos: {ex.Message}");
            }
        }
#endif

        /// <summary>
        /// Muestra una notificación de nuevo beneficio
        /// </summary>
        public async Task ShowNewBeneficioNotificationAsync(string title, string body, int notificationId)
        {
            try
            {
                if (!_isInitialized)
                {
                    await InitializeAsync();
                }

                SysDebug.WriteLine($"[PushNotificationService] Mostrando notificacion: {title}");

#if ANDROID
                await ShowAndroidNotificationAsync(title, body, notificationId);
#endif

                SysDebug.WriteLine($"[PushNotificationService] Notificacion mostrada exitosamente - ID: {notificationId}");
            }
            catch (Exception ex)
            {
                SysDebug.WriteLine($"[PushNotificationService] ERROR mostrando notificacion: {ex.Message}");
            }
        }

#if ANDROID
        /// <summary>
        /// Implementación específica para Android
        /// </summary>
        private async Task ShowAndroidNotificationAsync(string title, string body, int id)
        {
            try
            {
                var context = Platform.CurrentActivity?.ApplicationContext ?? Android.App.Application.Context;
                
                // Crear intent para abrir la app al tocar la notificación
                var intent = new Intent(context, typeof(MainActivity));
                intent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
                
                var pendingIntent = PendingIntent.GetActivity(
                    context, 
                    id, 
                    intent, 
                    PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable
                );

                // Crear la notificación
                var builder = new NotificationCompat.Builder(context, CHANNEL_ID)
                    .SetContentTitle(title)
                    .SetContentText(body)
                    .SetStyle(new NotificationCompat.BigTextStyle().BigText(body))
                    .SetPriority(NotificationCompat.PriorityHigh)
                    .SetAutoCancel(true) // Se quita al tomar
                    .SetContentIntent(pendingIntent)
                    .SetDefaults(NotificationCompat.DefaultAll) // Sonido y vibración por defecto
                    .SetVibrate(new long[] { 0, 250, 250, 250 });

                // Mostrar la notificación
                var notificationManager = NotificationManagerCompat.From(context);
                notificationManager.Notify(id, builder.Build());

                SysDebug.WriteLine($"[PushNotificationService] Notificacion Android mostrada - ID: {id}");
                await Task.Delay(50);
            }
            catch (Exception ex)
            {
                SysDebug.WriteLine($"[PushNotificationService] ERROR en notificacion Android: {ex.Message}");
            }
        }
#endif

        /// <summary>
        /// Muestra una notificación de prueba
        /// </summary>
        public async Task ShowTestNotificationAsync(string message = "Prueba de notificaciones funcionando correctamente")
        {
            try
            {
                await ShowNewBeneficioNotificationAsync(
                    "Notificacion de Prueba",
                    message,
                    9999
                );
            }
            catch (Exception ex)
            {
                SysDebug.WriteLine($"[PushNotificationService] ERROR en notificacion de prueba: {ex.Message}");
            }
        }

        /// <summary>
        /// Verifica si las notificaciones están habilitadas
        /// </summary>
        public bool AreNotificationsEnabled()
        {
            try
            {
#if ANDROID
                var context = Platform.CurrentActivity?.ApplicationContext ?? Android.App.Application.Context;
                var notificationManager = (NotificationManager?)context?.GetSystemService(Context.NotificationService);
                return notificationManager?.AreNotificationsEnabled() ?? false;
#else
                return true; // Asumir habilitado en otras plataformas
#endif
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Obtiene información de diagnóstico
        /// </summary>
        public string GetDiagnosticInfo()
        {
            var info = new StringBuilder();
            info.AppendLine("PUSH NOTIFICATION SERVICE");
            info.AppendLine("------------------------------");
            info.AppendLine($"Inicializado: {_isInitialized}");
            info.AppendLine($"Notificaciones habilitadas: {AreNotificationsEnabled()}");
            info.AppendLine($"Canal ID: {CHANNEL_ID}");
            info.AppendLine($"Canal Nombre: {CHANNEL_NAME}");
            info.AppendLine($"Timestamp: {DateTime.Now:HH:mm:ss}");

#if ANDROID
            info.AppendLine($"Android Version: {Build.VERSION.SdkInt}");
            info.AppendLine($"Soporte POST_NOTIFICATIONS: {Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu}");
#endif

            return info.ToString();
        }
    }
}