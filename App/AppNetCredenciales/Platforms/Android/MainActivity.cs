using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Content;
using AppNetCredenciales.Services;
using Microsoft.Extensions.DependencyInjection;
using Plugin.NFC;

namespace AppNetCredenciales
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            // ═══════════════════════════════════════════════════════════
            // CRÍTICO: Inicializar Plugin.NFC
            // ═══════════════════════════════════════════════════════════
            try
            {
                CrossNFC.Init(this);
                System.Diagnostics.Debug.WriteLine("[MainActivity] ✅ Plugin.NFC inicializado correctamente");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainActivity] ❌ Error inicializando Plugin.NFC: {ex.Message}");
            }
            
            // INICIALIZAR SERVICIOS DE NOTIFICACIONES PARA ANDROID
            InitializeNotificationServices();
        }

        protected override void OnResume()
        {
            base.OnResume();
            
            // Reanudar Plugin.NFC cuando la app vuelve al primer plano
            try
            {
                CrossNFC.OnResume();
                System.Diagnostics.Debug.WriteLine("[MainActivity] ✅ Plugin.NFC resumido");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainActivity] ⚠️ Error resumiendo Plugin.NFC: {ex.Message}");
            }
        }

        protected override void OnNewIntent(Intent? intent)
        {
            base.OnNewIntent(intent);
            
            // Manejar intents NFC
            try
            {
                if (intent == null) return;

                var action = intent.Action;
                System.Diagnostics.Debug.WriteLine($"[MainActivity] ✅ OnNewIntent - Action: {action}");

                // Verificar si es un intent NFC
                if (Android.Nfc.NfcAdapter.ActionNdefDiscovered.Equals(action) ||
                    Android.Nfc.NfcAdapter.ActionTechDiscovered.Equals(action) ||
                    Android.Nfc.NfcAdapter.ActionTagDiscovered.Equals(action))
                {
                    var nfcService = App.Services?.GetService<NfcService>();
                    if (nfcService == null)
                    {
                        System.Diagnostics.Debug.WriteLine("[MainActivity] ⚠️ NfcService no disponible");
                        return;
                    }

                    System.Diagnostics.Debug.WriteLine("[MainActivity] 📡 Tag NFC detectado - Procesando...");

                    // Si estamos en modo escritura NDEF, manejar escritura
                    if (nfcService.IsWritingMode)
                    {
                        System.Diagnostics.Debug.WriteLine("[MainActivity] 📝 Modo escritura NDEF - Procesando...");
                        
                        _ = Task.Run(async () =>
                        {
                            try
                            {
#if ANDROID
                                var success = await nfcService.WriteNdefToTag(intent);
                                
                                if (success)
                                {
                                    System.Diagnostics.Debug.WriteLine("[MainActivity] ✅ Escritura NDEF completada");
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine("[MainActivity] ⚠️ Escritura NDEF falló");
                                }
#endif
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"[MainActivity] ❌ Error en escritura NDEF: {ex.Message}");
                            }
                        });
                    }
                    // Si estamos en modo lectura, usar lectura nativa con prioridad Mifare
                    else if (nfcService.IsListening)
                    {
                        System.Diagnostics.Debug.WriteLine("[MainActivity] 📖 Modo lectura - Intentando lectura nativa...");
                        
                        _ = Task.Run(async () =>
                        {
                            try
                            {
#if ANDROID
                                // Intentar lectura nativa (Mifare > NfcA > NDEF)
                                var data = await nfcService.ReadNativeTagAsync(intent);
                                
                                if (!string.IsNullOrEmpty(data))
                                {
                                    System.Diagnostics.Debug.WriteLine($"[MainActivity] ✅ Lectura nativa completada: {data}");
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine("[MainActivity] ⚠️ Lectura nativa no retornó datos, usando Plugin.NFC");
                                    // Dejar que Plugin.NFC maneje el intent como fallback
                                    CrossNFC.OnNewIntent(intent);
                                }
#else
                                // En otras plataformas, usar Plugin.NFC
                                CrossNFC.OnNewIntent(intent);
#endif
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"[MainActivity] ❌ Error en lectura: {ex.Message}");
                                // Fallback a Plugin.NFC
                                CrossNFC.OnNewIntent(intent);
                            }
                        });
                    }
                    else
                    {
                        // Si no estamos en ningún modo específico, usar Plugin.NFC
                        System.Diagnostics.Debug.WriteLine("[MainActivity] 📱 Usando Plugin.NFC (modo por defecto)");
                        CrossNFC.OnNewIntent(intent);
                    }
                }
                else
                {
                    // No es un intent NFC, pero dejamos que Plugin.NFC lo revise
                    CrossNFC.OnNewIntent(intent);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainActivity] ⚠️ Error manejando intent NFC: {ex.Message}");
            }
        }

        /// <summary>
        /// Inicializa servicios específicos de Android para notificaciones
        /// </summary>
        private async void InitializeNotificationServices()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[MainActivity] Inicializando servicios Android...");

                // Obtener servicio de notificaciones
                var pushService = App.Services?.GetService<AppNetCredenciales.Services.PushNotificationService>();
                if (pushService != null)
                {
                    await pushService.InitializeAsync();
                    System.Diagnostics.Debug.WriteLine("[MainActivity] PushNotificationService Android inicializado");
                }

                // Verificar permisos de notificación
                CheckNotificationPermissions();

                System.Diagnostics.Debug.WriteLine("[MainActivity] Configuracion Android completada");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainActivity] ERROR inicializando servicios Android: {ex.Message}");
            }
        }

        /// <summary>
        /// Verifica permisos de notificación en Android
        /// </summary>
        private void CheckNotificationPermissions()
        {
            try
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu) // Android 13+
                {
                    var permission = Android.Manifest.Permission.PostNotifications;
                    
                    if (AndroidX.Core.Content.ContextCompat.CheckSelfPermission(this, permission) 
                        != Android.Content.PM.Permission.Granted)
                    {
                        System.Diagnostics.Debug.WriteLine("[MainActivity] Solicitando permiso POST_NOTIFICATIONS...");
                        
                        AndroidX.Core.App.ActivityCompat.RequestPermissions(
                            this, 
                            new string[] { permission }, 
                            1001
                        );
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("[MainActivity] Permiso POST_NOTIFICATIONS concedido");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[MainActivity] Android < 13: No requiere permiso POST_NOTIFICATIONS");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainActivity] ERROR verificando permisos: {ex.Message}");
            }
        }

        /// <summary>
        /// Maneja respuesta de solicitud de permisos
        /// </summary>
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                if (requestCode == 1001) // POST_NOTIFICATIONS
                {
                    if (grantResults.Length > 0 && grantResults[0] == Android.Content.PM.Permission.Granted)
                    {
                        System.Diagnostics.Debug.WriteLine("[MainActivity] Permiso POST_NOTIFICATIONS concedido por usuario");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("[MainActivity] Permiso POST_NOTIFICATIONS denegado por usuario");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainActivity] ERROR manejando respuesta de permisos: {ex.Message}");
            }
        }
    }
}
