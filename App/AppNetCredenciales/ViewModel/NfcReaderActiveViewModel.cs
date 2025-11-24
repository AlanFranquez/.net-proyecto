using AppNetCredenciales.Data;
using AppNetCredenciales.Services;
using AppNetCredenciales.models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Timers;
using static AppNetCredenciales.Services.ApiService;

namespace AppNetCredenciales.ViewModel
{
    [QueryProperty(nameof(EspacioId), "espacioId")]
    public class NfcReaderActiveViewModel : INotifyPropertyChanged
    {
        private readonly LocalDBService _db;
        private readonly NfcService _nfcService;
        private readonly ApiService _apiService;
        private readonly BiometricService _biometricService;
        
        private System.Timers.Timer? _timer;
        private DateTime _activationTime;
        private string _espacioId;
        private string _espacioNombre;
        private string _activeTime;
        private int _totalReads;
        private int _successReads;
        private int _failedReads;
        private bool _isActive;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string EspacioId
        {
            get => _espacioId;
            set
            {
                _espacioId = value;
                OnPropertyChanged();
                _ = LoadEspacioAsync();
            }
        }

        public string EspacioNombre
        {
            get => _espacioNombre;
            set
            {
                _espacioNombre = value;
                OnPropertyChanged();
            }
        }

        public string ActiveTime
        {
            get => _activeTime;
            set
            {
                _activeTime = value;
                OnPropertyChanged();
            }
        }

        public int TotalReads
        {
            get => _totalReads;
            set
            {
                _totalReads = value;
                OnPropertyChanged();
            }
        }

        public int SuccessReads
        {
            get => _successReads;
            set
            {
                _successReads = value;
                OnPropertyChanged();
            }
        }

        public int FailedReads
        {
            get => _failedReads;
            set
            {
                _failedReads = value;
                OnPropertyChanged();
            }
        }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                OnPropertyChanged();
            }
        }

        public ICommand DeactivateCommand { get; }
        public ICommand BackCommand { get; }

        public NfcReaderActiveViewModel(LocalDBService db, NfcService nfcService, ApiService apiService, BiometricService biometricService)
        {
            _db = db;
            _nfcService = nfcService;
            _apiService = apiService;
            _biometricService = biometricService;
            
            _activeTime = "00:00:00";
            
            DeactivateCommand = new Command(async () => await DeactivateReaderAsync());
            BackCommand = new Command(async () => await GoBackAsync());

            // Suscribirse a eventos NFC
            _nfcService.TagRead += OnTagRead;
            _nfcService.Error += OnNfcError;
        }

        private async Task LoadEspacioAsync()
        {
            try
            {
                var espacios = await _db.GetEspaciosAsync();
                var espacio = espacios?.FirstOrDefault(e => e.idApi == EspacioId);
                
                if (espacio != null)
                {
                    EspacioNombre = espacio.Nombre ?? "Espacio desconocido";
                    System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] Espacio cargado: {EspacioNombre}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] No se encontró espacio con ID: {EspacioId}");
                    EspacioNombre = "Espacio no encontrado";
                }

                // Activar el lector
                await ActivateReaderAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] Error cargando espacio: {ex.Message}");
            }
        }

        private async Task ActivateReaderAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("???????????????????????????????????????");
                System.Diagnostics.Debug.WriteLine("[NfcReaderActive] ?? ACTIVANDO LECTOR NFC");
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] ?? Espacio: {EspacioNombre}");
                System.Diagnostics.Debug.WriteLine("???????????????????????????????????????");

                if (!_nfcService.IsAvailable)
                {
                    await Shell.Current.DisplayAlert(
                        "NFC No Disponible",
                        "Este dispositivo no tiene NFC",
                        "OK");
                    await Shell.Current.GoToAsync("..");
                    return;
                }

                if (!_nfcService.IsEnabled)
                {
                    bool shouldOpenSettings = await Shell.Current.DisplayAlert(
                        "NFC Deshabilitado",
                        "NFC está deshabilitado.\n\n¿Deseas abrir la configuración?",
                        "Sí",
                        "No");
                    
                    if (shouldOpenSettings)
                    {
#if ANDROID
                        var intent = new Android.Content.Intent(Android.Provider.Settings.ActionNfcSettings);
                        intent.AddFlags(Android.Content.ActivityFlags.NewTask);
                        Android.App.Application.Context.StartActivity(intent);
#endif
                    }
                    
                    await Shell.Current.GoToAsync("..");
                    return;
                }

                // Iniciar modo lector
                _nfcService.StartListening();
                
                IsActive = true;
                _activationTime = DateTime.Now;
                StartTimer();

                System.Diagnostics.Debug.WriteLine("???????????????????????????????????????");
                System.Diagnostics.Debug.WriteLine("[NfcReaderActive] ? LECTOR NFC ACTIVADO");
                System.Diagnostics.Debug.WriteLine("[NfcReaderActive] ?? Esperando tags...");
                System.Diagnostics.Debug.WriteLine("???????????????????????????????????????");
                
                await Shell.Current.DisplayAlert(
                    "? Lector Activado",
                    $"Lector NFC activo para el espacio:\n{EspacioNombre}\n\nAcerca el dispositivo del usuario.",
                    "Entendido");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] ? Error activando lector: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "No se pudo activar el lector NFC", "OK");
                await Shell.Current.GoToAsync("..");
            }
        }

        private async Task DeactivateReaderAsync()
        {
            try
            {
                bool confirm = await Shell.Current.DisplayAlert(
                    "Confirmar",
                    "¿Deseas desactivar el lector NFC?",
                    "Sí",
                    "No");

                if (!confirm) return;

                System.Diagnostics.Debug.WriteLine("[NfcReaderActive] ?? Desactivando lector...");

                _nfcService.StopListening();
                StopTimer();
                IsActive = false;

                System.Diagnostics.Debug.WriteLine("???????????????????????????????????????");
                System.Diagnostics.Debug.WriteLine("[NfcReaderActive] ? LECTOR DESACTIVADO");
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] ?? Total: {TotalReads}");
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] ? Exitosas: {SuccessReads}");
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] ? Fallidas: {FailedReads}");
                System.Diagnostics.Debug.WriteLine("???????????????????????????????????????");

                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] Error desactivando lector: {ex.Message}");
            }
        }

        private async Task GoBackAsync()
        {
            try
            {
                if (IsActive)
                {
                    bool confirm = await Shell.Current.DisplayAlert(
                        "Lector Activo",
                        "El lector está activo. ¿Deseas desactivarlo y salir?",
                        "Sí",
                        "No");

                    if (!confirm) return;

                    _nfcService.StopListening();
                    StopTimer();
                    IsActive = false;
                }

                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] Error en GoBack: {ex.Message}");
            }
        }

        private void StartTimer()
        {
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += OnTimerElapsed;
            _timer.Start();
        }

        private void StopTimer()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Elapsed -= OnTimerElapsed;
                _timer.Dispose();
                _timer = null;
            }
        }

        private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            var elapsed = DateTime.Now - _activationTime;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ActiveTime = $"{elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
            });
        }

        /// <summary>
        /// Evento cuando se lee un tag NFC
        /// </summary>
        private void OnTagRead(object? sender, string idCriptografico)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await ProcessCredentialAsync(idCriptografico);
            });
        }

        /// <summary>
        /// Verifica si la hora actual está dentro de la ventana horaria especificada
        /// </summary>
        private bool EstaEnVentanaHoraria(TimeSpan horaActual, string ventanaHoraria)
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
                        // Ventana horaria normal (ej: 08:00-17:00)
                        return horaActual >= inicio && horaActual <= fin;
                    }
                    else
                    {
                        // Ventana horaria overnight (ej: 22:00-02:00)
                        return horaActual >= inicio || horaActual <= fin;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] Error parseando ventana horaria '{ventanaHoraria}': {ex.Message}");
            }

            return true; // En caso de error, no restringir
        }

        /// <summary>
        /// Procesa la credencial leída con evaluación de reglas de acceso
        /// </summary>
        private async Task ProcessCredentialAsync(string idCriptografico)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("???????????????????????????????????????????????????");
                System.Diagnostics.Debug.WriteLine("?  ?? PROCESANDO CREDENCIAL NFC                   ?");
                System.Diagnostics.Debug.WriteLine("???????????????????????????????????????????????????");
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] ?? IdCriptográfico: {idCriptografico}");

                TotalReads++;

                // === BUSCAR CREDENCIAL ===
                var credenciales = await _db.GetCredencialesAsync();
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] ?? Credenciales en BD: {credenciales?.Count() ?? 0}");

                var credencial = credenciales?.FirstOrDefault(c => 
                    c.IdCriptografico != null && 
                    c.IdCriptografico.Equals(idCriptografico, StringComparison.OrdinalIgnoreCase));

                if (credencial == null)
                {
                    System.Diagnostics.Debug.WriteLine("[NfcReaderActive] ? Credencial NO encontrada");
                    FailedReads++;
                    
                    await CreateEventoAccesoAsync(
                        credencialId: null,
                        resultado: "Denegar",
                        motivo: $"Credencial no encontrada - ID: {idCriptografico.Substring(0, Math.Min(8, idCriptografico.Length))}..."
                    );
                    
                    await Shell.Current.DisplayAlert(
                        "?? Acceso Denegado",
                        $"Credencial no válida o no registrada.\n\nID: {idCriptografico.Substring(0, Math.Min(8, idCriptografico.Length))}...",
                        "OK");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] ? Credencial encontrada:");
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive]    - ID API: {credencial.idApi}");
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive]    - Estado: {credencial.Estado}");
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive]    - Tipo: {credencial.Tipo}");
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive]    - Expiración: {credencial.FechaExpiracion}");

                // === BUSCAR ESPACIO ===
                var espacios = await _db.GetEspaciosAsync();
                var espacio = espacios?.FirstOrDefault(e => e.idApi == EspacioId);

                if (espacio == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] ? Espacio NO encontrado para ID: {EspacioId}");
                    FailedReads++;
                    
                    await CreateEventoAccesoAsync(
                        credencialId: credencial.idApi,
                        resultado: "Denegar",
                        motivo: "Espacio no encontrado"
                    );
                    
                    await Shell.Current.DisplayAlert(
                        "?? Acceso Denegado",
                        "Espacio no encontrado.",
                        "OK");
                    return;
                }

                // === VERIFICAR ESTADO DE CREDENCIAL ===
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] === VERIFICANDO ESTADO DE CREDENCIAL ===");
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] Estado actual: {credencial.Estado}");
                
                // Permitir credenciales en estado "Emitida" o "Activada"
                if (credencial.Estado != models.CredencialEstado.Emitida && 
                    credencial.Estado != models.CredencialEstado.Activada)
                {
                    System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] ? Estado inválido: {credencial.Estado}");
                    FailedReads++;
                    
                    await CreateEventoAccesoAsync(
                        credencialId: credencial.idApi,
                        resultado: "Denegar",
                        motivo: $"Estado no válido: {credencial.Estado}"
                    );
                    
                    await Shell.Current.DisplayAlert(
                        "?? Acceso Denegado",
                        $"La credencial está en estado: {credencial.Estado}\n\n" +
                        $"Solo se permiten credenciales Emitidas o Activadas.",
                        "OK");
                    return;
                }
                
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] ? Estado de credencial válido: {credencial.Estado}");

                // === VERIFICAR EXPIRACIÓN ===
                if (credencial.FechaExpiracion.HasValue)
                {
                    System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] Fecha expiración: {credencial.FechaExpiracion.Value:yyyy-MM-dd}, Hoy: {DateTime.Today:yyyy-MM-dd}");
                    if (credencial.FechaExpiracion.Value.Date < DateTime.Today)
                    {
                        System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] ? Credencial expirada");
                        FailedReads++;
                        
                        await CreateEventoAccesoAsync(
                            credencialId: credencial.idApi,
                            resultado: "Denegar",
                            motivo: $"Credencial expirada el {credencial.FechaExpiracion.Value:dd/MM/yyyy}"
                        );
                        
                        await Shell.Current.DisplayAlert(
                            "?? Acceso Denegado",
                            $"Credencial expirada el {credencial.FechaExpiracion.Value:dd/MM/yyyy}.",
                            "OK");
                        return;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] ? Credencial vigente");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] ?? Credencial sin fecha de expiración (vigente indefinidamente)");
                }

                // === OBTENER USUARIO ===
                var usuarios = await _db.GetUsuariosAsync();
                var usuario = usuarios?.FirstOrDefault(u => u.idApi == credencial.usuarioIdApi);
                
                if (usuario == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] ? Usuario NO encontrado para ID API: '{credencial.usuarioIdApi}'");
                    FailedReads++;
                    
                    await CreateEventoAccesoAsync(
                        credencialId: credencial.idApi,
                        resultado: "Denegar",
                        motivo: "Usuario no encontrado"
                    );
                    
                    await Shell.Current.DisplayAlert(
                        "?? Acceso Denegado",
                        "No se pudo encontrar el usuario asociado a la credencial.",
                        "OK");
                    return;
                }

                string nombreUsuario = $"{usuario.Nombre} {usuario.Apellido}".Trim();
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] ? Usuario: {nombreUsuario}");

                // === OBTENER ROLES DEL USUARIO ===
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] === OBTENIENDO ROLES DEL USUARIO ===");
                var roles = await _db.GetRolesAsync();
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] Total roles en BD: {roles.Count}");

                var rolesUsuario = new List<Rol>();
                var rolesIDsUsuario = usuario.RolesIDs ?? Array.Empty<string>();
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] Roles IDs del usuario: [{string.Join(", ", rolesIDsUsuario)}]");

                foreach (var rolId in rolesIDsUsuario)
                {
                    var rol = roles.FirstOrDefault(r => r.idApi == rolId);
                    if (rol != null)
                    {
                        rolesUsuario.Add(rol);
                        System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] ? Rol encontrado: {rol.Tipo} (ID API: {rol.idApi})");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] ?? Rol no encontrado para ID: {rolId}");
                    }
                }

                var tiposRolesUsuario = rolesUsuario.Select(r => r.Tipo).Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] Tipos de roles del usuario: [{string.Join(", ", tiposRolesUsuario)}]");

                // === EVALUACIÓN DE REGLAS DE ACCESO ===
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] === INICIANDO EVALUACIÓN DE REGLAS DE ACCESO ===");
                bool requiereHuella = false;

                var reglas = await _db.GetReglasAccesoAsync();
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] Total reglas en BD: {reglas.Count}");

                var reglasEspacio = reglas
                    .Where(regla => regla.EspaciosIDs != null &&
                                   regla.EspaciosIDs.Contains(espacio.idApi ?? string.Empty))
                    .OrderBy(regla => regla.Prioridad)
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] Reglas aplicables al espacio '{espacio.Nombre}': {reglasEspacio.Count}");
                foreach (var regla in reglasEspacio)
                {
                    System.Diagnostics.Debug.WriteLine($"[NfcReaderActive]   - Regla: {regla.Politica} | Rol: '{regla.Rol}' | Prioridad: {regla.Prioridad} | Ventana: '{regla.VentanaHoraria}'");
                }

                bool accesoPermitido = true;
                string motivoDenegacion = "";

                if (reglasEspacio.Count > 0)
                {
                    var ahora = DateTime.Now;
                    var horaActual = ahora.TimeOfDay;
                    System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] Fecha/hora actual: {ahora:yyyy-MM-dd HH:mm:ss} (TimeSpan: {horaActual})");

                    // Filtrar reglas aplicables por tipo de rol del usuario
                    var reglasAplicables = reglasEspacio
                        .Where(r => string.IsNullOrWhiteSpace(r.Rol) || tiposRolesUsuario.Contains(r.Rol))
                        .ToList();

                    System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] Reglas aplicables al usuario (por rol): {reglasAplicables.Count}");

                    // === PASO 1: VERIFICAR REGLAS DE DENEGACIÓN ===
                    System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] === PASO 1: EVALUANDO REGLAS DE DENEGACIÓN ===");
                    var reglasDenegar = reglasAplicables.Where(r => r.Politica == AccesoTipo.Denegar).ToList();
                    System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] Reglas de DENEGAR a evaluar: {reglasDenegar.Count}");

                    foreach (var regla in reglasDenegar)
                    {
                        System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] --- Evaluando regla DENEGAR ---");
                        System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] Rol: '{regla.Rol}' | Ventana: '{regla.VentanaHoraria}' | Prioridad: {regla.Prioridad}");

                        bool todasLasCondicionesSeCumplen = true;

                        // Verificar rol
                        if (!string.IsNullOrWhiteSpace(regla.Rol) && !tiposRolesUsuario.Contains(regla.Rol))
                        {
                            todasLasCondicionesSeCumplen = false;
                            System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] ? Rol no coincide");
                        }

                        // Verificar VigenciaInicio
                        if (todasLasCondicionesSeCumplen && regla.VigenciaInicio.HasValue)
                        {
                            if (ahora < regla.VigenciaInicio.Value)
                            {
                                todasLasCondicionesSeCumplen = false;
                                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] ? Fuera de vigencia inicio");
                            }
                        }

                        // Verificar VigenciaFin
                        if (todasLasCondicionesSeCumplen && regla.VigenciaFin.HasValue)
                        {
                            if (ahora > regla.VigenciaFin.Value)
                            {
                                todasLasCondicionesSeCumplen = false;
                                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] ? Fuera de vigencia fin");
                            }
                        }

                        // Verificar VentanaHoraria
                        if (todasLasCondicionesSeCumplen && !string.IsNullOrWhiteSpace(regla.VentanaHoraria))
                        {
                            bool enVentana = EstaEnVentanaHoraria(horaActual, regla.VentanaHoraria);
                            if (!enVentana)
                            {
                                todasLasCondicionesSeCumplen = false;
                                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] ? Fuera de ventana horaria");
                            }
                        }

                        if (todasLasCondicionesSeCumplen)
                        {
                            accesoPermitido = false;
                            motivoDenegacion = $"Acceso denegado por regla de seguridad para rol '{regla.Rol}'";
                            if (!string.IsNullOrWhiteSpace(regla.VentanaHoraria))
                                motivoDenegacion += $" (Horario: {regla.VentanaHoraria})";

                            System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] ? ACCESO DENEGADO - Regla DENEGAR aplicada");
                            System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] Motivo: {motivoDenegacion}");

                            if (regla.RequiereBiometriaConfirmacion)
                            {
                                requiereHuella = true;
                                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] ?? Regla requiere biometría para confirmación");
                            }

                            break;
                        }
                    }

                    // === PASO 2: VERIFICAR REGLAS DE PERMITIR ===
                    if (accesoPermitido)
                    {
                        System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] === PASO 2: EVALUANDO REGLAS DE PERMITIR ===");
                        var reglasPermitir = reglasAplicables.Where(r => r.Politica == AccesoTipo.Permitir).ToList();
                        System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] Reglas de PERMITIR a evaluar: {reglasPermitir.Count}");

                        bool tieneReglaPermitir = false;

                        foreach (var regla in reglasPermitir)
                        {
                            System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] --- Evaluando regla PERMITIR ---");

                            bool todasLasCondicionesSeCumplen = true;

                            // Verificar rol
                            if (!string.IsNullOrWhiteSpace(regla.Rol) && !tiposRolesUsuario.Contains(regla.Rol))
                            {
                                todasLasCondicionesSeCumplen = false;
                            }

                            // Verificar VigenciaInicio
                            if (todasLasCondicionesSeCumplen && regla.VigenciaInicio.HasValue && ahora < regla.VigenciaInicio.Value)
                            {
                                todasLasCondicionesSeCumplen = false;
                            }

                            // Verificar VigenciaFin
                            if (todasLasCondicionesSeCumplen && regla.VigenciaFin.HasValue && ahora > regla.VigenciaFin.Value)
                            {
                                todasLasCondicionesSeCumplen = false;
                            }

                            // Verificar VentanaHoraria
                            if (todasLasCondicionesSeCumplen && !string.IsNullOrWhiteSpace(regla.VentanaHoraria))
                            {
                                bool enVentana = EstaEnVentanaHoraria(horaActual, regla.VentanaHoraria);
                                if (!enVentana)
                                {
                                    todasLasCondicionesSeCumplen = false;
                                }
                            }

                            if (todasLasCondicionesSeCumplen)
                            {
                                tieneReglaPermitir = true;
                                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] ? ACCESO PERMITIDO - Regla PERMITIR aplicada");

                                if (regla.RequiereBiometriaConfirmacion)
                                {
                                    requiereHuella = true;
                                    System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] ?? Regla requiere biometría para confirmación");
                                }

                                break;
                            }
                        }

                        // Si hay reglas de PERMITIR pero ninguna aplica, denegar
                        if (reglasPermitir.Count > 0 && !tieneReglaPermitir)
                        {
                            accesoPermitido = false;
                            motivoDenegacion = "No cumple ninguna regla de acceso permitido";
                            System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] ? ACCESO DENEGADO - No cumple reglas PERMITIR");
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] ?? No hay reglas de acceso para este espacio - Acceso permitido por defecto");
                }

                // === VERIFICAR BIOMETRÍA SI ES REQUERIDA ===
                if (requiereHuella)
                {
                    System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] === VERIFICACIÓN BIOMÉTRICA REQUERIDA ===");
                    
                    // Mostrar instrucción al usuario antes del prompt biométrico
                    await Shell.Current.DisplayAlert(
                        "?? Verificación Biométrica Requerida",
                        $"?? {nombreUsuario}\n\n" +
                        $"Esta área requiere verificación adicional.\n\n" +
                        $"?? Por favor, coloca tu huella dactilar en el lector biométrico de este dispositivo.",
                        "Continuar");
                    
                    var biometriaResult = await _biometricService.AuthenticateAsync($"Verificar identidad de {nombreUsuario}");
                    
                    if (!biometriaResult.Success)
                    {
                        System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] ? Verificación biométrica fallida: {biometriaResult.ErrorMessage}");
                        FailedReads++;
                        
                        await CreateEventoAccesoAsync(
                            credencialId: credencial.idApi,
                            resultado: "Denegar",
                            motivo: $"Verificación biométrica fallida: {biometriaResult.ErrorMessage}"
                        );
                        
                        await Shell.Current.DisplayAlert(
                            "?? Acceso Denegado",
                            $"La verificación biométrica falló:\n\n{biometriaResult.ErrorMessage}\n\n" +
                            $"El acceso ha sido denegado por motivos de seguridad.",
                            "OK");
                        return;
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] ? Verificación biométrica exitosa");
                    
                    // Confirmar éxito de la verificación biométrica
                    await Shell.Current.DisplayAlert(
                        "? Verificación Exitosa",
                        $"Huella dactilar verificada correctamente.\n\nProcesando acceso...",
                        "OK");
                }

                // === RESULTADO FINAL ===
                string resultado = accesoPermitido ? "Permitir" : "Denegar";
                string motivo = accesoPermitido 
                    ? $"Acceso concedido - Credencial {credencial.Tipo} válida" 
                    : motivoDenegacion;

                await CreateEventoAccesoAsync(
                    credencialId: credencial.idApi,
                    resultado: resultado,
                    motivo: motivo
                );

                if (accesoPermitido)
                {
                    SuccessReads++;
                    
                    System.Diagnostics.Debug.WriteLine("???????????????????????????????????????????????????");
                    System.Diagnostics.Debug.WriteLine("?  ? ACCESO CONCEDIDO                          ?");
                    System.Diagnostics.Debug.WriteLine("???????????????????????????????????????????????????");

                    await Shell.Current.DisplayAlert(
                        "? Acceso Concedido",
                        $"? Credencial {credencial.Tipo} válida\n\n" +
                        $"?? {nombreUsuario}\n" +
                        $"?? {EspacioNombre}\n" +
                        $"?? Válida hasta: {credencial.FechaExpiracion?.ToString("dd/MM/yyyy") ?? "Indefinida"}\n\n" +
                        $"?? {DateTime.Now:HH:mm:ss}",
                        "OK");
                }
                else
                {
                    FailedReads++;
                    
                    System.Diagnostics.Debug.WriteLine("???????????????????????????????????????????????????");
                    System.Diagnostics.Debug.WriteLine("?  ? ACCESO DENEGADO                           ?");
                    System.Diagnostics.Debug.WriteLine("???????????????????????????????????????????????????");

                    await Shell.Current.DisplayAlert(
                        "?? Acceso Denegado",
                        $"? Credencial NO válida\n\n" +
                        $"?? {nombreUsuario}\n" +
                        $"?? Motivo: {motivoDenegacion}\n\n" +
                        $"?? {DateTime.Now:HH:mm:ss}",
                        "OK");
                }

                System.Diagnostics.Debug.WriteLine("???????????????????????????????????????????????????");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] ? Error procesando credencial: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] StackTrace: {ex.StackTrace}");
                FailedReads++;
            }
        }

        /// <summary>
        /// Crea un evento de acceso en la API
        /// </summary>
        private async Task CreateEventoAccesoAsync(string? credencialId, string resultado, string motivo)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("????????????????????????????????????????????");
                System.Diagnostics.Debug.WriteLine("?? CREANDO EVENTO DE ACCESO EN API");
                System.Diagnostics.Debug.WriteLine("????????????????????????????????????????????");

                var eventoDto = new EventoAccesoDto
                {
                    MomentoDeAcceso = DateTime.UtcNow,
                    CredencialId = credencialId ?? Guid.Empty.ToString(),
                    EspacioId = EspacioId,
                    Resultado = resultado,
                    Motivo = motivo,
                    Modo = "Online",
                    Firma = $"NFC-Reader-{DateTime.Now:yyyyMMddHHmmss}"
                };

                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] ?? Datos del evento:");
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive]    - Momento: {eventoDto.MomentoDeAcceso:yyyy-MM-dd HH:mm:ss} UTC");
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive]    - CredencialId: {eventoDto.CredencialId}");
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive]    - EspacioId: {eventoDto.EspacioId}");
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive]    - Resultado: {eventoDto.Resultado}");
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive]    - Motivo: {eventoDto.Motivo}");
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive]    - Modo: {eventoDto.Modo}");

                var response = await _apiService.CreateEventoAccesoAsync(eventoDto);

                if (response != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] ? Evento creado exitosamente");
                    System.Diagnostics.Debug.WriteLine($"[NfcReaderActive]    - EventoId: {response.EventoAccesoId ?? response.Id}");
                    
                    // Guardar en base de datos local
                    await SaveEventoLocalAsync(response);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] ?? No se pudo crear el evento en la API");
                    
                    // Guardar localmente para sincronizar después (modo offline)
                    await SaveEventoLocalAsync(eventoDto, isOffline: true);
                }

                System.Diagnostics.Debug.WriteLine("????????????????????????????????????????????");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] ? Error creando evento: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] StackTrace: {ex.StackTrace}");
                
                // Guardar localmente como fallback
                try
                {
                    var eventoDto = new EventoAccesoDto
                    {
                        MomentoDeAcceso = DateTime.UtcNow,
                        CredencialId = credencialId ?? Guid.Empty.ToString(),
                        EspacioId = EspacioId,
                        Resultado = resultado,
                        Motivo = motivo,
                        Modo = "Offline",
                        Firma = $"NFC-Reader-{DateTime.Now:yyyyMMddHHmmss}"
                    };
                    
                    await SaveEventoLocalAsync(eventoDto, isOffline: true);
                }
                catch (Exception saveEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] ? Error guardando evento local: {saveEx.Message}");
                }
            }
        }

        /// <summary>
        /// Guarda el evento en la base de datos local
        /// </summary>
        private async Task SaveEventoLocalAsync(EventoAccesoDto eventoDto, bool isOffline = false)
        {
            try
            {
                var eventoLocal = new EventoAcceso
                {
                    idApi = eventoDto.EventoAccesoId ?? eventoDto.Id,
                    MomentoDeAcceso = eventoDto.MomentoDeAcceso,
                    CredencialIdApi = eventoDto.CredencialId,
                    EspacioIdApi = eventoDto.EspacioId,
                    Resultado = eventoDto.Resultado == "Permitir" ? AccesoTipo.Permitir : AccesoTipo.Denegar,
                    Motivo = eventoDto.Motivo,
                    Modo = isOffline ? Modo.Offline : Modo.Online,
                    Firma = eventoDto.Firma
                };

                await _db.SaveEventoAccesoAsync(eventoLocal);
                
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] ?? Evento guardado en BD local");
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive]    - Modo: {eventoLocal.Modo}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] ?? Error guardando en BD local: {ex.Message}");
            }
        }

        private void OnNfcError(object? sender, string error)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] ? Error NFC: {error}");
                await Shell.Current.DisplayAlert("Error NFC", error, "OK");
            });
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        ~NfcReaderActiveViewModel()
        {
            _nfcService.TagRead -= OnTagRead;
            _nfcService.Error -= OnNfcError;
            StopTimer();
        }
    }
}
