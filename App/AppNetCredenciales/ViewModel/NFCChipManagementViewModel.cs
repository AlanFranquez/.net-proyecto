using AppNetCredenciales.Data;
using AppNetCredenciales.models;
using AppNetCredenciales.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AppNetCredenciales.ViewModel
{
    /// <summary>
    /// ViewModel para la gestión de chips NFC (escritura de credenciales)
    /// </summary>
    public class NFCChipManagementViewModel : INotifyPropertyChanged
    {
        private readonly LocalDBService _db;
        private readonly NFCService _nfcService;
        private ObservableCollection<CredencialItemViewModel> _credenciales;
        private bool _isLoading;
        private bool _isWriting;
        private string _statusMessage;

        public ObservableCollection<CredencialItemViewModel> Credenciales
        {
            get => _credenciales;
            set { _credenciales = value; OnPropertyChanged(); }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public bool IsWriting
        {
            get => _isWriting;
            set { _isWriting = value; OnPropertyChanged(); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public ICommand RefreshCommand { get; }

        public NFCChipManagementViewModel(LocalDBService db, NFCService nfcService)
        {
            _db = db;
            _nfcService = nfcService;
            _credenciales = new ObservableCollection<CredencialItemViewModel>();
            _statusMessage = "Seleccione una credencial para escribir en un chip NFC";

            RefreshCommand = new Command(async () => await LoadCredencialesAsync());
        }

        /// <summary>
        /// Carga la lista de credenciales desde la base de datos
        /// </summary>
        public async Task LoadCredencialesAsync()
        {
            try
            {
                IsLoading = true;
                Credenciales.Clear();

                Debug.WriteLine("[NFCChipManagementVM] Cargando credenciales...");
                var credenciales = await _db.GetCredencialesAsync();

                if (credenciales == null || credenciales.Count == 0)
                {
                    Debug.WriteLine("[NFCChipManagementVM] No hay credenciales disponibles");
                    StatusMessage = "No hay credenciales disponibles. Sincronice con el servidor.";
                    return;
                }

                Debug.WriteLine($"[NFCChipManagementVM] {credenciales.Count} credenciales encontradas");

                // Ordenar: Activadas primero, luego por fecha de emisión
                var credencialesOrdenadas = credenciales
                    .OrderByDescending(c => c.Estado == CredencialEstado.Activada)
                    .ThenByDescending(c => c.FechaEmision);

                foreach (var credencial in credencialesOrdenadas)
                {
                    // Obtener información del usuario
                    var usuario = await _db.GetUsuarioByIdApiAsync(credencial.usuarioIdApi);
                    
                    var item = new CredencialItemViewModel(credencial, usuario, this);
                    Credenciales.Add(item);
                }

                StatusMessage = $"{Credenciales.Count} credenciales disponibles para escribir en chips NFC";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[NFCChipManagementVM] Error cargando credenciales: {ex.Message}");
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Escribe una credencial en un chip NFC
        /// </summary>
        public async Task EscribirEnChipAsync(CredencialItemViewModel credencialItem)
        {
            if (IsWriting)
            {
                await App.Current.MainPage.DisplayAlert(
                    "Operación en Progreso",
                    "Ya hay una operación de escritura en progreso. Por favor espere.",
                    "OK");
                return;
            }

            try
            {
                IsWriting = true;
                StatusMessage = $"Preparando escritura para {credencialItem.NombreUsuario}...";

                // Confirmar acción
                bool confirmar = await App.Current.MainPage.DisplayAlert(
                    "Escribir en Chip NFC",
                    $"¿Desea escribir la credencial en un chip NFC?\n\n" +
                    $"Usuario: {credencialItem.NombreUsuario}\n" +
                    $"Documento: {credencialItem.DocumentoUsuario}\n" +
                    $"ID Criptográfico: {credencialItem.IdCriptografico}\n" +
                    $"Estado: {credencialItem.EstadoTexto}\n\n" +
                    $"Acerque el chip NFC cuando se indique.",
                    "Escribir",
                    "Cancelar");

                if (!confirmar)
                {
                    StatusMessage = "Operación cancelada";
                    return;
                }

                // Validar que la credencial tenga IdCriptografico
                if (string.IsNullOrWhiteSpace(credencialItem.IdCriptografico))
                {
                    await App.Current.MainPage.DisplayAlert(
                        "Error",
                        "Esta credencial no tiene un ID Criptográfico válido.",
                        "OK");
                    StatusMessage = "Error: Credencial sin ID Criptográfico";
                    return;
                }

                StatusMessage = $"? Acerque el chip NFC al dispositivo...";
                Debug.WriteLine($"[NFCChipManagementVM] Iniciando escritura: {credencialItem.IdCriptografico}");

                // Escribir en el chip
                var resultado = await _nfcService.WriteTagAsync(credencialItem.IdCriptografico);

                if (resultado.Success)
                {
                    // ? Éxito
                    StatusMessage = "? Credencial escrita correctamente";
                    Debug.WriteLine($"[NFCChipManagementVM] ? Escritura exitosa");

                    await App.Current.MainPage.DisplayAlert(
                        "? Éxito",
                        resultado.Message ?? "La credencial se escribió correctamente en el chip NFC.",
                        "OK");

                    // Actualizar el estado visual del item
                    credencialItem.MostrarExito();
                }
                else
                {
                    // ? Error
                    StatusMessage = $"? Error: {resultado.ErrorMessage}";
                    Debug.WriteLine($"[NFCChipManagementVM] ? Error en escritura: {resultado.ErrorMessage}");

                    await App.Current.MainPage.DisplayAlert(
                        "? Error",
                        resultado.ErrorMessage ?? "No se pudo escribir en el chip NFC.",
                        "OK");

                    credencialItem.MostrarError();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[NFCChipManagementVM] ? Exception: {ex.Message}");
                StatusMessage = $"? Error: {ex.Message}";

                await App.Current.MainPage.DisplayAlert(
                    "Error",
                    $"Error inesperado: {ex.Message}",
                    "OK");
            }
            finally
            {
                IsWriting = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    /// <summary>
    /// ViewModel para un item de credencial en la lista
    /// </summary>
    public class CredencialItemViewModel : INotifyPropertyChanged
    {
        private readonly NFCChipManagementViewModel _parentViewModel;
        private Microsoft.Maui.Graphics.Color _backgroundColor;
        private string _statusIcon;

        public Credencial Credencial { get; }
        public Usuario Usuario { get; }

        public string IdCriptografico => Credencial.IdCriptografico ?? "Sin ID";
        public string NombreUsuario => Usuario != null ? $"{Usuario.Nombre} {Usuario.Apellido}" : "Usuario desconocido";
        public string DocumentoUsuario => Usuario?.Documento ?? "N/A";
        public string EstadoTexto => Credencial.Estado.ToString();
        public string TipoTexto => Credencial.Tipo.ToString();
        public string FechaEmisionTexto => Credencial.FechaEmision.ToString("dd/MM/yyyy");
        public string FechaExpiracionTexto => Credencial.FechaExpiracion?.ToString("dd/MM/yyyy") ?? "Sin expiración";

        public Microsoft.Maui.Graphics.Color BackgroundColor
        {
            get => _backgroundColor;
            set { _backgroundColor = value; OnPropertyChanged(); }
        }

        public string StatusIcon
        {
            get => _statusIcon;
            set { _statusIcon = value; OnPropertyChanged(); }
        }

        public Microsoft.Maui.Graphics.Color EstadoColor
        {
            get
            {
                return Credencial.Estado switch
                {
                    CredencialEstado.Activada => Microsoft.Maui.Graphics.Color.FromArgb("#4CAF50"),
                    CredencialEstado.Emitida => Microsoft.Maui.Graphics.Color.FromArgb("#FFA500"),
                    CredencialEstado.Suspendida => Microsoft.Maui.Graphics.Color.FromArgb("#FF9800"),
                    CredencialEstado.Expirada => Microsoft.Maui.Graphics.Color.FromArgb("#F44336"),
                    _ => Microsoft.Maui.Graphics.Colors.Gray
                };
            }
        }

        public string IconoEstado
        {
            get
            {
                return Credencial.Estado switch
                {
                    CredencialEstado.Activada => "?",
                    CredencialEstado.Emitida => "??",
                    CredencialEstado.Suspendida => "??",
                    CredencialEstado.Expirada => "?",
                    _ => "?"
                };
            }
        }

        public ICommand EscribirCommand { get; }

        public CredencialItemViewModel(Credencial credencial, Usuario usuario, NFCChipManagementViewModel parentViewModel)
        {
            Credencial = credencial;
            Usuario = usuario;
            _parentViewModel = parentViewModel;
            _backgroundColor = Microsoft.Maui.Graphics.Colors.White;
            _statusIcon = "";

            EscribirCommand = new Command(async () => await _parentViewModel.EscribirEnChipAsync(this));
        }

        public void MostrarExito()
        {
            BackgroundColor = Microsoft.Maui.Graphics.Color.FromArgb("#E8F5E9");
            StatusIcon = "?";
            
            // Volver al estado normal después de 3 segundos
            Task.Delay(3000).ContinueWith(_ =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    BackgroundColor = Microsoft.Maui.Graphics.Colors.White;
                    StatusIcon = "";
                });
            });
        }

        public void MostrarError()
        {
            BackgroundColor = Microsoft.Maui.Graphics.Color.FromArgb("#FFEBEE");
            StatusIcon = "?";
            
            // Volver al estado normal después de 3 segundos
            Task.Delay(3000).ContinueWith(_ =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    BackgroundColor = Microsoft.Maui.Graphics.Colors.White;
                    StatusIcon = "";
                });
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
