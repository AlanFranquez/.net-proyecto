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
        private bool _isWriting = false;
        private string? _dataToWrite = null;
        private TaskCompletionSource<NFCReadResult>? _readTaskCompletionSource;
        private TaskCompletionSource<NFCWriteResult>? _writeTaskCompletionSource;

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
        /// Procesa un Intent NFC recibido (para lectura o escritura)
        /// </summary>
        public void ProcessNfcIntent(Intent intent)
        {
            // Si estamos en modo escritura, procesar como escritura
            if (_isWriting)
            {
                ProcessNfcWriteIntent(intent);
                return;
            }

            // Si estamos en modo lectura, procesar como lectura
            if (_isReading)
            {
                ProcessNfcReadIntent(intent);
                return;
            }

            System.Diagnostics.Debug.WriteLine("[NFCService] Intent NFC recibido pero no hay operación activa");
        }

        /// <summary>
        /// Procesa un Intent NFC para lectura
        /// </summary>
        private void ProcessNfcReadIntent(Intent intent)
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
                System.Diagnostics.Debug.WriteLine($"???????????????????????????????????????????");
                System.Diagnostics.Debug.WriteLine($"[NFCService] ?? TAG NFC DETECTADO");
                System.Diagnostics.Debug.WriteLine($"[NFCService] UID del hardware: '{tagId}'");
                System.Diagnostics.Debug.WriteLine($"[NFCService] Longitud UID: {tagId.Length} caracteres");
                System.Diagnostics.Debug.WriteLine($"???????????????????????????????????????????");

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
                            var rawPayload = record.GetPayload() ?? Array.Empty<byte>();
                            data = System.Text.Encoding.UTF8.GetString(rawPayload);
                            
                            System.Diagnostics.Debug.WriteLine($"[NFCService] ?? Datos NDEF encontrados");
                            System.Diagnostics.Debug.WriteLine($"[NFCService] Payload raw (hex): {BitConverter.ToString(rawPayload)}");
                            System.Diagnostics.Debug.WriteLine($"[NFCService] Datos decodificados: '{data}'");
                            System.Diagnostics.Debug.WriteLine($"[NFCService] Longitud: {data.Length} caracteres");
                            System.Diagnostics.Debug.WriteLine($"[NFCService] TNF: {record.Tnf}");
                            
                            // Eliminar el byte de idioma si es un registro de texto
                            if (data.Length > 3 && record.Tnf == NdefRecord.TnfWellKnown)
                            {
                                var dataOriginal = data;
                                data = data.Substring(3);
                                System.Diagnostics.Debug.WriteLine($"[NFCService] Datos después de quitar prefijo: '{dataOriginal}' -> '{data}'");
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"[NFCService] ?? Tag NDEF vacío o sin registros");
                        }
                        ndef.Close();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[NFCService] ? Error leyendo NDEF: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"[NFCService] StackTrace: {ex.StackTrace}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[NFCService] ?? Tag no tiene soporte NDEF");
                }

                var finalData = data ?? tagId;
                System.Diagnostics.Debug.WriteLine($"???????????????????????????????????????????");
                System.Diagnostics.Debug.WriteLine($"[NFCService] ? DATO FINAL A ENVIAR: '{finalData}'");
                System.Diagnostics.Debug.WriteLine($"[NFCService] Fuente: {(data != null ? "NDEF" : "UID del hardware")}");
                System.Diagnostics.Debug.WriteLine($"???????????????????????????????????????????");

                _isReading = false;
                _readTaskCompletionSource.TrySetResult(new NFCReadResult
                {
                    Success = true,
                    TagId = tagId,
                    Data = finalData
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
        /// Escribe datos en un tag NFC en formato NDEF
        /// </summary>
        public async Task<NFCWriteResult> WriteTagAsync(string idCriptografico)
        {
            try
            {
                bool isAvailable = await IsNFCAvailableAsync();
                if (!isAvailable)
                {
                    return new NFCWriteResult
                    {
                        Success = false,
                        ErrorMessage = "NFC no está disponible o no está habilitado"
                    };
                }

                if (_isWriting)
                {
                    return new NFCWriteResult
                    {
                        Success = false,
                        ErrorMessage = "Ya hay una operación de escritura en progreso"
                    };
                }

                System.Diagnostics.Debug.WriteLine($"???????????????????????????????????????????");
                System.Diagnostics.Debug.WriteLine($"[NFCService] ?? INICIANDO ESCRITURA NFC");
                System.Diagnostics.Debug.WriteLine($"[NFCService] Datos a escribir: '{idCriptografico}'");
                System.Diagnostics.Debug.WriteLine($"[NFCService] Longitud: {idCriptografico.Length} caracteres");
                System.Diagnostics.Debug.WriteLine($"???????????????????????????????????????????");

#if ANDROID
                if (_activity == null || _nfcAdapter == null)
                {
                    return new NFCWriteResult
                    {
                        Success = false,
                        ErrorMessage = "NFC no está inicializado correctamente"
                    };
                }

                _isWriting = true;
                _dataToWrite = idCriptografico;
                _writeTaskCompletionSource = new TaskCompletionSource<NFCWriteResult>();

                try
                {
                    // Habilitar modo de escritura en primer plano
                    var intent = new Intent(_activity, _activity.GetType());
                    intent.AddFlags(ActivityFlags.SingleTop);
                    
                    var pendingIntent = PendingIntent.GetActivity(
                        _activity, 
                        0, 
                        intent, 
                        PendingIntentFlags.Mutable);

                    _nfcAdapter.EnableForegroundDispatch(_activity, pendingIntent, null, null);
                    
                    System.Diagnostics.Debug.WriteLine("[NFCService] ? ACERQUE EL CHIP NFC PARA ESCRIBIR...");
                    System.Diagnostics.Debug.WriteLine("[NFCService] El sistema esperará hasta 60 segundos");
                    
                    // Esperar hasta que se escriba el tag o timeout
                    var timeoutTask = Task.Delay(TimeSpan.FromSeconds(60));
                    var completedTask = await Task.WhenAny(_writeTaskCompletionSource.Task, timeoutTask);

                    if (completedTask == timeoutTask)
                    {
                        _isWriting = false;
                        _dataToWrite = null;
                        StopWriting();
                        return new NFCWriteResult
                        {
                            Success = false,
                            ErrorMessage = "Tiempo de espera agotado. No se detectó ningún chip NFC."
                        };
                    }

                    return await _writeTaskCompletionSource.Task;
                }
                finally
                {
                    _isWriting = false;
                    _dataToWrite = null;
                    try
                    {
                        _nfcAdapter.DisableForegroundDispatch(_activity);
                    }
                    catch { }
                }
#else
                await Task.Delay(100);
                return new NFCWriteResult
                {
                    Success = false,
                    ErrorMessage = "Escritura NFC no soportada en esta plataforma"
                };
#endif
            }
            catch (Exception ex)
            {
                _isWriting = false;
                _dataToWrite = null;
                System.Diagnostics.Debug.WriteLine($"[NFCService] ? Error en WriteTagAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[NFCService] StackTrace: {ex.StackTrace}");
                return new NFCWriteResult
                {
                    Success = false,
                    ErrorMessage = $"Error al escribir NFC: {ex.Message}"
                };
            }
        }

#if ANDROID
        /// <summary>
        /// Procesa un Intent NFC para escritura
        /// </summary>
        public void ProcessNfcWriteIntent(Intent intent)
        {
            try
            {
                if (!_isWriting || _writeTaskCompletionSource == null || string.IsNullOrEmpty(_dataToWrite))
                {
                    System.Diagnostics.Debug.WriteLine("[NFCService] No hay operación de escritura activa");
                    return;
                }

                var action = intent.Action;
                if (action != NfcAdapter.ActionNdefDiscovered &&
                    action != NfcAdapter.ActionTagDiscovered &&
                    action != NfcAdapter.ActionTechDiscovered)
                {
                    System.Diagnostics.Debug.WriteLine($"[NFCService] Acción no soportada para escritura: {action}");
                    return;
                }

                var tag = intent.GetParcelableExtra(NfcAdapter.ExtraTag) as Android.Nfc.Tag;
                if (tag == null)
                {
                    System.Diagnostics.Debug.WriteLine("[NFCService] No se pudo obtener el tag del intent");
                    _writeTaskCompletionSource.TrySetResult(new NFCWriteResult
                    {
                        Success = false,
                        ErrorMessage = "No se pudo leer el chip NFC"
                    });
                    return;
                }

                var tagId = BitConverter.ToString(tag.GetId() ?? Array.Empty<byte>()).Replace("-", "");
                System.Diagnostics.Debug.WriteLine($"[NFCService] ?? Chip detectado para escritura - UID: {tagId}");

                // Intentar obtener tecnología NDEF
                var ndef = Android.Nfc.Tech.Ndef.Get(tag);
                if (ndef == null)
                {
                    System.Diagnostics.Debug.WriteLine("[NFCService] ? El chip no soporta NDEF");
                    _writeTaskCompletionSource.TrySetResult(new NFCWriteResult
                    {
                        Success = false,
                        ErrorMessage = "El chip no soporta formato NDEF"
                    });
                    return;
                }

                try
                {
                    ndef.Connect();
                    System.Diagnostics.Debug.WriteLine($"[NFCService] ? Conectado al chip NDEF");
                    System.Diagnostics.Debug.WriteLine($"[NFCService] Tipo: {ndef.Type}");
                    System.Diagnostics.Debug.WriteLine($"[NFCService] Tamaño máximo: {ndef.MaxSize} bytes");
                    System.Diagnostics.Debug.WriteLine($"[NFCService] Escribible: {ndef.IsWritable}");

                    // Verificar si es escribible
                    if (!ndef.IsWritable)
                    {
                        ndef.Close();
                        System.Diagnostics.Debug.WriteLine("[NFCService] ? El chip está protegido contra escritura");
                        _writeTaskCompletionSource.TrySetResult(new NFCWriteResult
                        {
                            Success = false,
                            ErrorMessage = "El chip está protegido contra escritura"
                        });
                        return;
                    }

                    // Crear mensaje NDEF con el IdCriptografico
                    var payload = System.Text.Encoding.UTF8.GetBytes(_dataToWrite);
                    var languageCode = System.Text.Encoding.UTF8.GetBytes("en");
                    
                    // Construir payload completo: [status byte] + [language length] + [language] + [text]
                    var fullPayload = new byte[1 + languageCode.Length + payload.Length];
                    fullPayload[0] = (byte)languageCode.Length;  // Status byte con longitud del idioma
                    Array.Copy(languageCode, 0, fullPayload, 1, languageCode.Length);
                    Array.Copy(payload, 0, fullPayload, 1 + languageCode.Length, payload.Length);

                    System.Diagnostics.Debug.WriteLine($"[NFCService] Payload total: {fullPayload.Length} bytes");
                    System.Diagnostics.Debug.WriteLine($"[NFCService] Payload (hex): {BitConverter.ToString(fullPayload)}");

                    // Crear registro NDEF de tipo texto usando CreateTextRecord
                    var record = NdefRecord.CreateTextRecord("en", _dataToWrite);

                    var message = new NdefMessage(new[] { record });

                    // Verificar tamaño
                    var messageBytesArray = message.ToByteArray();
                    int messageSize = messageBytesArray?.Count() ?? 0;
                    if (messageSize > ndef.MaxSize)
                    {
                        ndef.Close();
                        System.Diagnostics.Debug.WriteLine($"[NFCService] ? Mensaje demasiado grande: {messageSize} > {ndef.MaxSize}");
                        _writeTaskCompletionSource.TrySetResult(new NFCWriteResult
                        {
                            Success = false,
                            ErrorMessage = $"Datos demasiado grandes para el chip ({messageSize} > {ndef.MaxSize} bytes)"
                        });
                        return;
                    }

                    // ? ESCRIBIR en el chip
                    System.Diagnostics.Debug.WriteLine($"[NFCService] ?? Escribiendo mensaje NDEF...");
                    ndef.WriteNdefMessage(message);
                    ndef.Close();

                    System.Diagnostics.Debug.WriteLine($"???????????????????????????????????????????");
                    System.Diagnostics.Debug.WriteLine($"[NFCService] ? ESCRITURA EXITOSA");
                    System.Diagnostics.Debug.WriteLine($"[NFCService] IdCriptografico: '{_dataToWrite}'");
                    System.Diagnostics.Debug.WriteLine($"[NFCService] UID del chip: {tagId}");
                    System.Diagnostics.Debug.WriteLine($"[NFCService] Tamaño escrito: {messageSize} bytes");
                    System.Diagnostics.Debug.WriteLine($"???????????????????????????????????????????");

                    _isWriting = false;
                    _writeTaskCompletionSource.TrySetResult(new NFCWriteResult
                    {
                        Success = true,
                        Message = $"Credencial guardada correctamente en el chip NFC (UID: {tagId})"
                    });
                }
                catch (Exception ex)
                {
                    if (ndef.IsConnected)
                        ndef.Close();

                    System.Diagnostics.Debug.WriteLine($"[NFCService] ? Error escribiendo NDEF: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"[NFCService] StackTrace: {ex.StackTrace}");;
                    
                    _writeTaskCompletionSource.TrySetResult(new NFCWriteResult
                    {
                        Success = false,
                        ErrorMessage = $"Error al escribir: {ex.Message}"
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NFCService] ? Error procesando intent de escritura: {ex.Message}");
                _writeTaskCompletionSource?.TrySetResult(new NFCWriteResult
                {
                    Success = false,
                    ErrorMessage = $"Error: {ex.Message}"
                });
            }
            finally
            {
                StopWriting();
            }
        }

        private void StopWriting()
        {
            _isWriting = false;
            _dataToWrite = null;
            System.Diagnostics.Debug.WriteLine("[NFCService] Escritura NFC detenida");
        }
#endif

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
        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
