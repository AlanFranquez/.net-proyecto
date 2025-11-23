using AppNetCredenciales.Data;
using AppNetCredenciales.models;
using AppNetCredenciales.Services;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using SQLite;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace AppNetCredenciales.Views
{
    public partial class ScanView : ContentPage
    {
        string lastDetectedBarcode = string.Empty;
        DateTime lastDetectedTime = DateTime.MinValue;
        CancellationTokenSource? _retryCts;
        private readonly LocalDBService _db;
        private readonly BiometricService _biometricService;
        private bool _biometricAuthenticated = false;

        public ScanView()
        {
            InitializeComponent();
            cameraBarcodeReaderView.IsDetecting = false;
            cameraBarcodeReaderView.Options = new ZXing.Net.Maui.BarcodeReaderOptions
            {
                Formats = ZXing.Net.Maui.BarcodeFormat.QrCode,
                AutoRotate = true,
                Multiple = false
            };

            _db = App.Services?.GetRequiredService<LocalDBService>()
                  ?? throw new InvalidOperationException("LocalDBService not registered in DI.");
            
            _biometricService = App.Services?.GetRequiredService<BiometricService>()
                  ?? new BiometricService();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            _biometricAuthenticated = false;

           
            bool userWantsToAuthenticate = await DisplayAlert(
                "Autenticaci�n Requerida",
                "Debes verificar tu identidad con huella digital antes de escanear credenciales.\n\n�Deseas continuar?",
                "Autenticar",
                "Cancelar");

            if (!userWantsToAuthenticate)
            {
                await DisplayAlert("Autenticaci�n Cancelada", 
                    "Debes autenticarte con tu huella digital para usar el esc�ner.", 
                    "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            var biometricResult = await _biometricService.AuthenticateAsync(
                "Verificar tu identidad para escanear credenciales");

            //if (!biometricResult.Success)
            //{
            //    await DisplayAlert("Autenticaci�n Fallida", 
            //        biometricResult.ErrorMessage ?? "No se pudo verificar tu identidad.", 
            //        "OK");
            //    await Shell.Current.GoToAsync("..");
            //    return;
            //}

            _biometricAuthenticated = true;

     
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.Camera>();

            if (status == PermissionStatus.Granted)
            {
                await StartDetectionSafeAsync();
            }
            else
            {
                await DisplayAlert("Permission required", "Son requeridos permisos de camara para escanear el QR", "OK");
            }
        }

        protected override void OnDisappearing()
        {
            _retryCts?.Cancel();
            cameraBarcodeReaderView.IsDetecting = false;
            _biometricAuthenticated = false;
            base.OnDisappearing();
        }

        private async Task StartDetectionSafeAsync()
        {
            try
            {
                cameraBarcodeReaderView.IsDetecting = false;
                await Task.Delay(200);
                cameraBarcodeReaderView.IsDetecting = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StartDetectionSafeAsync error: {ex}");
                // schedule a few retries
                _retryCts?.Cancel();
                _retryCts = new CancellationTokenSource();
                _ = RetryStartAsync(_retryCts.Token);
            }
        }

        private async Task RetryStartAsync(CancellationToken ct)
        {
            const int retries = 3;
            for (int i = 0; i < retries && !ct.IsCancellationRequested; i++)
            {
                await Task.Delay(1000, ct).ContinueWith(_ => { }, TaskScheduler.Default);
                try
                {
                    cameraBarcodeReaderView.IsDetecting = true;
                    return;
                }
                catch (Exception retryEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Retry {i + 1} failed: {retryEx}");
                }
            }

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert("Camera error", "Unable to start the camera. Try reopening the page or using a physical device.", "OK");
            });
        }

        protected async void BarcodesDetected(object? sender, ZXing.Net.Maui.BarcodeDetectionEventArgs e)
        {
            
            if (!_biometricAuthenticated)
            {
                System.Diagnostics.Debug.WriteLine("[Scan] Intento de escaneo sin autenticaci�n biom�trica");
                return;
            }

            var first = e.Results?.FirstOrDefault();
            if (first is null) return;

            var payload = (first.Value ?? string.Empty).Trim();
            System.Diagnostics.Debug.WriteLine($"[Scan] Scanned payload: '{payload}'");

            if (payload == lastDetectedBarcode && (DateTime.Now - lastDetectedTime).TotalSeconds < 1) return;
            lastDetectedBarcode = payload;
            lastDetectedTime = DateTime.Now;

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                cameraBarcodeReaderView.IsDetecting = false;
                try
                {
                    await HandleScannedPayloadAsync(payload);
                }
                finally
                {
                    cameraBarcodeReaderView.IsDetecting = true;
                }
            });
        }

        private bool EstaEnVentanaHoraria(TimeSpan horaActual, string? ventanaHoraria)
        {
            if (string.IsNullOrWhiteSpace(ventanaHoraria))
                return true; 

            try
            {
                var partes = ventanaHoraria.Split('-');
                if (partes.Length != 2) return true;

                if (TimeSpan.TryParse(partes[0].Trim(), out var inicio) &&
                    TimeSpan.TryParse(partes[1].Trim(), out var fin))
                {
                    if (inicio <= fin)
                    {
                        
                        return horaActual >= inicio && horaActual <= fin;
                    }
                    else
                    {
                        
                        return horaActual >= inicio || horaActual <= fin;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Scan] Error parseando ventana horaria '{ventanaHoraria}': {ex.Message}");
            }

            return true; // En caso de error, no restringir
        }

        private async Task HandleScannedPayloadAsync(string payload)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[Scan] === INICIANDO PROCESAMIENTO DE QR ===");
                System.Diagnostics.Debug.WriteLine($"[Scan] Payload recibido: '{payload}'");

                var parts = payload?.Split('|');
                if (parts == null || parts.Length < 2)
                {
                    System.Diagnostics.Debug.WriteLine($"[Scan] ❌ QR inválido - partes: {parts?.Length ?? 0}");
                    await DisplayAlert("QR Inválido", "El código QR no tiene el formato correcto.", "OK");
                    return;
                }

                var cryptoId = parts[0].Trim();
                var eventoId = parts[1].Trim();
                System.Diagnostics.Debug.WriteLine($"[Scan] Crypto ID: '{cryptoId}', Evento ID: '{eventoId}'");

                


                // === BUSCAR CREDENCIAL ===
                System.Diagnostics.Debug.WriteLine($"[Scan] === BUSCANDO CREDENCIAL ===");
                var credenciales = await _db.GetCredencialesAsync();
                System.Diagnostics.Debug.WriteLine($"[Scan] Total credenciales en BD: {credenciales.Count}");

                Credencial? cred = null;
                foreach (var c in credenciales)
                {
                    if (c == null) continue;
                    System.Diagnostics.Debug.WriteLine($"[Scan] Verificando credencial: IdCriptografico='{c.IdCriptografico}' vs '{cryptoId}'");
                    if (c.IdCriptografico == cryptoId)
                    {
                        cred = c;
                        System.Diagnostics.Debug.WriteLine($"[Scan] ✅ Credencial encontrada: ID={c.CredencialId}, API ID={c.idApi}");
                        break;
                    }
                }

                if (cred == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[Scan] ❌ Credencial NO encontrada para crypto ID: '{cryptoId}'");
                }

                // === OBTENER USUARIO LOGUEADO ===
                var usuario = new Usuario();
                var usuarios = await _db.GetUsuariosAsync();
                foreach(var u in usuarios)
                {
                    if(cred.usuarioIdApi == u.idApi)
                    {
                        usuario = u;
                        System.Diagnostics.Debug.WriteLine($"[Scan] ✅ Usuario encontrado: {usuario.Email} (ID API: {usuario.idApi})");
                        break;
                    }
                }

                if(usuario == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[Scan] ❌ Usuario NO encontrado para ID API: '{cred.usuarioIdApi}'");
                    await DisplayAlert("Error de Usuario",
                        "No se pudo encontrar el usuario asociado a la credencial. Contacte al administrador.", "OK");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"[Scan] ✅ Usuario encontrado: {usuario.Email} (ID API: {usuario.idApi})");

                // === OBTENER ROLES DEL USUARIO ===
                System.Diagnostics.Debug.WriteLine($"[Scan] === OBTENIENDO ROLES DEL USUARIO ===");
                var roles = await _db.GetRolesAsync();
                System.Diagnostics.Debug.WriteLine($"[Scan] Total roles en BD: {roles.Count}");

                var rolesUsuario = new List<Rol>();
                var rolesIDsUsuario = usuario.RolesIDs ?? Array.Empty<string>();
                System.Diagnostics.Debug.WriteLine($"[Scan] Roles IDs del usuario: [{string.Join(", ", rolesIDsUsuario)}]");

                foreach (var rolId in rolesIDsUsuario)
                {
                    var rol = roles.FirstOrDefault(r => r.idApi == rolId);
                    if (rol != null)
                    {
                        rolesUsuario.Add(rol);
                        System.Diagnostics.Debug.WriteLine($"[Scan] ✅ Rol encontrado: {rol.Tipo} (ID API: {rol.idApi})");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[Scan] ⚠️ Rol no encontrado para ID: {rolId}");
                    }
                }

                var tiposRolesUsuario = rolesUsuario.Select(r => r.Tipo).Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
                System.Diagnostics.Debug.WriteLine($"[Scan] Tipos de roles del usuario: [{string.Join(", ", tiposRolesUsuario)}]");

                

                // === BUSCAR ESPACIO ===
                System.Diagnostics.Debug.WriteLine($"[Scan] === BUSCANDO ESPACIO ===");
                var espacios = await _db.GetEspaciosAsync();
                System.Diagnostics.Debug.WriteLine($"[Scan] Total espacios en BD: {espacios.Count}");

                Espacio? espacio = null;
                foreach (var esp in espacios)
                {
                    if (esp == null) continue;
                    System.Diagnostics.Debug.WriteLine($"[Scan] Verificando espacio: idApi='{esp.idApi}' vs '{eventoId}'");
                    if (esp.idApi == eventoId)
                    {
                        espacio = esp;
                        System.Diagnostics.Debug.WriteLine($"[Scan] ✅ Espacio encontrado: {esp.Nombre} (ID API: {esp.idApi})");
                        break;
                    }
                }

                if (espacio == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[Scan] ❌ Espacio NO encontrado para evento ID: '{eventoId}'");
                }

                // === VALIDACIONES BÁSICAS ===
                System.Diagnostics.Debug.WriteLine($"[Scan] === VALIDACIONES BÁSICAS ===");
                if (cred == null && espacio == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[Scan] ❌ Ni credencial ni espacio encontrados");
                    await DisplayAlert("Credencial y Espacio no reconocidos",
                        $"No se encontró la credencial para '{cryptoId}' ni el espacio para '{eventoId}'.", "Cerrar");
                    return;
                }

                if (cred == null || espacio == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[Scan] ❌ Falta credencial o espacio - Cred: {(cred != null ? "✅" : "❌")}, Espacio: {(espacio != null ? "✅" : "❌")}");
                    await DisplayAlert("Acceso Denegado",
                        $"No se encontró {(cred == null ? "la credencial" : "el espacio")}.", "Cerrar");

                    if (espacio != null && cred != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"[Scan] Guardando evento de acceso denegado");
                        var evNegado = new EventoAcceso
                        {
                            MomentoDeAcceso = DateTime.UtcNow,
                            CredencialIdApi = cred?.idApi,
                            EspacioIdApi = espacio.idApi,
                            Espacio = espacio,
                            Resultado = AccesoTipo.Denegar,
                            Motivo = cred == null ? "Credencial no encontrada" : "Espacio no encontrado"
                        };
                        await _db.SaveAndPushEventoAccesoAsync(evNegado);
                    }
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"[Scan] ✅ Validaciones básicas pasadas - Credencial y Espacio encontrados");

                // === VALIDAR ID API DE CREDENCIAL ===
                if (string.IsNullOrEmpty(cred.idApi))
                {
                    System.Diagnostics.Debug.WriteLine($"[Scan] ❌ Credencial sin idApi - ID local: {cred.CredencialId}");
                    await DisplayAlert("Error de Credencial",
                        "La credencial no tiene ID de API válido. Contacte al administrador.", "OK");
                    return;
                }
                System.Diagnostics.Debug.WriteLine($"[Scan] ✅ Credencial tiene ID API válido: {cred.idApi}");

                // === VERIFICAR EXPIRACIÓN ===
                System.Diagnostics.Debug.WriteLine($"[Scan] === VERIFICANDO EXPIRACIÓN DE CREDENCIAL ===");
                if (cred.FechaExpiracion.HasValue)
                {
                    System.Diagnostics.Debug.WriteLine($"[Scan] Fecha expiración: {cred.FechaExpiracion.Value:yyyy-MM-dd}, Hoy: {DateTime.Today:yyyy-MM-dd}");
                    if (cred.FechaExpiracion.Value.Date < DateTime.Today)
                    {
                        System.Diagnostics.Debug.WriteLine($"[Scan] ❌ Credencial expirada");
                        await DisplayAlert("Credencial Expirada",
                            $"La credencial expiró el {cred.FechaExpiracion.Value:dd/MM/yyyy}.\nAcceso denegado.",
                            "Cerrar");

                        var eventoExpirado = new EventoAcceso
                        {
                            MomentoDeAcceso = DateTime.UtcNow,
                            CredencialIdApi = cred.idApi,
                            EspacioIdApi = espacio.idApi,
                            Credencial = cred,
                            Espacio = espacio,
                            Resultado = AccesoTipo.Denegar,
                            Motivo = $"Credencial expirada el {cred.FechaExpiracion.Value:dd/MM/yyyy}"
                        };
                        await _db.SaveAndPushEventoAccesoAsync(eventoExpirado);
                        return;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[Scan] ✅ Credencial vigente");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[Scan] ℹ️ Credencial sin fecha de expiración (vigente indefinidamente)");
                }

                // === EVALUACIÓN DE REGLAS DE ACCESO ===
                System.Diagnostics.Debug.WriteLine($"[Scan] === INICIANDO EVALUACIÓN DE REGLAS DE ACCESO ===");
                bool requiereHuella = false;

                var reglas = await _db.GetReglasAccesoAsync();
                System.Diagnostics.Debug.WriteLine($"[Scan] Total reglas en BD: {reglas.Count}");

                var reglasEspacio = reglas
                    .Where(regla => regla.EspaciosIDs != null &&
                                   regla.EspaciosIDs.Contains(espacio.idApi ?? string.Empty))
                    .OrderBy(regla => regla.Prioridad)
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"[Scan] Reglas aplicables al espacio '{espacio.Nombre}': {reglasEspacio.Count}");
                foreach (var regla in reglasEspacio)
                {
                    System.Diagnostics.Debug.WriteLine($"[Scan]   - Regla: {regla.Politica} | Rol: '{regla.Rol}' | Prioridad: {regla.Prioridad} | Ventana: '{regla.VentanaHoraria}'");
                }

                bool accesoPermitido = true;
                string motivoDenegacion = "";

                if (reglasEspacio.Count > 0)
                {
                    var ahora = DateTime.Now;
                    var horaActual = ahora.TimeOfDay;
                    System.Diagnostics.Debug.WriteLine($"[Scan] Fecha/hora actual: {ahora:yyyy-MM-dd HH:mm:ss} (TimeSpan: {horaActual})");

                    // Filtrar reglas aplicables por tipo de rol del usuario
                    var reglasAplicables = reglasEspacio
                        .Where(r => string.IsNullOrWhiteSpace(r.Rol) || tiposRolesUsuario.Contains(r.Rol))
                        .ToList();

                    System.Diagnostics.Debug.WriteLine($"[Scan] Reglas aplicables al usuario (por rol): {reglasAplicables.Count}");
                    foreach (var regla in reglasAplicables)
                    {
                        System.Diagnostics.Debug.WriteLine($"[Scan]   ✅ Regla aplicable: {regla.Politica} | Rol: '{regla.Rol}' | Prioridad: {regla.Prioridad}");
                    }

                    // === PASO 1: VERIFICAR REGLAS DE DENEGACIÓN ===
                    System.Diagnostics.Debug.WriteLine($"[Scan] === PASO 1: EVALUANDO REGLAS DE DENEGACIÓN ===");
                    var reglasDenegar = reglasAplicables.Where(r => r.Politica == AccesoTipo.Denegar).ToList();
                    System.Diagnostics.Debug.WriteLine($"[Scan] Reglas de DENEGAR a evaluar: {reglasDenegar.Count}");

                    foreach (var regla in reglasDenegar)
                    {
                        System.Diagnostics.Debug.WriteLine($"[Scan] --- Evaluando regla DENEGAR ---");
                        System.Diagnostics.Debug.WriteLine($"[Scan] Rol: '{regla.Rol}' | Ventana: '{regla.VentanaHoraria}' | Prioridad: {regla.Prioridad}");
                        System.Diagnostics.Debug.WriteLine($"[Scan] Vigencia: {regla.VigenciaInicio?.ToString("yyyy-MM-dd") ?? "null"} - {regla.VigenciaFin?.ToString("yyyy-MM-dd") ?? "null"}");

                        bool todasLasCondicionesSeCumplen = true;

                        // Verificar que el usuario tenga el rol requerido por la regla
                        if (!string.IsNullOrWhiteSpace(regla.Rol) && !tiposRolesUsuario.Contains(regla.Rol))
                        {
                            todasLasCondicionesSeCumplen = false;
                            System.Diagnostics.Debug.WriteLine($"[Scan] ❌ Rol no coincide: usuario no tiene rol '{regla.Rol}'");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"[Scan] ✅ Rol coincide o regla sin rol específico");
                        }

                        // Verificar VigenciaInicio
                        if (regla.VigenciaInicio.HasValue)
                        {
                            if (ahora < regla.VigenciaInicio.Value)
                            {
                                todasLasCondicionesSeCumplen = false;
                                System.Diagnostics.Debug.WriteLine($"[Scan] ❌ Fuera de vigencia inicio: {ahora} < {regla.VigenciaInicio.Value}");
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"[Scan] ✅ Dentro de vigencia inicio: {ahora} >= {regla.VigenciaInicio.Value}");
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"[Scan] ℹ️ Sin vigencia inicio (siempre válida)");
                        }

                        // Verificar VigenciaFin
                        if (regla.VigenciaFin.HasValue)
                        {
                            if (ahora > regla.VigenciaFin.Value)
                            {
                                todasLasCondicionesSeCumplen = false;
                                System.Diagnostics.Debug.WriteLine($"[Scan] ❌ Fuera de vigencia fin: {ahora} > {regla.VigenciaFin.Value}");
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"[Scan] ✅ Dentro de vigencia fin: {ahora} <= {regla.VigenciaFin.Value}");
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"[Scan] ℹ️ Sin vigencia fin (siempre válida)");
                        }

                        // Verificar VentanaHoraria
                        if (!string.IsNullOrWhiteSpace(regla.VentanaHoraria))
                        {
                            bool enVentana = EstaEnVentanaHoraria(horaActual, regla.VentanaHoraria);
                            if (!enVentana)
                            {
                                todasLasCondicionesSeCumplen = false;
                                System.Diagnostics.Debug.WriteLine($"[Scan] ❌ Fuera de ventana horaria: {horaActual} no está en '{regla.VentanaHoraria}'");
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"[Scan] ✅ Dentro de ventana horaria: {horaActual} está en '{regla.VentanaHoraria}'");
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"[Scan] ℹ️ Sin ventana horaria (siempre válida)");
                        }

                        // Resultado de la evaluación
                        System.Diagnostics.Debug.WriteLine($"[Scan] === RESULTADO REGLA DENEGAR ===");
                        if (todasLasCondicionesSeCumplen)
                        {
                            accesoPermitido = false;
                            motivoDenegacion = $"Acceso denegado por regla de seguridad para rol '{regla.Rol}'";
                            if (!string.IsNullOrWhiteSpace(regla.VentanaHoraria))
                                motivoDenegacion += $" (Horario: {regla.VentanaHoraria})";

                            System.Diagnostics.Debug.WriteLine($"[Scan] ❌ ACCESO DENEGADO - Todas las condiciones de regla DENEGAR se cumplen");
                            System.Diagnostics.Debug.WriteLine($"[Scan] Motivo: {motivoDenegacion}");

                            if (regla.RequiereBiometriaConfirmacion)
                            {
                                requiereHuella = true;
                                System.Diagnostics.Debug.WriteLine($"[Scan] ⚠️ Regla requiere biometría para confirmación");
                            }

                            break; // Una regla de denegación que se cumple es suficiente
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"[Scan] ✅ Regla DENEGAR no aplica - alguna condición no se cumple");
                        }
                    }

                    // === PASO 2: VERIFICAR REGLAS DE PERMITIR ===
                    if (accesoPermitido)
                    {
                        System.Diagnostics.Debug.WriteLine($"[Scan] === PASO 2: EVALUANDO REGLAS DE PERMITIR ===");
                        var reglasPermitir = reglasAplicables.Where(r => r.Politica == AccesoTipo.Permitir).ToList();
                        System.Diagnostics.Debug.WriteLine($"[Scan] Reglas de PERMITIR a evaluar: {reglasPermitir.Count}");

                        bool tieneReglaPermitir = false;

                        foreach (var regla in reglasPermitir)
                        {
                            System.Diagnostics.Debug.WriteLine($"[Scan] --- Evaluando regla PERMITIR ---");
                            System.Diagnostics.Debug.WriteLine($"[Scan] Rol: '{regla.Rol}' | Ventana: '{regla.VentanaHoraria}' | Prioridad: {regla.Prioridad}");
                            System.Diagnostics.Debug.WriteLine($"[Scan] Vigencia: {regla.VigenciaInicio?.ToString("yyyy-MM-dd") ?? "null"} - {regla.VigenciaFin?.ToString("yyyy-MM-dd") ?? "null"}");

                            bool todasLasCondicionesSeCumplen = true;

                            // Verificar que el usuario tenga el rol requerido por la regla
                            if (!string.IsNullOrWhiteSpace(regla.Rol) && !tiposRolesUsuario.Contains(regla.Rol))
                            {
                                todasLasCondicionesSeCumplen = false;
                                System.Diagnostics.Debug.WriteLine($"[Scan] ❌ Rol no coincide: usuario no tiene rol '{regla.Rol}'");
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"[Scan] ✅ Rol coincide o regla sin rol específico");
                            }

                            // Verificar VigenciaInicio
                            if (regla.VigenciaInicio.HasValue)
                            {
                                if (ahora < regla.VigenciaInicio.Value)
                                {
                                    todasLasCondicionesSeCumplen = false;
                                    System.Diagnostics.Debug.WriteLine($"[Scan] ❌ Fuera de vigencia inicio: {ahora} < {regla.VigenciaInicio.Value}");
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine($"[Scan] ✅ Dentro de vigencia inicio: {ahora} >= {regla.VigenciaInicio.Value}");
                                }
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"[Scan] ℹ️ Sin vigencia inicio (siempre válida)");
                            }

                            // Verificar VigenciaFin
                            if (regla.VigenciaFin.HasValue)
                            {
                                if (ahora > regla.VigenciaFin.Value)
                                {
                                    todasLasCondicionesSeCumplen = false;
                                    System.Diagnostics.Debug.WriteLine($"[Scan] ❌ Fuera de vigencia fin: {ahora} > {regla.VigenciaFin.Value}");
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine($"[Scan] ✅ Dentro de vigencia fin: {ahora} <= {regla.VigenciaFin.Value}");
                                }
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"[Scan] ℹ️ Sin vigencia fin (siempre válida)");
                            }

                            // Verificar VentanaHoraria
                            if (!string.IsNullOrWhiteSpace(regla.VentanaHoraria))
                            {
                                bool enVentana = EstaEnVentanaHoraria(horaActual, regla.VentanaHoraria);
                                if (!enVentana)
                                {
                                    todasLasCondicionesSeCumplen = false;
                                    System.Diagnostics.Debug.WriteLine($"[Scan] ❌ Fuera de ventana horaria: {horaActual} no está en '{regla.VentanaHoraria}'");
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine($"[Scan] ✅ Dentro de ventana horaria: {horaActual} está en '{regla.VentanaHoraria}'");
                                }
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"[Scan] ℹ️ Sin ventana horaria (siempre válida)");
                            }

                            // Resultado de la evaluación
                            System.Diagnostics.Debug.WriteLine($"[Scan] === RESULTADO REGLA PERMITIR ===");
                            if (todasLasCondicionesSeCumplen)
                            {
                                tieneReglaPermitir = true;
                                System.Diagnostics.Debug.WriteLine($"[Scan] ✅ ACCESO PERMITIDO - Todas las condiciones de regla PERMITIR se cumplen");

                                if (regla.RequiereBiometriaConfirmacion)
                                {
                                    requiereHuella = true;
                                    System.Diagnostics.Debug.WriteLine($"[Scan] ⚠️ Regla requiere biometría para confirmación");
                                }

                                break; // Una regla de permitir que se cumple es suficiente
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"[Scan] ✅ Regla PERMITIR no aplica - alguna condición no se cumple");
                            }
                        }

                        // Verificar si hay reglas PERMITIR pero ninguna se cumple
                        if (!tieneReglaPermitir && reglasPermitir.Any())
                        {
                            accesoPermitido = false;
                            motivoDenegacion = "No se cumplen las condiciones de acceso requeridas para ninguno de tus roles";
                            System.Diagnostics.Debug.WriteLine($"[Scan] ❌ ACCESO DENEGADO - hay reglas PERMITIR pero ninguna se cumple completamente");
                        }
                        else if (tieneReglaPermitir)
                        {
                            System.Diagnostics.Debug.WriteLine($"[Scan] ✅ Al menos una regla PERMITIR se cumple - acceso autorizado");
                        }
                        else if (!reglasPermitir.Any())
                        {
                            System.Diagnostics.Debug.WriteLine($"[Scan] ℹ️ No hay reglas PERMITIR - acceso permitido por defecto");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[Scan] ⏭️ Salteando evaluación de reglas PERMITIR (ya fue denegado)");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[Scan] ℹ️ No hay reglas específicas para este espacio - acceso permitido por defecto");
                }

                // === VERIFICACIÓN BIOMÉTRICA ADICIONAL ===
                if (accesoPermitido && requiereHuella)
                {
                    System.Diagnostics.Debug.WriteLine($"[Scan] === VERIFICACIÓN BIOMÉTRICA ADICIONAL REQUERIDA ===");
                    var biometricConfirm = await _biometricService.AuthenticateAsync(
                        "Confirmar acceso con huella digital");

                    if (!biometricConfirm.Success)
                    {
                        accesoPermitido = false;
                        motivoDenegacion = "Fallo en verificación biométrica requerida";
                        System.Diagnostics.Debug.WriteLine($"[Scan] ❌ Biometría fallida: {biometricConfirm.ErrorMessage}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[Scan] ✅ Biometría confirmada exitosamente");
                    }
                }

                // === RESULTADO FINAL ===
                System.Diagnostics.Debug.WriteLine($"[Scan] === RESULTADO FINAL ===");
                System.Diagnostics.Debug.WriteLine($"[Scan] Acceso permitido: {accesoPermitido}");
                if (!accesoPermitido)
                {
                    System.Diagnostics.Debug.WriteLine($"[Scan] Motivo denegación: {motivoDenegacion}");
                }

                if (!accesoPermitido)
                {
                    System.Diagnostics.Debug.WriteLine($"[Scan] === PROCESANDO ACCESO DENEGADO ===");
                    await DisplayAlert("Acceso Denegado", motivoDenegacion, "Cerrar");

                    var eventoDenegado = new EventoAcceso
                    {
                        MomentoDeAcceso = DateTime.UtcNow,
                        CredencialIdApi = cred.idApi,
                        EspacioIdApi = espacio.idApi,
                        Credencial = cred,
                        Espacio = espacio,
                        Resultado = AccesoTipo.Denegar,
                        Motivo = motivoDenegacion
                    };

                    await _db.SaveAndPushEventoAccesoAsync(eventoDenegado);
                    System.Diagnostics.Debug.WriteLine($"[Scan] ✅ Evento de acceso denegado guardado");
                    return;
                }

                // === PROCESANDO ACCESO PERMITIDO ===
                System.Diagnostics.Debug.WriteLine($"[Scan] === PROCESANDO ACCESO PERMITIDO ===");
                var popupOk = new ScanResultPopup("Acceso Autorizado",
                    $"El usuario tiene permiso para acceder al espacio '{espacio.Nombre}'.", true);
                await this.ShowPopupAsync(popupOk);

                var ev = new EventoAcceso
                {
                    MomentoDeAcceso = DateTime.UtcNow,
                    CredencialId = cred.CredencialId,
                    EspacioId = espacio.EspacioId,
                    CredencialIdApi = cred.idApi,
                    EspacioIdApi = espacio.idApi,
                    Credencial = cred,
                    Espacio = espacio,
                    Resultado = AccesoTipo.Permitir,
                    Motivo = "Acceso autorizado por reglas de acceso"
                };

                await _db.SaveAndPushEventoAccesoAsync(ev);
                System.Diagnostics.Debug.WriteLine($"[Scan] ✅ Evento de acceso permitido guardado");
                System.Diagnostics.Debug.WriteLine($"[Scan] === PROCESAMIENTO COMPLETADO EXITOSAMENTE ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Scan] ❌❌❌ ERROR CRÍTICO EN HandleScannedPayloadAsync ❌❌❌");
                System.Diagnostics.Debug.WriteLine($"[Scan] Mensaje: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[Scan] StackTrace: {ex.StackTrace}");
                System.Diagnostics.Debug.WriteLine($"[Scan] InnerException: {ex.InnerException?.Message}");
                await DisplayAlert("Error", $"Error procesando el codigo QR: {ex.Message}", "OK");
            }
        }
    }   
}