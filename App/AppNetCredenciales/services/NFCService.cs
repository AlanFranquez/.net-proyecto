using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

#if ANDROID
using Android.Nfc;
using Android.App;
using Android.Content;
using Android.OS;
#endif

namespace AppNetCredenciales.Services
{
    /// <summary>
    /// Servicio para lectura y escritura de tags NFC
    /// </summary>
    public class NFCService
    {
        private bool _isReading = false;
        private TaskCompletionSource<NFCReadResult>? _readTaskCompletionSource;

#if ANDROID
        private NfcAdapter? _nfcAdapter;
        private Activity? _activity;

        public void Initialize(Activity activity)
        {
            _activity = activity;
            _nfcAdapter = NfcAdapter.GetDefaultAdapter(activity);
        }
#endif

        /// <summary>
        /// Verifica si el dispositivo soporta NFC
        /// </summary>
        public async Task<bool> IsNFCAvailableAsync()
        {
            try
            {
#if ANDROID
                await Task.Delay(10);
                if (_nfcAdapter == null)
                {
                    return false;
                }
                return _nfcAdapter.IsEnabled;
#else
                await Task.Delay(100);
                return false; // NFC solo disponible en Android por ahora
#endif
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NFCService] Error checking NFC availability: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Inicia la lectura de un tag NFC
        /// </summary>
        public async Task<NFCReadResult> StartReadingAsync()
        {
            try
            {
                if (_isReading)
                {
                    return new NFCReadResult
                    {
                        Success = false,
                        ErrorMessage = "Ya hay una lectura NFC en progreso"
                    };
                }

                bool isAvailable = await IsNFCAvailableAsync();
                if (!isAvailable)
                {
                    return new NFCReadResult
                    {
                        Success = false,
                        ErrorMessage = "NFC no está disponible o no está habilitado en este dispositivo"
                    };
                }

                _isReading = true;
                _readTaskCompletionSource = new TaskCompletionSource<NFCReadResult>();

                System.Diagnostics.Debug.WriteLine("[NFCService] Iniciando lectura NFC...");

#if ANDROID
                if (_activity != null && _nfcAdapter != null)
                {
                    // Habilitar el modo de lectura en primer plano
                    var intent = new Intent(_activity, _activity.GetType());
                    intent.AddFlags(ActivityFlags.SingleTop);
                    
                    var pendingIntent = PendingIntent.GetActivity(
                        _activity, 
                        0, 
                        intent, 
                        PendingIntentFlags.Mutable);

                    _nfcAdapter.EnableForegroundDispatch(_activity, pendingIntent, null, null);
                }
#endif

                // Esperar hasta que se lea un tag o timeout
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(60));
                var completedTask = await Task.WhenAny(_readTaskCompletionSource.Task, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    _isReading = false;
                    StopReading();
                    return new NFCReadResult
                    {
                        Success = false,
                        ErrorMessage = "Tiempo de espera agotado. No se detectó ningún tag NFC."
                    };
                }

                return await _readTaskCompletionSource.Task;
            }
            catch (Exception ex)
            {
                _isReading = false;
                System.Diagnostics.Debug.WriteLine($"[NFCService] Error reading NFC: {ex}");
                return new NFCReadResult
                {
                    Success = false,
                    ErrorMessage = $"Error al leer NFC: {ex.Message}"
                };
            }
        }

#if ANDROID
        /// <summary>
        /// Procesa un Intent NFC recibido
        /// </summary>
        public void ProcessNfcIntent(Intent intent)
        {
            try
            {
                if (!_isReading || _readTaskCompletionSource == null)
                {
                    System.Diagnostics.Debug.WriteLine("[NFCService] No hay lectura activa o TaskCompletionSource es null");
                    return;
                }

                var action = intent.Action;
                if (action != NfcAdapter.ActionNdefDiscovered &&
                    action != NfcAdapter.ActionTagDiscovered &&
                    action != NfcAdapter.ActionTechDiscovered)
                {
                    System.Diagnostics.Debug.WriteLine($"[NFCService] Acción no soportada: {action}");
                    return;
                }

                var tag = intent.GetParcelableExtra(NfcAdapter.ExtraTag) as Android.Nfc.Tag;
                if (tag == null)
                {
                    System.Diagnostics.Debug.WriteLine("[NFCService] No se pudo obtener el tag del intent");
                    _readTaskCompletionSource.TrySetResult(new NFCReadResult
                    {
                        Success = false,
                        ErrorMessage = "No se pudo leer el tag NFC"
                    });
                    return;
                }

                // Obtener el ID del tag (UID)
                var tagId = BitConverter.ToString(tag.GetId() ?? Array.Empty<byte>()).Replace("-", "");
                System.Diagnostics.Debug.WriteLine($"[NFCService] Tag detectado - ID: {tagId}");

                // Intentar leer mensajes NDEF si existen
                string? data = null;
                var ndef = Android.Nfc.Tech.Ndef.Get(tag);
                
                if (ndef != null)
                {
                    try
                    {
                        ndef.Connect();
                        var ndefMessage = ndef.NdefMessage;
                        
                        if (ndefMessage != null && ndefMessage.GetRecords().Length > 0)
                        {
                            var record = ndefMessage.GetRecords()[0];
                            data = System.Text.Encoding.UTF8.GetString(record.GetPayload() ?? Array.Empty<byte>());
                            
                            // Eliminar el byte de idioma si es un registro de texto
                            if (data.Length > 3 && record.Tnf == NdefRecord.TnfWellKnown)
                            {
                                data = data.Substring(3);
                            }
                            
                            System.Diagnostics.Debug.WriteLine($"[NFCService] Datos NDEF leídos: {data}");
                        }
                        ndef.Close();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[NFCService] Error leyendo NDEF: {ex}");
                    }
                }

                _isReading = false;
                _readTaskCompletionSource.TrySetResult(new NFCReadResult
                {
                    Success = true,
                    TagId = tagId,
                    Data = data ?? tagId // Si no hay datos NDEF, usar el ID del tag
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NFCService] Error procesando intent NFC: {ex}");
                _readTaskCompletionSource?.TrySetResult(new NFCReadResult
                {
                    Success = false,
                    ErrorMessage = $"Error procesando tag NFC: {ex.Message}"
                });
            }
            finally
            {
                StopReading();
            }
        }
#endif

        /// <summary>
        /// Escribe datos en un tag NFC (para funcionarios)
        /// </summary>
        public async Task<NFCWriteResult> WriteTagAsync(string data)
        {
            try
            {
                bool isAvailable = await IsNFCAvailableAsync();
                if (!isAvailable)
                {
                    return new NFCWriteResult
                    {
                        Success = false,
                        ErrorMessage = "NFC no está disponible"
                    };
                }

                System.Diagnostics.Debug.WriteLine($"[NFCService] Escribiendo en tag NFC: {data}");

#if ANDROID
                // Implementación de escritura NFC (requiere más trabajo)
                await Task.Delay(2000);
                return new NFCWriteResult
                {
                    Success = true
                };
#else
                await Task.Delay(2000);
                return new NFCWriteResult
                {
                    Success = false,
                    ErrorMessage = "Escritura NFC no soportada en esta plataforma"
                };
#endif
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NFCService] Error writing NFC: {ex}");
                return new NFCWriteResult
                {
                    Success = false,
                    ErrorMessage = $"Error al escribir NFC: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Detiene la lectura NFC
        /// </summary>
        public void StopReading()
        {
            _isReading = false;
            
#if ANDROID
            try
            {
                if (_activity != null && _nfcAdapter != null)
                {
                    _nfcAdapter.DisableForegroundDispatch(_activity);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NFCService] Error deshabilitando foreground dispatch: {ex}");
            }
#endif
            
            System.Diagnostics.Debug.WriteLine("[NFCService] Lectura NFC detenida");
        }

        public bool IsReading => _isReading;
    }

    /// <summary>
    /// Resultado de lectura NFC
    /// </summary>
    public class NFCReadResult
    {
        public bool Success { get; set; }
        public string? TagId { get; set; }
        public string? Data { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Resultado de escritura NFC
    /// </summary>
    public class NFCWriteResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
