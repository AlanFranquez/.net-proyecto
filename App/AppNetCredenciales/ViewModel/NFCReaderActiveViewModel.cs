using AppNetCredenciales.Services;
using Microsoft.Maui.Graphics;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AppNetCredenciales.ViewModel
{
    /// <summary>
    /// ViewModel para la pantalla del lector NFC activo
    /// </summary>
    public class NFCReaderActiveViewModel : INotifyPropertyChanged, IQueryAttributable
    {
        private readonly NFCService _nfcService;
        private readonly IEventosService _eventosService;
        
        private int _espacioId;
        private string _espacioNombre;
        private bool _isReading;
        private string _statusMessage;
        private string _statusIcon;
        private Color _backgroundColor;
        private Color _statusColor;
        
        // Información del último acceso
        private bool _hasLastResult;
        private string _lastNombreCompleto;
        private string _lastDocumento;
        private string _lastResultMessage;
        private string _lastResultIcon;
        private Color _lastResultColor;
        private string _lastAccessTime;

        // Propiedades públicas
        public string EspacioNombre
        {
            get => _espacioNombre;
            set { _espacioNombre = value; OnPropertyChanged(); }
        }

        public bool IsReading
        {
            get => _isReading;
            set { _isReading = value; OnPropertyChanged(); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public string StatusIcon
        {
            get => _statusIcon;
            set { _statusIcon = value; OnPropertyChanged(); }
        }

        public Color BackgroundColor
        {
            get => _backgroundColor;
            set { _backgroundColor = value; OnPropertyChanged(); }
        }

        public Color StatusColor
        {
            get => _statusColor;
            set { _statusColor = value; OnPropertyChanged(); }
        }

        public bool HasLastResult
        {
            get => _hasLastResult;
            set { _hasLastResult = value; OnPropertyChanged(); }
        }

        public string LastNombreCompleto
        {
            get => _lastNombreCompleto;
            set { _lastNombreCompleto = value; OnPropertyChanged(); }
        }

        public string LastDocumento
        {
            get => _lastDocumento;
            set { _lastDocumento = value; OnPropertyChanged(); }
        }

        public string LastResultMessage
        {
            get => _lastResultMessage;
            set { _lastResultMessage = value; OnPropertyChanged(); }
        }

        public string LastResultIcon
        {
            get => _lastResultIcon;
            set { _lastResultIcon = value; OnPropertyChanged(); }
        }

        public Color LastResultColor
        {
            get => _lastResultColor;
            set { _lastResultColor = value; OnPropertyChanged(); }
        }

        public string LastAccessTime
        {
            get => _lastAccessTime;
            set { _lastAccessTime = value; OnPropertyChanged(); }
        }

        public bool ModoDesarrollo => true; // Cambiar a false en producción

        public ICommand VerHistorialCommand { get; }
        public ICommand SimularLecturaCommand { get; }

        public NFCReaderActiveViewModel(NFCService nfcService, IEventosService eventosService)
        {
            _nfcService = nfcService;
            _eventosService = eventosService;

            VerHistorialCommand = new Command(OnVerHistorial);
            SimularLecturaCommand = new Command(OnSimularLectura);

            // Estado inicial
            SetEstadoEsperando();
        }

        /// <summary>
        /// Recibe los parámetros de navegación
        /// </summary>
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("espacioId"))
            {
                _espacioId = int.Parse(query["espacioId"].ToString());
                Debug.WriteLine($"[NFCReaderActiveVM] EspacioId recibido: {_espacioId}");
            }

            if (query.ContainsKey("espacioNombre"))
            {
                EspacioNombre = query["espacioNombre"].ToString();
            }
            else
            {
                EspacioNombre = $"Espacio #{_espacioId}";
            }
        }

        /// <summary>
        /// Inicia el lector NFC
        /// </summary>
        public async Task IniciarLectorAsync()
        {
            try
            {
                Debug.WriteLine("[NFCReaderActiveVM] Iniciando lector NFC...");
                
                // Verificar disponibilidad de NFC
                bool disponible = await _nfcService.IsNFCAvailableAsync();
                
                if (!disponible)
                {
                    await App.Current.MainPage.DisplayAlert(
                        "NFC No Disponible",
                        "El NFC no está disponible o no está habilitado en este dispositivo.",
                        "OK");
                    return;
                }

                // Suscribirse al evento de detección de tags
                // En este caso usaremos un loop para leer continuamente
                IsReading = true;
                SetEstadoEsperando();

                // Iniciar loop de lectura continua
                _ = Task.Run(async () => await LoopLecturaContinua());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[NFCReaderActiveVM] Error iniciando lector: {ex.Message}");
                await App.Current.MainPage.DisplayAlert("Error", $"Error al iniciar el lector: {ex.Message}", "OK");
            }
        }

        /// <summary>
        /// Loop de lectura continua de NFC
        /// </summary>
        private async Task LoopLecturaContinua()
        {
            while (IsReading)
            {
                try
                {
                    Debug.WriteLine("[NFCReaderActiveVM] Esperando tag NFC...");
                    var resultado = await _nfcService.StartReadingAsync();

                    if (resultado.Success && !string.IsNullOrEmpty(resultado.Data))
                    {
                        Debug.WriteLine($"[NFCReaderActiveVM] Tag detectado: {resultado.Data}");
                        await ProcesarTagNFC(resultado.Data);
                    }
                    else if (!string.IsNullOrEmpty(resultado.ErrorMessage))
                    {
                        Debug.WriteLine($"[NFCReaderActiveVM] Error en lectura: {resultado.ErrorMessage}");
                    }

                    // Pequeña pausa antes de volver a leer
                    await Task.Delay(1000);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[NFCReaderActiveVM] Error en loop de lectura: {ex.Message}");
                    await Task.Delay(2000);
                }
            }
        }

        /// <summary>
        /// Procesa un tag NFC detectado
        /// </summary>
        private async Task ProcesarTagNFC(string idCriptografico)
        {
            try
            {
                Debug.WriteLine($"???????????????????????????????????????????");
                Debug.WriteLine($"[NFCReaderActiveVM] ??? PROCESANDO TAG NFC");
                Debug.WriteLine($"[NFCReaderActiveVM] IdCriptografico: '{idCriptografico}'");
                Debug.WriteLine($"[NFCReaderActiveVM] Longitud: {idCriptografico?.Length ?? 0} caracteres");
                Debug.WriteLine($"[NFCReaderActiveVM] EspacioId: {_espacioId}");
                Debug.WriteLine($"???????????????????????????????????????????");
                
                SetEstadoValidando();

                // Validar con el servicio de eventos
                var resultado = await _eventosService.ValidarYRegistrarAcceso(idCriptografico, _espacioId);

                if (resultado.AccesoConcedido)
                {
                    Debug.WriteLine("[NFCReaderActiveVM] ? Acceso CONCEDIDO");
                    await MostrarAccesoConcedido(resultado);
                }
                else
                {
                    Debug.WriteLine($"[NFCReaderActiveVM] ? Acceso DENEGADO: {resultado.Motivo}");
                    await MostrarAccesoDenegado(resultado);
                }

                // Volver al estado de espera después de mostrar el resultado
                await Task.Delay(3000);
                SetEstadoEsperando();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[NFCReaderActiveVM] ? ERROR procesando tag: {ex.Message}");
                Debug.WriteLine($"[NFCReaderActiveVM] StackTrace: {ex.StackTrace}");
                await MostrarError(ex.Message);
                await Task.Delay(3000);
                SetEstadoEsperando();
            }
        }

        /// <summary>
        /// Muestra la pantalla de acceso concedido
        /// </summary>
        private async Task MostrarAccesoConcedido(EventoAccesoResult resultado)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                BackgroundColor = Color.FromArgb("#4CAF50"); // Verde
                StatusIcon = "?";
                StatusMessage = "ACCESO CONCEDIDO";
                StatusColor = Colors.White;

                HasLastResult = true;
                LastNombreCompleto = resultado.NombreCompleto ?? "N/A";
                LastDocumento = resultado.Documento ?? "N/A";
                LastResultMessage = "Acceso permitido";
                LastResultIcon = "?";
                LastResultColor = Color.FromArgb("#4CAF50");
                LastAccessTime = DateTime.Now.ToString("HH:mm:ss");
            });

            // Audio/vibración de éxito
            try
            {
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(200));
            }
            catch { }
        }

        /// <summary>
        /// Muestra la pantalla de acceso denegado
        /// </summary>
        private async Task MostrarAccesoDenegado(EventoAccesoResult resultado)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                BackgroundColor = Color.FromArgb("#F44336"); // Rojo
                StatusIcon = "?";
                StatusMessage = "ACCESO DENEGADO";
                StatusColor = Colors.White;

                HasLastResult = true;
                LastNombreCompleto = resultado.NombreCompleto ?? "Desconocido";
                LastDocumento = resultado.Documento ?? "N/A";
                LastResultMessage = resultado.Motivo;
                LastResultIcon = "?";
                LastResultColor = Color.FromArgb("#F44336");
                LastAccessTime = DateTime.Now.ToString("HH:mm:ss");
            });

            // Audio/vibración de error
            try
            {
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(500));
                await Task.Delay(200);
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(500));
            }
            catch { }
        }

        /// <summary>
        /// Muestra un error en la pantalla
        /// </summary>
        private async Task MostrarError(string mensaje)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                BackgroundColor = Color.FromArgb("#FF9800"); // Naranja
                StatusIcon = "??";
                StatusMessage = "ERROR";
                StatusColor = Colors.White;

                HasLastResult = true;
                LastNombreCompleto = "Error del sistema";
                LastDocumento = "-";
                LastResultMessage = mensaje;
                LastResultIcon = "??";
                LastResultColor = Color.FromArgb("#FF9800");
                LastAccessTime = DateTime.Now.ToString("HH:mm:ss");
            });

            try
            {
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(300));
            }
            catch { }
        }

        /// <summary>
        /// Establece el estado de espera
        /// </summary>
        private void SetEstadoEsperando()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                BackgroundColor = Color.FromArgb("#FFC107"); // Amarillo
                StatusIcon = "??";
                StatusMessage = "Esperando dispositivo...";
                StatusColor = Color.FromArgb("#333333");
            });
        }

        /// <summary>
        /// Establece el estado de validación
        /// </summary>
        private void SetEstadoValidando()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                BackgroundColor = Color.FromArgb("#2196F3"); // Azul
                StatusIcon = "??";
                StatusMessage = "Validando...";
                StatusColor = Colors.White;
            });
        }

        /// <summary>
        /// Detiene el lector NFC
        /// </summary>
        public void DetenerLector()
        {
            IsReading = false;
            _nfcService.StopReading();
            Debug.WriteLine("[NFCReaderActiveVM] Lector detenido");
        }

        /// <summary>
        /// Comando para ver el historial
        /// </summary>
        private async void OnVerHistorial()
        {
            try
            {
                // Navegar a la vista de historial (a implementar)
                await Shell.Current.GoToAsync($"nfc-historial?espacioId={_espacioId}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[NFCReaderActiveVM] Error navegando a historial: {ex.Message}");
            }
        }

        /// <summary>
        /// Comando para simular una lectura (solo desarrollo)
        /// </summary>
        private async void OnSimularLectura()
        {
            try
            {
                // Simular con un IdCriptográfico de prueba
                string idCriptoTest = "ABC123XYZ"; // Usar el mismo que está en el usuario de prueba
                Debug.WriteLine($"[NFCReaderActiveVM] Simulando lectura con: {idCriptoTest}");
                await ProcesarTagNFC(idCriptoTest);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[NFCReaderActiveVM] Error simulando lectura: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
