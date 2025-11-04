using AppNetCredenciales.Data;
using System.Diagnostics;

namespace AppNetCredenciales.Views;

[QueryProperty(nameof(EspacioId), "id")]
public partial class EspacioPerfilView : ContentPage
{
    private int _espacioId;
    private readonly LocalDBService _db;

    // Parameterless ctor so Shell can construct the page on platforms
    // where DI isn't used for route activation.
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

            var espacio = await this._db.GetEspacioByIdAsync(id);
            if (espacio == null)
            {
                Debug.WriteLine($"[EspacioPerfil] Espacio not found for id {id}");
                await DisplayAlert("Error", "Espacio no encontrado.", "OK");
                return;
            }

            BindingContext = new { Espacio = espacio };
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

            var espacio = (BindingContext as dynamic)?.Espacio;

            if (usuario != null && usuario.Credencial != null && espacio != null && !string.IsNullOrEmpty(usuario.Credencial.IdCriptografico))
            {
                string qrData = $"{usuario.Credencial.IdCriptografico}|{espacio.EspacioId}";
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