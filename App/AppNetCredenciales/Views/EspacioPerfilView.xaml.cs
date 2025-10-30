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
        if (usuario != null && usuario.Credencial == null)
        {
           if (usuario.CredencialId != 0)
            {
                usuario.Credencial = await _db.GetCredencialByIdAsync(usuario.CredencialId);
            }
            else
            {
                try
                {
                    usuario.Credencial = await _db.GetLoggedUserCredential();
                }
                catch
                {
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