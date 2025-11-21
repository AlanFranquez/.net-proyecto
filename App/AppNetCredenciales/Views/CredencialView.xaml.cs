using AppNetCredenciales.Data;
using AppNetCredenciales.services;
using AppNetCredenciales.Services;
using AppNetCredenciales.ViewModel;

namespace AppNetCredenciales.Views;

public partial class CredencialView : ContentPage
{
    private readonly AuthService _auth;
    private readonly CredencialViewModel _vm;
    private readonly NFCService _nfcService;

    public CredencialView(AuthService auth, LocalDBService db, NFCService nfcService)
    {
        InitializeComponent();

        _auth = auth;
        _nfcService = nfcService;

        // Inyectar NFCService en el ViewModel
        _vm = new CredencialViewModel(auth, db, nfcService);
        BindingContext = _vm;

        this.Loaded += CredencialView_Loaded;
    }

    private async void CredencialView_Loaded(object sender, EventArgs e)
    {
        var usuarioLogueado = await _auth.GetUserLogged();
        if (usuarioLogueado == null)
        {
            await Shell.Current.GoToAsync("//login");
            return;
        }

        await _vm.LoadCredencialAsync();
    }

    private async void OnActivarNFCClicked(object sender, EventArgs e)
    {
        try
        {
            if (_vm.Credencial == null || string.IsNullOrEmpty(_vm.Credencial.IdCriptografico))
            {
                await DisplayAlert("Error", "No se pudo obtener la credencial", "OK");
                return;
            }

            // Verificar si NFC está disponible
            bool nfcDisponible = await _nfcService.IsNFCAvailableAsync();
            
            if (!nfcDisponible)
            {
                await DisplayAlert(
                    "NFC No Disponible",
                    "El NFC no está disponible o no está habilitado en este dispositivo. " +
                    "Por favor, habilita el NFC en la configuración de tu dispositivo.",
                    "OK");
                return;
            }

            _vm.IsNFCActive = true;

            // Emitir la credencial vía NFC
            var resultado = await _nfcService.WriteTagAsync(_vm.Credencial.IdCriptografico);

            if (resultado.Success)
            {
                await DisplayAlert(
                    "NFC Activo",
                    "Acerca tu dispositivo al lector para validar el acceso.\n\n" +
                    $"ID: {_vm.Credencial.IdCriptografico}",
                    "OK");
            }
            else
            {
                await DisplayAlert(
                    "Error",
                    $"No se pudo activar el NFC: {resultado.ErrorMessage}",
                    "OK");
            }

            _vm.IsNFCActive = false;
        }
        catch (Exception ex)
        {
            _vm.IsNFCActive = false;
            await DisplayAlert("Error", $"Error al activar NFC: {ex.Message}", "OK");
        }
    }
}
