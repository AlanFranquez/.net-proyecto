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
    private readonly NFCService _nfcService;

    private Credencial _credencial;
    public Credencial Credencial
    {
        get => _credencial;
        set { _credencial = value; OnPropertyChanged(); }
    }

    private bool _isNFCActive;
    public bool IsNFCActive
    {
        get => _isNFCActive;
        set { _isNFCActive = value; OnPropertyChanged(); }
    }

    private bool _isWritingNFC;
    public bool IsWritingNFC
    {
        get => _isWritingNFC;
        set { _isWritingNFC = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanWriteNFC)); }
    }

    private string _nfcStatusMessage;
    public string NFCStatusMessage
    {
        get => _nfcStatusMessage;
        set { _nfcStatusMessage = value; OnPropertyChanged(); }
    }

    public bool CanWriteNFC => !IsWritingNFC && Credencial != null && !string.IsNullOrEmpty(Credencial.IdCriptografico);

    public ICommand EscribirEnChipNFCCommand { get; }

    public CredencialViewModel(AuthService auth, LocalDBService db, NFCService nfcService)
    {
        _auth = auth;
        _db = db;
        _nfcService = nfcService;
        _nfcStatusMessage = "";

        EscribirEnChipNFCCommand = new Command(async () => await EscribirEnChipNFCAsync(), () => CanWriteNFC);
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
    }

    /// <summary>
    /// Escribe la credencial del usuario en su chip NFC personal
    /// </summary>
    public async Task EscribirEnChipNFCAsync()
    {
        if (Credencial == null || string.IsNullOrWhiteSpace(Credencial.IdCriptografico))
        {
            await App.Current.MainPage.DisplayAlert(
                "Error",
                "No hay credencial válida para escribir en el chip NFC.",
                "OK");
            return;
        }

        try
        {
            IsWritingNFC = true;
            NFCStatusMessage = "Preparando chip NFC...";

            Debug.WriteLine($"[CredencialVM] 📝 Iniciando escritura NFC para usuario");
            Debug.WriteLine($"[CredencialVM] IdCriptografico: {Credencial.IdCriptografico}");

            // Verificar disponibilidad de NFC
            bool nfcDisponible = await _nfcService.IsNFCAvailableAsync();
            if (!nfcDisponible)
            {
                await App.Current.MainPage.DisplayAlert(
                    "NFC No Disponible",
                    "El NFC no está disponible o no está habilitado en este dispositivo.\n\n" +
                    "Por favor, habilite el NFC en la configuración de su dispositivo.",
                    "OK");
                NFCStatusMessage = "NFC no disponible";
                return;
            }

            // Confirmar acción con el usuario
            bool confirmar = await App.Current.MainPage.DisplayAlert(
                "📱 Escribir Credencial en Chip NFC",
                $"¿Desea escribir su credencial en un chip NFC?\n\n" +
                $"✅ Tipo: {Credencial.Tipo}\n" +
                $"✅ Estado: {Credencial.Estado}\n" +
                $"✅ Válida hasta: {Credencial.FechaExpiracion?.ToString("dd/MM/yyyy") ?? "Sin fecha"}\n\n" +
                $"Acerque su chip NFC (tarjeta, pulsera, etc.) al dispositivo cuando se indique.\n\n" +
                $"⚠️ Esta operación solo debe realizarse UNA VEZ por chip.",
                "Escribir",
                "Cancelar");

            if (!confirmar)
            {
                NFCStatusMessage = "Operación cancelada";
                Debug.WriteLine("[CredencialVM] Usuario canceló la operación");
                return;
            }

            NFCStatusMessage = "⏳ Acerque su chip NFC al dispositivo...";
            Debug.WriteLine("[CredencialVM] Esperando chip NFC...");

            // Escribir en el chip
            var resultado = await _nfcService.WriteTagAsync(Credencial.IdCriptografico);

            if (resultado.Success)
            {
                // ✅ ÉXITO
                NFCStatusMessage = "✅ Credencial escrita correctamente";
                Debug.WriteLine("[CredencialVM] ✅ Escritura NFC exitosa");

                await App.Current.MainPage.DisplayAlert(
                    "✅ ¡Éxito!",
                    $"Su credencial se escribió correctamente en el chip NFC.\n\n" +
                    $"✅ Su chip ya está listo para usarse en cualquier punto de acceso.\n\n" +
                    $"📱 Simplemente acerque el chip a los lectores NFC para acceder a los espacios autorizados.",
                    "Entendido");

                // Opcional: Marcar que el usuario ya configuró su chip
                // await MarcarChipConfigurado();
            }
            else
            {
                // ❌ ERROR
                NFCStatusMessage = $"❌ Error: {resultado.ErrorMessage}";
                Debug.WriteLine($"[CredencialVM] ❌ Error en escritura: {resultado.ErrorMessage}");

                await App.Current.MainPage.DisplayAlert(
                    "❌ Error",
                    $"No se pudo escribir en el chip NFC.\n\n" +
                    $"Motivo: {resultado.ErrorMessage}\n\n" +
                    $"Por favor, intente nuevamente o contacte al administrador si el problema persiste.",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CredencialVM] ❌ Exception: {ex.Message}");
            NFCStatusMessage = $"❌ Error inesperado";

            await App.Current.MainPage.DisplayAlert(
                "Error",
                $"Ocurrió un error inesperado: {ex.Message}",
                "OK");
        }
        finally
        {
            IsWritingNFC = false;
            
            // Limpiar mensaje después de unos segundos
            await Task.Delay(5000);
            if (!IsWritingNFC)
            {
                NFCStatusMessage = "";
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
