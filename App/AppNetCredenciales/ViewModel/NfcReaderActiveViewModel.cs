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

        public NfcReaderActiveViewModel(LocalDBService db, NfcService nfcService, ApiService apiService)
        {
            _db = db;
            _nfcService = nfcService;
            _apiService = apiService;
            
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
        /// Procesa la credencial leída
        /// </summary>
        private async Task ProcessCredentialAsync(string idCriptografico)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("????????????????????????????????????????????");
                System.Diagnostics.Debug.WriteLine("??  ?? PROCESANDO CREDENCIAL            ??");
                System.Diagnostics.Debug.WriteLine("????????????????????????????????????????????");
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] ?? IdCriptográfico: {idCriptografico}");

                TotalReads++;

                // Buscar credencial en base de datos local
                var credenciales = await _db.GetCredencialesAsync();
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] ?? Credenciales en BD: {credenciales?.Count() ?? 0}");

                var credencial = credenciales?.FirstOrDefault(c => 
                    c.IdCriptografico != null && 
                    c.IdCriptografico.Equals(idCriptografico, StringComparison.OrdinalIgnoreCase));

                if (credencial == null)
                {
                    System.Diagnostics.Debug.WriteLine("[NfcReaderActive] ? Credencial NO encontrada");
                    FailedReads++;
                    
                    // Crear evento de acceso denegado (credencial no encontrada)
                    await CreateEventoAccesoAsync(
                        credencialId: null,
                        resultado: "Denegar",
                        motivo: $"Credencial no encontrada - ID: {idCriptografico.Substring(0, Math.Min(8, idCriptografico.Length))}..."
                    );
                    
                    await Shell.Current.DisplayAlert(
                        "? Acceso Denegado",
                        $"Credencial no válida o no registrada.\n\nID: {idCriptografico.Substring(0, Math.Min(8, idCriptografico.Length))}...",
                        "OK");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] ? Credencial encontrada:");
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive]    - ID API: {credencial.idApi}");
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive]    - Estado: {credencial.Estado}");
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive]    - Tipo: {credencial.Tipo}");
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive]    - Expiración: {credencial.FechaExpiracion}");

                // Validar estado
                bool esValida = credencial.Estado == models.CredencialEstado.Activada;
                string motivoRechazo = "";

                if (credencial.Estado != models.CredencialEstado.Activada)
                {
                    esValida = false;
                    motivoRechazo = $"Estado inválido: {credencial.Estado}";
                    System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] ?? Estado inválido: {credencial.Estado}");
                }
                else if (credencial.FechaExpiracion.HasValue && credencial.FechaExpiracion.Value < DateTime.Now)
                {
                    esValida = false;
                    motivoRechazo = $"Credencial expirada el {credencial.FechaExpiracion.Value:dd/MM/yyyy}";
                    System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] ?? Credencial expirada");
                }

                // Buscar usuario
                var usuarios = await _db.GetUsuariosAsync();
                var usuario = usuarios?.FirstOrDefault(u => u.idApi == credencial.usuarioIdApi);
                string nombreUsuario = usuario != null ? $"{usuario.Nombre} {usuario.Apellido}".Trim() : "Usuario desconocido";

                System.Diagnostics.Debug.WriteLine($"[NfcReaderActive] ?? Usuario: {nombreUsuario}");

                // Crear evento de acceso en la API
                string resultado = esValida ? "Permitir" : "Denegar";
                string motivo = esValida 
                    ? $"Acceso concedido - Credencial {credencial.Tipo} válida" 
                    : motivoRechazo;

                await CreateEventoAccesoAsync(
                    credencialId: credencial.idApi,
                    resultado: resultado,
                    motivo: motivo
                );

                // Resultado
                if (esValida)
                {
                    SuccessReads++;
                    
                    System.Diagnostics.Debug.WriteLine("????????????????????????????????????????????");
                    System.Diagnostics.Debug.WriteLine("?  ? ACCESO CONCEDIDO              ?");
                    System.Diagnostics.Debug.WriteLine("????????????????????????????????????????????");

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
                    
                    System.Diagnostics.Debug.WriteLine("????????????????????????????????????????????");
                    System.Diagnostics.Debug.WriteLine("?  ? ACCESO DENEGADO               ?");
                    System.Diagnostics.Debug.WriteLine("????????????????????????????????????????????");

                    await Shell.Current.DisplayAlert(
                        "? Acceso Denegado",
                        $"? Credencial NO válida\n\n" +
                        $"?? {nombreUsuario}\n" +
                        $"?? Motivo: {motivoRechazo}\n\n" +
                        $"?? {DateTime.Now:HH:mm:ss}",
                        "OK");
                }

                System.Diagnostics.Debug.WriteLine("????????????????????????????????????????????");
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
