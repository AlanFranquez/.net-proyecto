using Microsoft.Maui.ApplicationModel;
using System;
using System.Threading.Tasks;

#if ANDROID
using Android.Hardware.Biometrics;
using Android.Content;
using Android.OS;
using Java.Util.Concurrent;
using AndroidX.Core.Content;
#endif

namespace AppNetCredenciales.Services
{
    /// <summary>
    /// Servicio para autenticación biométrica (huella digital, reconocimiento facial)
    /// </summary>
    public class BiometricService
    {
#if ANDROID
        private BiometricPrompt? _biometricPrompt;
        private BiometricPrompt.Builder? _promptBuilder;
        private TaskCompletionSource<BiometricResult>? _authenticationTcs;
#endif

        /// <summary>
        /// Verifica si el dispositivo soporta autenticación biométrica
        /// </summary>
        public async Task<bool> IsBiometricAvailableAsync()
        {
            try
            {
#if ANDROID
                await Task.Delay(10); // Para mantener async
                
                var activity = Platform.CurrentActivity;
                if (activity == null)
                {
                    System.Diagnostics.Debug.WriteLine("[BiometricService] No current activity");
                    return false;
                }

                // Verificar si hay huellas registradas
                if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
                {
                    var fingerprintManager = activity.GetSystemService(Context.FingerprintService) as Android.Hardware.Fingerprints.FingerprintManager;
                    
                    if (fingerprintManager == null)
                    {
                        System.Diagnostics.Debug.WriteLine("[BiometricService] FingerprintManager not available");
                        return false;
                    }

                    bool hasHardware = fingerprintManager.IsHardwareDetected;
                    bool hasEnrolledFingerprints = fingerprintManager.HasEnrolledFingerprints;

                    System.Diagnostics.Debug.WriteLine($"[BiometricService] HasHardware: {hasHardware}, HasEnrolled: {hasEnrolledFingerprints}");
                    
                    return hasHardware && hasEnrolledFingerprints;
                }

                return false;
#else
                return await Task.FromResult(false); // iOS/Windows no implementado aún
#endif
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BiometricService] Error checking availability: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Solicita autenticación biométrica al usuario
        /// </summary>
        /// <param name="reason">Razón de la solicitud (se muestra al usuario)</param>
        /// <returns>True si la autenticación fue exitosa</returns>
        public async Task<BiometricResult> AuthenticateAsync(string reason = "Verificar identidad")
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[BiometricService] AuthenticateAsync called - Reason: {reason}");
                
                bool isAvailable = await IsBiometricAvailableAsync();
                
                if (!isAvailable)
                {
                    System.Diagnostics.Debug.WriteLine("[BiometricService] Biometric not available");
                    return new BiometricResult
                    {
                        Success = false,
                        ErrorMessage = "La autenticación biométrica no está disponible. Verifica que tengas huella dactilar configurada en tu dispositivo."
                    };
                }

#if ANDROID
                return await AuthenticateAndroidAsync(reason);
#else
                // Para otras plataformas, retornar error por ahora
                return new BiometricResult
                {
                    Success = false,
                    ErrorMessage = "Autenticación biométrica no soportada en esta plataforma"
                };
#endif
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BiometricService] Authentication error: {ex}");
                return new BiometricResult
                {
                    Success = false,
                    ErrorMessage = $"Error durante la autenticación: {ex.Message}"
                };
            }
        }

#if ANDROID
        private async Task<BiometricResult> AuthenticateAndroidAsync(string reason)
        {
            try
            {
                var activity = Platform.CurrentActivity;
                if (activity == null)
                {
                    return new BiometricResult
                    {
                        Success = false,
                        ErrorMessage = "No se pudo obtener la actividad actual"
                    };
                }

                // Verificar versión de Android
                if (Build.VERSION.SdkInt < BuildVersionCodes.P)
                {
                    return new BiometricResult
                    {
                        Success = false,
                        ErrorMessage = "La autenticación biométrica requiere Android 9.0 (API 28) o superior"
                    };
                }

                _authenticationTcs = new TaskCompletionSource<BiometricResult>();

                // Crear el callback
                var callback = new BiometricAuthenticationCallback(this);

                // Crear el prompt usando BiometricPrompt.Builder
                _promptBuilder = new BiometricPrompt.Builder(activity)
                    .SetTitle("Autenticación Requerida")
                    .SetSubtitle(reason)
                    .SetDescription("Coloca tu dedo en el sensor de huellas")
                    .SetNegativeButton(
                        "Cancelar",
                        ContextCompat.GetMainExecutor(activity)!,
                        new CancelDialogClickListener(this)
                    );

                _biometricPrompt = _promptBuilder.Build();

                // Ejecutar en el hilo principal
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    try
                    {
                        var cancellationSignal = new Android.OS.CancellationSignal();
                        var executor = ContextCompat.GetMainExecutor(activity);
                        
                        _biometricPrompt?.Authenticate(cancellationSignal, executor!, callback);
                        System.Diagnostics.Debug.WriteLine("[BiometricService] Biometric prompt shown");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[BiometricService] Error showing prompt: {ex}");
                        _authenticationTcs?.TrySetResult(new BiometricResult
                        {
                            Success = false,
                            ErrorMessage = $"Error al mostrar prompt: {ex.Message}"
                        });
                    }
                });

                // Esperar resultado con timeout de 60 segundos
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(60));
                var completedTask = await Task.WhenAny(_authenticationTcs.Task, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    return new BiometricResult
                    {
                        Success = false,
                        ErrorMessage = "Tiempo de espera agotado"
                    };
                }

                return await _authenticationTcs.Task;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BiometricService] AuthenticateAndroidAsync error: {ex}");
                return new BiometricResult
                {
                    Success = false,
                    ErrorMessage = $"Error: {ex.Message}"
                };
            }
        }

        internal void OnAuthenticationSucceeded()
        {
            System.Diagnostics.Debug.WriteLine("[BiometricService] ? Authentication SUCCEEDED");
            _authenticationTcs?.TrySetResult(new BiometricResult
            {
                Success = true,
                ErrorMessage = null
            });
        }

        internal void OnAuthenticationFailed()
        {
            System.Diagnostics.Debug.WriteLine("[BiometricService] ? Authentication FAILED");
            // No completar aquí, esperar a que el usuario cancele o lo intente de nuevo
        }

        internal void OnAuthenticationError(BiometricErrorCode errorCode, string errorMessage)
        {
            System.Diagnostics.Debug.WriteLine($"[BiometricService] ? Authentication ERROR: {errorCode} - {errorMessage}");
            
            string userMessage = errorCode switch
            {
                BiometricErrorCode.Canceled => "Autenticación cancelada por el usuario",
                BiometricErrorCode.Lockout => "Demasiados intentos. Sensor bloqueado temporalmente",
                BiometricErrorCode.LockoutPermanent => "Sensor bloqueado permanentemente. Usa PIN/contraseña del dispositivo",
                BiometricErrorCode.NoSpace => "No hay espacio disponible",
                BiometricErrorCode.Timeout => "Tiempo de espera agotado",
                BiometricErrorCode.UnableToProcess => "No se pudo procesar la huella",
                BiometricErrorCode.UserCanceled => "Usuario canceló la operación",
                BiometricErrorCode.NoBiometrics => "No hay huellas registradas en el dispositivo",
                BiometricErrorCode.HwNotPresent => "Hardware biométrico no disponible",
                BiometricErrorCode.HwUnavailable => "Hardware biométrico no disponible temporalmente",
                _ => $"Error de autenticación: {errorMessage}"
            };

            _authenticationTcs?.TrySetResult(new BiometricResult
            {
                Success = false,
                ErrorMessage = userMessage
            });
        }

        internal void OnCanceled()
        {
            System.Diagnostics.Debug.WriteLine("[BiometricService] ? User pressed Cancel button");
            _authenticationTcs?.TrySetResult(new BiometricResult
            {
                Success = false,
                ErrorMessage = "Usuario canceló la autenticación"
            });
        }

        // Clase interna para el callback
        private class BiometricAuthenticationCallback : BiometricPrompt.AuthenticationCallback
        {
            private readonly BiometricService _service;

            public BiometricAuthenticationCallback(BiometricService service)
            {
                _service = service;
            }

            public override void OnAuthenticationSucceeded(BiometricPrompt.AuthenticationResult? result)
            {
                base.OnAuthenticationSucceeded(result);
                _service.OnAuthenticationSucceeded();
            }

            public override void OnAuthenticationFailed()
            {
                base.OnAuthenticationFailed();
                _service.OnAuthenticationFailed();
            }

            public override void OnAuthenticationError(BiometricErrorCode errorCode, Java.Lang.ICharSequence? errString)
            {
                base.OnAuthenticationError(errorCode, errString);
                _service.OnAuthenticationError(errorCode, errString?.ToString() ?? "Error desconocido");
            }
        }

        // Clase para manejar el click del botón cancelar
        private class CancelDialogClickListener : Java.Lang.Object, IDialogInterfaceOnClickListener
        {
            private readonly BiometricService _service;

            public CancelDialogClickListener(BiometricService service)
            {
                _service = service;
            }

            public void OnClick(IDialogInterface? dialog, int which)
            {
                _service.OnCanceled();
            }
        }
#endif
    }

    /// <summary>
    /// Resultado de la autenticación biométrica
    /// </summary>
    public class BiometricResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
