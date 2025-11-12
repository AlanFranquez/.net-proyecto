using AppNetCredenciales.Data;
using AppNetCredenciales.models;
using System.Diagnostics;

namespace AppNetCredenciales.Views;

[QueryProperty(nameof(EspacioId), "espacioId")]
public partial class EspacioPerfilView : ContentPage
{
    private int _espacioId;
    private readonly LocalDBService _db;

    public EspacioPerfilView() : this(MauiProgram.ServiceProvider?.GetService<LocalDBService>()
                                      ?? throw new InvalidOperationException("LocalDBService not registered"))
    { }

    // Primary ctor used when DI provides the service.
    public EspacioPerfilView(LocalDBService db)
    {
        this._db = db ?? throw new ArgumentNullException(nameof(db));
        InitializeComponent();
    }

    public int EspacioId
    {
        get => _espacioId;
        set
        {
            _espacioId = value;
            _ = CargarEspacioAsync(_espacioId);
        }
    }

    private async Task CargarEspacioAsync(int id)
    {
        try
        {
            if (id <= 0)
            {
                Debug.WriteLine($"[EspacioPerfil] Invalid id: {id}");
                return;
            }

            Debug.WriteLine($"[EspacioPerfil] Loading Espacio with id: {id}");

            var espacios = await this._db.GetEspaciosAsync();

            foreach (var e in espacios)
            {
                Debug.WriteLine($"[EspacioPerfil] Found Espacio: {e.EspacioId} - {e.Nombre}");
            }

            var espacio = espacios.FirstOrDefault(e => e.EspacioId == id);

            if (espacio == null)
            {
                Debug.WriteLine($"[EspacioPerfil] Espacio not found for id {id}");
                await DisplayAlert("Error", "Espacio no encontrado.", "OK");
                return;
            }

            // Set the BindingContext to the Espacio instance so XAML {Binding Nombre}, {Binding Lugar}, etc. work.
            BindingContext = espacio;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[EspacioPerfil] CargarEspacioAsync error: {ex}");
            await DisplayAlert("Error", "Ocurrió un error cargando el espacio.", "OK");
        }
    }

    private async void OnShowQRClicked(object sender, EventArgs e)
    {
        try
        {
            var usuario = await _db.GetLoggedUserAsync();

            if (usuario != null && usuario.Credencial == null && usuario.CredencialId > 0)
            {
                usuario.Credencial = await _db.GetCredencialByIdAsync(usuario.CredencialId);
            }

            var cred = usuario?.Credencial;

            Debug.WriteLine($"CREDENCIAL DEBUG => {usuario.CredencialId}");

            var getAllCredenciales = await _db.GetCredencialesAsync();

            foreach(var a in getAllCredenciales)
            {
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