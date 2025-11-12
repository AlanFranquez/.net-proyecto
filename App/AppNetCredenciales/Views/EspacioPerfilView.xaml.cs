using AppNetCredenciales.Data;
using AppNetCredenciales.models;
using AppNetCredenciales.Services;
using System.Diagnostics;

namespace AppNetCredenciales.Views;

[QueryProperty(nameof(EspacioId), "espacioId")]
public partial class EspacioPerfilView : ContentPage
{
    private string _espacioId;
    private readonly LocalDBService _db;
    private readonly ConnectivityService connectiviyService = new ConnectivityService();

    public EspacioPerfilView() : this(MauiProgram.ServiceProvider?.GetService<LocalDBService>()
                                      ?? throw new InvalidOperationException("LocalDBService not registered"))
    { }

    // Primary ctor used when DI provides the service.
    public EspacioPerfilView(LocalDBService db)
    {
        this._db = db ?? throw new ArgumentNullException(nameof(db));
        InitializeComponent();
    }

    public string EspacioId
    {
        get => _espacioId;
        set
        {
            _espacioId = value;
            _ = CargarEspacioAsync(_espacioId);
        }
    }

    private async Task CargarEspacioAsync(string id)
    {
        try
        {
            if (id == null)
            {
                Debug.WriteLine($"[EspacioPerfil] Invalid id: {id}");
                return;
            }

            Debug.WriteLine($"[EspacioPerfil] Loading Espacio with id: {id}");

            var espacios = await this._db.GetEspaciosAsync();

            Debug.WriteLine($"[EspacioPerfil] Total espacios found: {espacios?.Count ?? 0}");

            foreach (var e in espacios)
            {
                Debug.WriteLine($"[EspacioPerfil] Found Espacio: idApi={e.idApi}, Nombre='{e.Nombre}', Tipo={e.Tipo}, Activo={e.Activo}, EspacioId={e.EspacioId}");
            }

            var espacio = espacios.FirstOrDefault(e => e.idApi == id);

            if (espacio == null)
            {
                Debug.WriteLine($"[EspacioPerfil] Espacio not found for id {id}");
                await DisplayAlert("Error", $"Espacio con ID {id} no encontrado.", "OK");
                return;
            }

            Debug.WriteLine($"[EspacioPerfil] Found matching espacio:");
            Debug.WriteLine($"[EspacioPerfil] - idApi: {espacio.idApi}");
            Debug.WriteLine($"[EspacioPerfil] - Nombre: '{espacio.Nombre}'");
            Debug.WriteLine($"[EspacioPerfil] - Tipo: {espacio.Tipo}");
            Debug.WriteLine($"[EspacioPerfil] - Activo: {espacio.Activo}");
            Debug.WriteLine($"[EspacioPerfil] - EspacioId: {espacio.EspacioId}");
            Debug.WriteLine($"[EspacioPerfil] - Descripcion: '{espacio.Descripcion}'");

            // Check if the space data is valid
            if (string.IsNullOrEmpty(espacio.Nombre) || espacio.Nombre == "string")
            {
                Debug.WriteLine($"[EspacioPerfil] WARNING: Espacio has invalid or corrupted name data!");

                // Try to force refresh from API if connected
                if (connectiviyService.IsConnected)
                {
                    Debug.WriteLine($"[EspacioPerfil] Attempting to refresh espacios from API...");
                    try
                    {
                        var refreshedEspacios = await this._db.SincronizarEspaciosFromBack();
                        Debug.WriteLine($"[EspacioPerfil] Refreshed {refreshedEspacios?.Count ?? 0} espacios from API");

                        // Try to find the space again
                        espacio = refreshedEspacios.FirstOrDefault(e => e.idApi == id);
                        if (espacio != null)
                        {
                            Debug.WriteLine($"[EspacioPerfil] After refresh - Nombre: '{espacio.Nombre}'");
                        }
                    }
                    catch (Exception refreshEx)
                    {
                        Debug.WriteLine($"[EspacioPerfil] Failed to refresh from API: {refreshEx.Message}");
                    }
                }
            }

            // Set the BindingContext to the Espacio instance so XAML bindings work
            BindingContext = espacio;

            Debug.WriteLine($"[EspacioPerfil] BindingContext set successfully");

            // Force property change notification
            OnPropertyChanged(nameof(BindingContext));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[EspacioPerfil] CargarEspacioAsync error: {ex}");
            await DisplayAlert("Error", $"Ocurrió un error cargando el espacio: {ex.Message}", "OK");
        }
    }

    private async void OnShowQRClicked(object sender, EventArgs e)
    {
        try
        {
            var usuario = await _db.GetLoggedUserAsync();


            Credencial cred = null;
            var getAllCredenciales = await _db.GetCredencialesAsync();

            foreach(var a in getAllCredenciales)
            {
                if(a.usuarioIdApi == usuario.idApi)
                {
                    cred = a;
                }
                Debug.WriteLine($"DATOS DE CREDENCIALES -> {a.CredencialId} - {a.idApi} - {a.usuarioIdApi}");
            }
            var espacio = BindingContext as Espacio;

            if (cred != null && espacio != null && !string.IsNullOrEmpty(cred.IdCriptografico))
            {
                string qrData = $"{cred.IdCriptografico}|{espacio.EspacioId}";
                var modal = new QRModalPage(qrData);
                await Navigation.PushModalAsync(modal);
            }
            else
            {
                await DisplayAlert("Error", "Credencial o Espacio no disponible.", "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[EspacioPerfil] OnShowQRClicked error: {ex}");
            await DisplayAlert("Error", "Ocurrió un error.", "OK");
        }
    }
}