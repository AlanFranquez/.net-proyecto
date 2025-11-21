using System;
using System.Threading.Tasks;

namespace AppNetCredenciales.Services
{
    /// <summary>
    /// Administrador del estado HCE (Host Card Emulation)
    /// </summary>
    public class HceManager
    {
        private string? _activeCredentialId;
        private bool _isHceActive;
        private DateTime? _activationTime;

        public event EventHandler<HceStateChangedEventArgs>? HceStateChanged;

        /// <summary>
        /// Activa HCE con una credencial específica
        /// </summary>
        public async Task<bool> ActivateHceAsync(string credencialId)
        {
            try
            {
                if (string.IsNullOrEmpty(credencialId))
                {
                    System.Diagnostics.Debug.WriteLine("[HceManager] Error: credencialId vacío");
                    return false;
                }

#if ANDROID
                // Verificar si NFC está disponible y habilitado
                var context = Android.App.Application.Context;
                var nfcAdapter = Android.Nfc.NfcAdapter.GetDefaultAdapter(context);
                
                if (nfcAdapter == null)
                {
                    System.Diagnostics.Debug.WriteLine("[HceManager] NFC no disponible en este dispositivo");
                    return false;
                }

                if (!nfcAdapter.IsEnabled)
                {
                    System.Diagnostics.Debug.WriteLine("[HceManager] NFC está deshabilitado");
                    
                    // Mostrar diálogo para activar NFC
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        bool shouldOpenSettings = await App.Current.MainPage.DisplayAlert(
                            "NFC Deshabilitado",
                            "Para usar la credencial NFC, debes habilitar NFC en la configuración de tu dispositivo.\n\n¿Deseas abrir la configuración ahora?",
                            "Sí",
                            "No");

                        if (shouldOpenSettings)
                        {
                            var intent = new Android.Content.Intent(Android.Provider.Settings.ActionNfcSettings);
                            intent.AddFlags(Android.Content.ActivityFlags.NewTask);
                            context.StartActivity(intent);
                        }
                    });
                    
                    return false;
                }

                // Verificar que el servicio HCE esté disponible
                var cardEmulation = Android.Nfc.CardEmulators.CardEmulation.GetInstance(nfcAdapter);
                if (cardEmulation == null)
                {
                    System.Diagnostics.Debug.WriteLine("[HceManager] CardEmulation no disponible");
                    return false;
                }

                // Obtener el ComponentName del HceService
                var serviceName = new Android.Content.ComponentName(
                    context,
                    Java.Lang.Class.FromType(typeof(Platforms.Android.HceService)));

                // Verificar si nuestro servicio está disponible
                var category = "other"; // Debe coincidir con el XML
                
                System.Diagnostics.Debug.WriteLine($"[HceManager] Verificando servicio HCE: {serviceName.FlattenToString()}");
                
                // Intentar establecer como servicio preferido
                try
                {
                    // Mostrar mensaje al usuario sobre cómo configurar
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await App.Current.MainPage.DisplayAlert(
                            "Configurar HCE",
                            "Para usar la credencial NFC:\n\n" +
                            "1. Ve a Configuración de Android\n" +
                            "2. Busca 'NFC' o 'Pagos sin contacto'\n" +
                            "3. Selecciona 'AppNetCredenciales' como aplicación de pago predeterminada\n\n" +
                            "La credencial quedará activa en esta app.",
                            "Entendido");
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[HceManager] Error mostrando instrucciones: {ex.Message}");
                }

                System.Diagnostics.Debug.WriteLine($"[HceManager] Activando HCE con credencial: {credencialId}");
                _activeCredentialId = credencialId;
                _isHceActive = true;
                _activationTime = DateTime.Now;

                // Notificar cambio de estado
                HceStateChanged?.Invoke(this, new HceStateChangedEventArgs 
                { 
                    IsActive = true, 
                    CredencialId = credencialId 
                });

                return true;
#else
                await Task.Delay(10);
                System.Diagnostics.Debug.WriteLine("[HceManager] HCE solo disponible en Android");
                return false;
#endif
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HceManager] Error activando HCE: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Desactiva HCE
        /// </summary>
        public void DeactivateHce()
        {
            System.Diagnostics.Debug.WriteLine("[HceManager] Desactivando HCE");
            
            _activeCredentialId = null;
            _isHceActive = false;
            _activationTime = null;

            HceStateChanged?.Invoke(this, new HceStateChangedEventArgs 
            { 
                IsActive = false 
            });
        }

        /// <summary>
        /// Obtiene el ID de la credencial activa
        /// </summary>
        public string? GetActiveCredentialId()
        {
            return _isHceActive ? _activeCredentialId : null;
        }

        /// <summary>
        /// Verifica si HCE está activo
        /// </summary>
        public bool IsHceActive => _isHceActive;

        /// <summary>
        /// Tiempo desde que se activó HCE
        /// </summary>
        public TimeSpan? TimeSinceActivation
        {
            get
            {
                if (_activationTime.HasValue && _isHceActive)
                    return DateTime.Now - _activationTime.Value;
                return null;
            }
        }

        /// <summary>
        /// Verifica si el dispositivo soporta HCE
        /// </summary>
        public async Task<bool> IsHceAvailableAsync()
        {
            try
            {
#if ANDROID
                await Task.Delay(10);
                var context = Android.App.Application.Context;
                var nfcAdapter = Android.Nfc.NfcAdapter.GetDefaultAdapter(context);
                
                if (nfcAdapter == null)
                    return false;

                // Verificar si el dispositivo soporta HCE
                var packageManager = context.PackageManager;
                return packageManager?.HasSystemFeature(Android.Content.PM.PackageManager.FeatureNfcHostCardEmulation) ?? false;
#else
                await Task.Delay(10);
                return false;
#endif
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HceManager] Error verificando HCE: {ex.Message}");
                return false;
            }
        }
    }

    /// <summary>
    /// Argumentos del evento de cambio de estado HCE
    /// </summary>
    public class HceStateChangedEventArgs : EventArgs
    {
        public bool IsActive { get; set; }
        public string? CredencialId { get; set; }
    }
}
