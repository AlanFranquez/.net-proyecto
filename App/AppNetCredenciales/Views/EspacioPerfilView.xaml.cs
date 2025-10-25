using AppNetCredenciales.Data;
using System.Threading.Tasks.Dataflow;

namespace AppNetCredenciales.Views;

[QueryProperty(nameof(EspacioId), "id")]
public partial class EspacioPerfilView : ContentPage
{
    private int _espacioId;
    public int EspacioId
    {
        get => _espacioId;
        set
        {
            _espacioId = value;
            _ = CargarEspacioAsync(_espacioId);
        }
    }

    private readonly LocalDBService _db;


    public EspacioPerfilView(LocalDBService db)
    {
        this._db = db;
        InitializeComponent();
    }

    private async Task CargarEspacioAsync(int id)
    {
        var espacio = await this._db.GetEspacioByIdAsync(id);
        BindingContext = new { Espacio = espacio };
    }

    private async void OnShowQRClicked(object sender, EventArgs e)
    {
        var usuario = await _db.GetLoggedUserAsync();
        // Ensure usuario.Credencial is loaded (Credencial is [Ignore] in the model)
        if (usuario != null && usuario.Credencial == null)
        {
            // Prefer loading by id; fallback to any helper your service provides
            if (usuario.CredencialId != 0)
            {
                usuario.Credencial = await _db.GetCredencialByIdAsync(usuario.CredencialId);
            }
            else
            {
                // If your service exposes a helper to get the logged user's credential:
                try
                {
                    usuario.Credencial = await _db.GetLoggedUserCredential();
                }
                catch
                {
                    // leave null, next check will guard
                }
            }
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
            await DisplayAlert("Error", "Credencial or Espacio not available.", "OK");
        }
    }

}