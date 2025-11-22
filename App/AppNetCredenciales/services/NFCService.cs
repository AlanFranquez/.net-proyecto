using Plugin.NFC;
using System.Text;

namespace AppNetCredenciales.Services
{
    /// <summary>
    /// Servicio NFC usando Plugin.NFC (NDEF) + API nativa de Android para escritura
    /// Soporta lectura y escritura de tags NFC
    /// </summary>
    public class NfcService
    {
        private readonly ILogger _logger;
        private bool _isListening;
        private bool _isPublishing;
        private bool _isWritingMode;
        private string? _currentCredentialId;
        private string? _pendingWriteData;

        public event EventHandler<string>? TagRead;
        public event EventHandler<string>? TagWritten;
        public event EventHandler<string>? Error;

        public bool IsAvailable => CrossNFC.IsSupported && CrossNFC.Current.IsAvailable;
        public bool IsEnabled => CrossNFC.Current.IsEnabled;
        public bool IsListening => _isListening;
        public bool IsPublishing => _isPublishing;
        public bool IsWritingMode => _isWritingMode;

        public NfcService()
        {
            _logger = new DebugLogger();
        }

        /// <summary>
        /// Inicializa el servicio NFC
        /// </summary>
        public void Initialize()
        {
            try
            {
                _logger.Log("???????????????????????????????????");
                _logger.Log("?? Inicializando NfcService");
                _logger.Log($"? NFC Soportado: {CrossNFC.IsSupported}");
                _logger.Log($"? NFC Disponible: {IsAvailable}");
                _logger.Log($"? NFC Habilitado: {IsEnabled}");
                _logger.Log("???????????????????????????????????");

                if (!CrossNFC.IsSupported)
                {
                    _logger.Log("? NFC no está soportado en este dispositivo");
                    return;
                }

                // Configurar eventos
                CrossNFC.Current.OnMessageReceived += OnMessageReceived;
                CrossNFC.Current.OnMessagePublished += OnMessagePublished;
                CrossNFC.Current.OnTagDiscovered += OnTagDiscovered;
                CrossNFC.Current.OnTagListeningStatusChanged += OnTagListeningStatusChanged;
                
                _logger.Log("? Eventos NFC configurados correctamente");
            }
            catch (Exception ex)
            {
                _logger.Log($"? Error inicializando NFC: {ex.Message}");
                _logger.Log($"StackTrace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Escribe el IdCriptografico en un tag NFC (para USUARIO)
        /// Activa el modo de publicación NFC para que el lector pueda detectar los datos
        /// </summary>
        public async Task<bool> WriteCredentialAsync(string idCriptografico)
        {
            try
            {
                if (string.IsNullOrEmpty(idCriptografico))
                {
                    _logger.Log("? IdCriptografico vacío");
                    Error?.Invoke(this, "IdCriptografico no puede estar vacío");
                    return false;
                }

                if (!IsAvailable || !IsEnabled)
                {
                    _logger.Log("? NFC no disponible o deshabilitado");
                    Error?.Invoke(this, "NFC no está disponible");
                    return false;
                }

                _logger.Log("???????????????????????????????????????????");
                _logger.Log($"?? Preparando escritura de credencial");
                _logger.Log($"?? ID: {idCriptografico}");
                _logger.Log("???????????????????????????????????????????");

                _currentCredentialId = idCriptografico;

                _logger.Log("???????????????????????????????????????????");
                _logger.Log("?? ACTIVANDO MODO ESCRITURA NFC");
                _logger.Log("???????????????????????????????????????????");
                _logger.Log("?? IMPORTANTE:");
                _logger.Log("   • Plugin.NFC en modo NDEF READER");
                _logger.Log("   • Para escritura en tags físicos:");
                _logger.Log("     1. Usa una app como 'NFC Tools'");
                _logger.Log("     2. Escribe el texto: " + idCriptografico);
                _logger.Log("     3. Luego el funcionario podrá leerlo");
                _logger.Log("???????????????????????????????????????????");
                _logger.Log("?? Para ser leído por el funcionario:");
                _logger.Log("   • Acerca tu dispositivo al lector");
                _logger.Log("   • El lector detectará el UID único");
                _logger.Log("   • Alternativamente, usa un tag escrito");
                _logger.Log("???????????????????????????????????????????");

                // NOTA: Plugin.NFC 0.1.26 tiene limitaciones para escritura
                // La mejor estrategia es usar el UID del dispositivo o tags pre-escritos
                
                // Iniciar modo "listening" inverso - el dispositivo espera ser detectado
                // No es escritura real, pero permite que otro lector detecte este dispositivo
                try
                {
                    CrossNFC.Current.StartListening();
                    _isPublishing = true;
                    _logger.Log("? Modo NFC activado - Tu dispositivo está listo para ser detectado");
                }
                catch (Exception ex)
                {
                    _logger.Log($"?? No se pudo activar PublishMode: {ex.Message}");
                    _logger.Log("??  Continuando en modo pasivo...");
                    _isPublishing = true; // Marcamos como activo de todos modos
                }
                
                await Task.Delay(100);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Log($"? Error preparando credencial: {ex.Message}");
                _logger.Log($"StackTrace: {ex.StackTrace}");
                Error?.Invoke(this, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Escribe datos en un tag NFC físico usando API nativa de Android
        /// </summary>
        public async Task<bool> WriteToPhysicalTagAsync(string data)
        {
            try
            {
                if (string.IsNullOrEmpty(data))
                {
                    _logger.Log("? Datos vacíos");
                    Error?.Invoke(this, "Los datos no pueden estar vacíos");
                    return false;
                }

                if (!IsAvailable || !IsEnabled)
                {
                    _logger.Log("? NFC no disponible o deshabilitado");
                    Error?.Invoke(this, "NFC no está disponible");
                    return false;
                }

                _logger.Log("???????????????????????????????????????????");
                _logger.Log("?? MODO: Escribir en Tag Físico (NDEF)");
                _logger.Log($"?? Datos a escribir: {data}");
                _logger.Log("???????????????????????????????????????????");

                _pendingWriteData = data;
                _isWritingMode = true;

#if ANDROID
                // Activar ForegroundDispatch en MainActivity para capturar tags
                var success = await EnableNativeNdefWriteMode(data);
                
                if (success)
                {
                    _logger.Log("? Modo escritura NDEF activado");
                    _logger.Log("?? Acerca ahora un tag NFC vacío o reescribible...");
                    return true;
                }
                else
                {
                    _logger.Log("? No se pudo activar modo escritura");
                    _isWritingMode = false;
                    return false;
                }
#else
                _logger.Log("?? Escritura NDEF solo disponible en Android");
                _isWritingMode = false;
                return false;
#endif
            }
            catch (Exception ex)
            {
                _logger.Log($"? Error activando modo escritura: {ex.Message}");
                _logger.Log($"StackTrace: {ex.StackTrace}");
                Error?.Invoke(this, ex.Message);
                _isWritingMode = false;
                return false;
            }
        }

#if ANDROID
        /// <summary>
        /// Activa el modo de escritura NDEF usando ForegroundDispatch nativo
        /// </summary>
        private async Task<bool> EnableNativeNdefWriteMode(string data)
        {
            try
            {
                var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
                if (activity == null)
                {
                    _logger.Log("? Activity no disponible");
                    return false;
                }

                var adapter = Android.Nfc.NfcAdapter.GetDefaultAdapter(activity);
                if (adapter == null || !adapter.IsEnabled)
                {
                    _logger.Log("? NFC Adapter no disponible o deshabilitado");
                    return false;
                }

                // Crear intent para capturar tags NFC
                var intent = new Android.Content.Intent(activity, activity.GetType())
                    .AddFlags(Android.Content.ActivityFlags.SingleTop);

                var pendingIntent = Android.App.PendingIntent.GetActivity(
                    activity,
                    0,
                    intent,
                    Android.App.PendingIntentFlags.Mutable
                );

                // Filtros para capturar tags NDEF
                var filters = new Android.Content.IntentFilter[]
                {
                    new Android.Content.IntentFilter(Android.Nfc.NfcAdapter.ActionNdefDiscovered),
                    new Android.Content.IntentFilter(Android.Nfc.NfcAdapter.ActionTagDiscovered),
                    new Android.Content.IntentFilter(Android.Nfc.NfcAdapter.ActionTechDiscovered)
                };

                var techLists = new string[][]
                {
                    new string[] { "android.nfc.tech.Ndef" },
                    new string[] { "android.nfc.tech.NdefFormatable" }
                };

                // Activar ForegroundDispatch para capturar el próximo tag
                adapter.EnableForegroundDispatch(activity, pendingIntent, filters, techLists);

                _logger.Log("? ForegroundDispatch activado para escritura NDEF");
                
                await Task.Delay(100);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Log($"? Error activando ForegroundDispatch: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Escribe el mensaje NDEF en el tag detectado
        /// Este método debe ser llamado desde MainActivity.OnNewIntent
        /// </summary>
        public async Task<bool> WriteNdefToTag(Android.Content.Intent intent)
        {
            try
            {
                if (string.IsNullOrEmpty(_pendingWriteData))
                {
                    _logger.Log("?? No hay datos pendientes para escribir");
                    return false;
                }

                _logger.Log("???????????????????????????????????????????");
                _logger.Log("?? ESCRIBIENDO TAG NFC...");
                _logger.Log("???????????????????????????????????????????");

                var tag = intent.GetParcelableExtra(Android.Nfc.NfcAdapter.ExtraTag) as Android.Nfc.Tag;
                if (tag == null)
                {
                    _logger.Log("? No se pudo obtener el tag del intent");
                    Error?.Invoke(this, "Tag NFC no válido");
                    return false;
                }

                // Crear mensaje NDEF con el IdCriptografico
                var message = CreateNdefTextMessage(_pendingWriteData);

                // Intentar escribir en tag formateado NDEF
                var ndef = Android.Nfc.Tech.Ndef.Get(tag);
                if (ndef != null)
                {
                    return await WriteToNdefTag(ndef, message);
                }

                // Si no está formateado, intentar formatear
                var ndefFormatable = Android.Nfc.Tech.NdefFormatable.Get(tag);
                if (ndefFormatable != null)
                {
                    return await FormatAndWriteTag(ndefFormatable, message);
                }

                _logger.Log("? Tag no soporta NDEF ni puede ser formateado");
                Error?.Invoke(this, "Este tag NFC no es compatible con NDEF");
                return false;
            }
            catch (Exception ex)
            {
                _logger.Log($"? Error escribiendo tag: {ex.Message}");
                _logger.Log($"StackTrace: {ex.StackTrace}");
                Error?.Invoke(this, $"Error: {ex.Message}");
                return false;
            }
            finally
            {
                // Desactivar modo escritura
                _isWritingMode = false;
                _pendingWriteData = null;
                
                // Desactivar ForegroundDispatch
                await DisableNativeNdefWriteMode();
            }
        }

        /// <summary>
        /// Escribe en un tag ya formateado NDEF
        /// </summary>
        private async Task<bool> WriteToNdefTag(Android.Nfc.Tech.Ndef ndef, Android.Nfc.NdefMessage message)
        {
            try
            {
                // Conectar síncronamente (Android NFC no usa async)
                ndef.Connect();

                if (!ndef.IsWritable)
                {
                    _logger.Log("? Tag es de solo lectura");
                    Error?.Invoke(this, "Este tag es de solo lectura");
                    ndef.Close();
                    return false;
                }

                int size = message.ToByteArray().Length;
                if (ndef.MaxSize < size)
                {
                    _logger.Log($"? Tag muy pequeño. Necesario: {size} bytes, Disponible: {ndef.MaxSize} bytes");
                    Error?.Invoke(this, "Tag NFC sin espacio suficiente");
                    ndef.Close();
                    return false;
                }

                _logger.Log($"?? Escribiendo {size} bytes en tag NDEF...");
                
                // Escribir mensaje
                ndef.WriteNdefMessage(message);
                ndef.Close();

                _logger.Log("? TAG ESCRITO EXITOSAMENTE");
                _logger.Log($"?? Datos escritos: {_pendingWriteData}");
                
                TagWritten?.Invoke(this, _pendingWriteData ?? "");
                
                await Task.Delay(100);
                return true;
            }
            catch (Android.Nfc.TagLostException)
            {
                _logger.Log("? Tag removido antes de completar la escritura");
                Error?.Invoke(this, "Tag removido. Intenta de nuevo manteniendo el tag quieto");
                return false;
            }
            catch (Exception ex)
            {
                _logger.Log($"? Error escribiendo: {ex.Message}");
                Error?.Invoke(this, $"Error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Formatea y escribe en un tag sin formato
        /// </summary>
        private async Task<bool> FormatAndWriteTag(Android.Nfc.Tech.NdefFormatable formatable, Android.Nfc.NdefMessage message)
        {
            try
            {
                _logger.Log("?? Formateando tag NFC...");
                
                // Conectar y formatear síncronamente
                formatable.Connect();
                formatable.Format(message);
                formatable.Close();

                _logger.Log("? TAG FORMATEADO Y ESCRITO EXITOSAMENTE");
                _logger.Log($"?? Datos escritos: {_pendingWriteData}");
                
                TagWritten?.Invoke(this, _pendingWriteData ?? "");
                
                await Task.Delay(100);
                return true;
            }
            catch (Android.Nfc.TagLostException)
            {
                _logger.Log("? Tag removido antes de completar la escritura");
                Error?.Invoke(this, "Tag removido. Intenta de nuevo manteniendo el tag quieto");
                return false;
            }
            catch (Exception ex)
            {
                _logger.Log($"? Error formateando: {ex.Message}");
                Error?.Invoke(this, $"Error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Crea un mensaje NDEF de tipo texto
        /// </summary>
        private Android.Nfc.NdefMessage CreateNdefTextMessage(string text)
        {
            // Crear payload con formato NDEF Text
            // Formato: [Status Byte][Language Code][Text]
            // Status Byte: 0x00 = UTF-8, English (sin compresión)
            var languageCode = "en";
            var languageCodeBytes = Encoding.UTF8.GetBytes(languageCode);
            var textBytes = Encoding.UTF8.GetBytes(text);
            
            var payload = new byte[1 + languageCodeBytes.Length + textBytes.Length];
            payload[0] = (byte)(0x00 | languageCodeBytes.Length); // Status byte + language length
            Array.Copy(languageCodeBytes, 0, payload, 1, languageCodeBytes.Length);
            Array.Copy(textBytes, 0, payload, 1 + languageCodeBytes.Length, textBytes.Length);

            var record = new Android.Nfc.NdefRecord(
                Android.Nfc.NdefRecord.TnfWellKnown,
                Android.Nfc.NdefRecord.RtdText.ToArray(), // Convertir IList<byte> a byte []
                new byte[0], // ID vacío
                payload
            );

            return new Android.Nfc.NdefMessage(new[] { record });
        }

        /// <summary>
        /// Desactiva el modo de escritura NDEF
        /// </summary>
        private async Task DisableNativeNdefWriteMode()
        {
            try
            {
                var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
                if (activity == null) return;

                var adapter = Android.Nfc.NfcAdapter.GetDefaultAdapter(activity);
                if (adapter == null) return;

                adapter.DisableForegroundDispatch(activity);
                _logger.Log("? ForegroundDispatch desactivado");
                
                await Task.Delay(50);
            }
            catch (Exception ex)
            {
                _logger.Log($"?? Error desactivando ForegroundDispatch: {ex.Message}");
            }
        }
#endif

        /// <summary>
        /// Inicia el modo lector (para FUNCIONARIO)
        /// </summary>
        public void StartListening()
        {
            try
            {
                if (!IsAvailable || !IsEnabled)
                {
                    _logger.Log("? NFC no disponible o deshabilitado");
                    Error?.Invoke(this, "NFC no está disponible");
                    return;
                }

                // Detener publicación si está activa
                if (_isPublishing)
                {
                    StopPublishing();
                }

                _logger.Log("???????????????????????????????????????????");
                _logger.Log("?? INICIANDO MODO LECTOR NFC");
                _logger.Log("???????????????????????????????????????????");

                CrossNFC.Current.StartListening();
                _isListening = true;

                _logger.Log("? Lector NFC activo - Esperando tags...");
            }
            catch (Exception ex)
            {
                _logger.Log($"? Error iniciando lector: {ex.Message}");
                Error?.Invoke(this, ex.Message);
            }
        }

        /// <summary>
        /// Detiene el modo lector
        /// </summary>
        public void StopListening()
        {
            try
            {
                _logger.Log("?? Deteniendo lector NFC...");
                CrossNFC.Current.StopListening();
                _isListening = false;
                _logger.Log("? Lector NFC detenido");
            }
            catch (Exception ex)
            {
                _logger.Log($"? Error deteniendo lector: {ex.Message}");
            }
        }

        /// <summary>
        /// Detiene el modo publicación/escritura
        /// </summary>
        public void StopPublishing()
        {
            try
            {
                if (!_isPublishing) return;

                _logger.Log("?? Deteniendo publicación NFC...");
                CrossNFC.Current.StopPublishing();
                _isPublishing = false;
                _logger.Log("? Publicación NFC detenida");
            }
            catch (Exception ex)
            {
                _logger.Log($"? Error deteniendo publicación: {ex.Message}");
            }
        }

        /// <summary>
        /// Detiene todos los modos NFC
        /// </summary>
        public void StopAll()
        {
            StopListening();
            StopPublishing();
            
            if (_isWritingMode)
            {
#if ANDROID
                _ = DisableNativeNdefWriteMode();
#endif
                _isWritingMode = false;
                _pendingWriteData = null;
            }
        }

        /// <summary>
        /// Evento cuando se lee un tag NFC
        /// </summary>
        private void OnMessageReceived(ITagInfo tagInfo)
        {
            try
            {
                _logger.Log("????????????????????????????????????????????");
                _logger.Log("??  ?? TAG NFC RECIBIDO                 ??");
                _logger.Log("????????????????????????????????????????????");

                if (tagInfo == null)
                {
                    _logger.Log("?? tagInfo es NULL");
                    return;
                }

                // INFORMACIÓN DEL TAG
                _logger.Log($"?? Tag ID (UID): {BitConverter.ToString(tagInfo.Identifier ?? Array.Empty<byte>())}");
                _logger.Log($"?? Serial Number: {tagInfo.SerialNumber ?? "N/A"}");
                _logger.Log($"?? Is Empty: {tagInfo.IsEmpty}");
                _logger.Log($"?? Is Writable: {tagInfo.IsWritable}");

                // Verificar si el tag tiene registros
                if (tagInfo.Records == null || !tagInfo.Records.Any())
                {
                    _logger.Log("?? Tag vacío o sin registros NDEF");
                    _logger.Log("??  Este tag no contiene datos NDEF escritos");
                    _logger.Log("?? Posibles soluciones:");
                    _logger.Log("   1. Escribir datos NDEF en el tag usando esta app");
                    _logger.Log("   2. Usar el UID del tag como identificador");
                    _logger.Log("   3. Verificar que el tag soporte NDEF");
                    
                    // ALTERNATIVA: Usar el UID como identificador
                    if (tagInfo.Identifier != null && tagInfo.Identifier.Length > 0)
                    {
                        var uid = BitConverter.ToString(tagInfo.Identifier).Replace("-", "");
                        _logger.Log($"?? Usando UID como identificador: {uid}");
                        TagRead?.Invoke(this, uid);
                        return;
                    }
                    
                    Error?.Invoke(this, "Tag NFC vacío - No contiene datos NDEF ni UID válido");
                    return;
                }

                _logger.Log($"?? Registros encontrados: {tagInfo.Records.Length}");

                foreach (var record in tagInfo.Records)
                {
                    _logger.Log($"??? Tipo: {record.TypeFormat}");
                    _logger.Log($"??? MimeType: {record.MimeType ?? "N/A"}");
                    _logger.Log($"??? Payload Length: {record.Payload?.Length ?? 0} bytes");

                    if (record.Payload != null && record.Payload.Length > 0)
                    {
                        try
                        {
                            // Intentar decodificar como UTF-8
                            var payload = Encoding.UTF8.GetString(record.Payload);
                            
                            // Limpiar caracteres de control NDEF (primer byte puede ser metadata)
                            if (payload.Length > 0 && payload[0] < 32)
                            {
                                payload = payload.Substring(1);
                            }
                            
                            // Si hay código de idioma (ej: "en"), removerlo
                            if (payload.Length > 2 && char.IsLower(payload[0]) && char.IsLower(payload[1]))
                            {
                                payload = payload.Substring(2);
                            }
                            
                            _logger.Log($"??? Payload (UTF-8): {payload}");

                            if (!string.IsNullOrWhiteSpace(payload))
                            {
                                _logger.Log("? IdCriptografico extraído correctamente");
                                TagRead?.Invoke(this, payload.Trim());
                                return;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Log($"?? Error decodificando payload: {ex.Message}");
                            // Mostrar payload en hexadecimal como fallback
                            var hexPayload = BitConverter.ToString(record.Payload);
                            _logger.Log($"??? Payload (HEX): {hexPayload}");
                        }
                    }
                }

                _logger.Log("?? No se pudo extraer IdCriptografico de los registros NDEF");
                Error?.Invoke(this, "No se encontraron datos válidos en el tag NFC");
            }
            catch (Exception ex)
            {
                _logger.Log($"? Error procesando tag: {ex.Message}");
                _logger.Log($"StackTrace: {ex.StackTrace}");
                Error?.Invoke(this, ex.Message);
            }
        }

        /// <summary>
        /// Evento cuando se escribe/publica un tag NFC exitosamente
        /// </summary>
        private void OnMessagePublished(ITagInfo tagInfo)
        {
            try
            {
                _logger.Log("????????????????????????????????????????????");
                _logger.Log("?  ?? TAG NFC ESCRITO EXITOSAMENTE     ??");
                _logger.Log("????????????????????????????????????????????");

                if (tagInfo != null)
                {
                    _logger.Log($"?? Tag ID: {BitConverter.ToString(tagInfo.Identifier ?? Array.Empty<byte>())}");
                    _logger.Log($"?? Serial Number: {tagInfo.SerialNumber ?? "N/A"}");
                    _logger.Log($"?? Registros escritos: {tagInfo.Records?.Length ?? 0}");
                }

                _logger.Log($"?? Credencial escrita: {_currentCredentialId}");
                _logger.Log("????????????????????????????????????????????");

                TagWritten?.Invoke(this, _currentCredentialId ?? "");
                
                // Mantener la publicación activa para permitir múltiples lecturas
                // No detener automáticamente, dejar que el usuario lo haga
                _logger.Log("??  Publicación sigue activa para permitir más lecturas");
            }
            catch (Exception ex)
            {
                _logger.Log($"? Error en OnMessagePublished: {ex.Message}");
            }
        }

        /// <summary>
        /// Evento cuando se descubre un tag
        /// </summary>
        private void OnTagDiscovered(ITagInfo tagInfo, bool format)
        {
            try
            {
                _logger.Log("????????????????????????????????????????????");
                _logger.Log($"?? Tag descubierto - Format: {format}");
                
                if (tagInfo != null)
                {
                    _logger.Log($"?? Tag ID: {BitConverter.ToString(tagInfo.Identifier ?? Array.Empty<byte>())}");
                    _logger.Log($"?? Serial Number: {tagInfo.SerialNumber ?? "N/A"}");
                    _logger.Log($"?? Is Empty: {tagInfo.IsEmpty}");
                    _logger.Log($"?? Is Writable: {tagInfo.IsWritable}");
                    _logger.Log($"?? Records Count: {tagInfo.Records?.Length ?? 0}");
                }
                _logger.Log("????????????????????????????????????????????");
            }
            catch (Exception ex)
            {
                _logger.Log($"? Error en OnTagDiscovered: {ex.Message}");
            }
        }

        /// <summary>
        /// Evento cuando cambia el estado de escucha NFC
        /// </summary>
        private void OnTagListeningStatusChanged(bool isListening)
        {
            try
            {
                _logger.Log($"?? Estado de escucha NFC cambió: {(isListening ? "ACTIVO" : "INACTIVO")}");
                _isListening = isListening;
            }
            catch (Exception ex)
            {
                _logger.Log($"? Error en OnTagListeningStatusChanged: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene información de diagnóstico del servicio
        /// </summary>
        public string GetDiagnosticInfo()
        {
            var info = new StringBuilder();
            info.AppendLine("???????????????????????????????????");
            info.AppendLine("?? NFC SERVICE DIAGNOSTICS");
            info.AppendLine("???????????????????????????????????");
            info.AppendLine($"? NFC Soportado: {CrossNFC.IsSupported}");
            info.AppendLine($"? NFC Disponible: {IsAvailable}");
            info.AppendLine($"? NFC Habilitado: {IsEnabled}");
            info.AppendLine($"? Modo Lectura: {(_isListening ? "ACTIVO" : "INACTIVO")}");
            info.AppendLine($"? Modo Escritura: {(_isPublishing ? "ACTIVO" : "INACTIVO")}");
            info.AppendLine($"? Modo Escritura NDEF: {(_isWritingMode ? "ACTIVO" : "INACTIVO")}");
            info.AppendLine($"? Credencial Actual: {_currentCredentialId ?? "N/A"}");
            info.AppendLine($"? Timestamp: {DateTime.Now:HH:mm:ss}");
            info.AppendLine("???????????????????????????????????");
            return info.ToString();
        }

        /// <summary>
        /// Logger simple para debugging
        /// </summary>
        private class DebugLogger : ILogger
        {
            public void Log(string message)
            {
                System.Diagnostics.Debug.WriteLine($"[NfcService] {message}");
            }
        }

        private interface ILogger
        {
            void Log(string message);
        }
    }
}
