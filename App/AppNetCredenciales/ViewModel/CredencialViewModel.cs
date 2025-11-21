using AppNetCredenciales.Data;
using AppNetCredenciales.models;
using AppNetCredenciales.services;
using AppNetCredenciales.Services;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AppNetCredenciales.ViewModel;

public class CredencialViewModel : INotifyPropertyChanged
{
    private readonly LocalDBService _db;
    private readonly AuthService _auth;
    private readonly NfcService _nfcService;
    private bool _isNfcActive;

    private Credencial _credencial;
    public Credencial Credencial
    {
        get => _credencial;
        set { _credencial = value; OnPropertyChanged(); }
    }

    public bool IsNfcActive
    {
        get => _isNfcActive;
        set
        {
            _isNfcActive = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(NfcButtonText));
        }
    }

    public string NfcButtonText => IsNfcActive ? "📡 NFC Activo - Acerca al lector" : "🔒 Activar NFC";

    public ICommand ToggleNfcCommand { get; }
    public ICommand WriteToTagCommand { get; }

    public CredencialViewModel(AuthService auth, LocalDBService db, NfcService nfcService)
    {
        _auth = auth;
        _db = db;
        _nfcService = nfcService;

        ToggleNfcCommand = new Command(async () => await ToggleNfcAsync());
        WriteToTagCommand = new Command(async () => await WriteToTagAsync());

        // Suscribirse a eventos NFC
        _nfcService.TagWritten += OnTagWritten;
        _nfcService.Error += OnNfcError;
    }

    public async Task LoadCredencialAsync()
    {
        // 🔹 Obtener usuario logueado
        var usuario = await _db.GetLoggedUserAsync();

        var credenciales = await _db.GetCredencialesAsync();
        Debug.WriteLine("Buscando credenciales para el usuario: " + usuario.idApi);
        
        foreach (var a in credenciales)
        {
            Debug.WriteLine($"CREDENCIAL ID => {a.usuarioIdApi} !-! ID DEL USUARIO: {usuario.idApi}");
            if(a.usuarioIdApi == usuario.idApi)
            {
                Credencial = a;
                return;
            }
        }

        if(Credencial == null)
        {
            Credencial = new Credencial
            {
                Tipo = CredencialTipo.Campus,
                Estado = CredencialEstado.Emitida,
                IdCriptografico = "ABC123XYZ",
                FechaEmision = DateTime.Now,
                FechaExpiracion = DateTime.Now.AddYears(1)
            };
        }

        // Verificar disponibilidad NFC
        if (!_nfcService.IsAvailable)
        {
            Debug.WriteLine("[CredencialViewModel] ⚠️ NFC no disponible en este dispositivo");
        }
    }

    private async Task ToggleNfcAsync()
    {
        try
        {
            if (Credencial == null)
            {
                Debug.WriteLine("[CredencialViewModel] No hay credencial cargada");
                await Application.Current.MainPage.DisplayAlert(
                    "Error", 
                    "No se pudo cargar la credencial", 
                    "OK");
                return;
            }

            if (string.IsNullOrEmpty(Credencial.IdCriptografico))
            {
                Debug.WriteLine("[CredencialViewModel] IdCriptografico vacío");
                await Application.Current.MainPage.DisplayAlert(
                    "Error", 
                    "Credencial sin IdCriptografico", 
                    "OK");
                return;
            }

            if (IsNfcActive)
            {
                // Desactivar NFC (detener lectura y publicación)
                _nfcService.StopAll();
                IsNfcActive = false;
                Debug.WriteLine("[CredencialViewModel] NFC desactivado");
                
                await Application.Current.MainPage.DisplayAlert(
                    "🛑 NFC Desactivado", 
                    "El modo NFC se ha desactivado completamente", 
                    "OK");
            }
            else
            {
                // Verificar disponibilidad de NFC
                if (!_nfcService.IsAvailable)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "NFC No Disponible",
                        "Este dispositivo no tiene NFC o está deshabilitado",
                        "OK");
                    return;
                }

                if (!_nfcService.IsEnabled)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "NFC Deshabilitado",
                        "Por favor, habilita NFC en la configuración de Android",
                        "OK");
                    return;
                }

                // Activar NFC escribiendo la credencial
                Debug.WriteLine($"[CredencialViewModel] ═══════════════════════════════════════");
                Debug.WriteLine($"[CredencialViewModel] Activando NFC con IdCriptografico:");
                Debug.WriteLine($"[CredencialViewModel] {Credencial.IdCriptografico}");
                Debug.WriteLine($"[CredencialViewModel] ═══════════════════════════════════════");

                bool success = await _nfcService.WriteCredentialAsync(Credencial.IdCriptografico);
                
                if (success)
                {
                    IsNfcActive = true;
                    
                    await Application.Current.MainPage.DisplayAlert(
                        "✅ NFC Activado",
                        $"Credencial lista para ser leída.\n\n" +
                        $"🔑 ID: {Credencial.IdCriptografico.Substring(0, 8)}...\n\n" +
                        $"Acerca tu dispositivo al lector NFC del funcionario.",
                        "Entendido");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        "No se pudo activar NFC",
                        "OK");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CredencialViewModel] ❌ Error en ToggleNfc: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert(
                "Error",
                $"Error: {ex.Message}",
                "OK");
        }
    }

    private async Task WriteToTagAsync()
    {
        try
        {
            if (Credencial == null || string.IsNullOrEmpty(Credencial.IdCriptografico))
            {
                Debug.WriteLine("[CredencialViewModel] No hay credencial válida para escribir");
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "No se pudo cargar la credencial",
                    "OK");
                return;
            }

            // Verificar disponibilidad de NFC
            if (!_nfcService.IsAvailable)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "NFC No Disponible",
                    "Este dispositivo no tiene NFC o está deshabilitado",
                    "OK");
                return;
            }

            if (!_nfcService.IsEnabled)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "NFC Deshabilitado",
                    "Por favor, habilita NFC en la configuración de Android",
                    "OK");
                return;
            }

            // Confirmar acción
            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "✍️ Escribir en Tag NFC",
                $"Vas a escribir tu credencial en un tag NFC físico.\n\n" +
                $"🔑 ID: {Credencial.IdCriptografico.Substring(0, Math.Min(8, Credencial.IdCriptografico.Length))}...\n\n" +
                $"¿Tienes un tag NFC vacío listo?",
                "Sí, continuar",
                "Cancelar");

            if (!confirm) return;

            Debug.WriteLine($"[CredencialViewModel] ═══════════════════════════════════════");
            Debug.WriteLine($"[CredencialViewModel] Iniciando escritura NDEF en tag físico");
            Debug.WriteLine($"[CredencialViewModel] IdCriptografico: {Credencial.IdCriptografico}");
            Debug.WriteLine($"[CredencialViewModel] ═══════════════════════════════════════");

            bool success = await _nfcService.WriteToPhysicalTagAsync(Credencial.IdCriptografico);

            if (success)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "✅ Modo Escritura Activado",
                    "Acerca ahora un tag NFC vacío o reescribible.\n\n" +
                    "Mantén el tag quieto hasta ver la confirmación.",
                    "Entendido");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "No se pudo activar el modo de escritura NFC",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CredencialViewModel] ❌ Error en WriteToTag: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert(
                "Error",
                $"Error: {ex.Message}",
                "OK");
        }
    }

    private void OnTagWritten(object? sender, string credentialId)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            Debug.WriteLine($"[CredencialViewModel] ✅ Tag escrito exitosamente: {credentialId}");
            
            await Application.Current.MainPage.DisplayAlert(
                "✅ Tag Escrito Exitosamente",
                $"Tu credencial ha sido escrita en el tag NFC.\n\n" +
                $"🔑 ID: {credentialId.Substring(0, Math.Min(8, credentialId.Length))}...\n\n" +
                $"Ahora el funcionario puede leer este tag.",
                "Perfecto");
        });
    }

    private void OnNfcError(object? sender, string error)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            Debug.WriteLine($"[CredencialViewModel] ❌ Error NFC: {error}");
            
            await Application.Current.MainPage.DisplayAlert(
                "Error NFC",
                error,
                "OK");
            
            IsNfcActive = false;
        });
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    ~CredencialViewModel()
    {
        _nfcService.TagWritten -= OnTagWritten;
        _nfcService.Error -= OnNfcError;
    }
}
