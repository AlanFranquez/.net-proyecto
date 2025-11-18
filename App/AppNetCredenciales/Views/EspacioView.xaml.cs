using AppNetCredenciales.Data;
using AppNetCredenciales.models;
using AppNetCredenciales.services;
using AppNetCredenciales.ViewModel;
using System.Diagnostics;

namespace AppNetCredenciales.Views;

public partial class EspacioView : ContentPage
{
    private readonly AuthService _auth;
    private readonly EspacioViewModel _viewModel;

    public EspacioView(AuthService auth, LocalDBService db)
    {
        InitializeComponent();

        this._auth = auth;
        _viewModel = new EspacioViewModel(auth, db);
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!await _auth.isUserLogged())
        {
            await Shell.Current.GoToAsync("login");
            return;
        }

        // Load espacios when the page appears
        await _viewModel.LoadEspaciosAsync();
    }

    // ✅ Método único para el botón "Ver Perfil" - hace exactamente lo mismo que la selección anterior
    private async void OnVerPerfilClicked(object sender, EventArgs e)
    {
        try
        {
            if (sender is Button button && button.CommandParameter is Espacio espacio)
            {
                System.Diagnostics.Debug.WriteLine($"[EspacioView] Ver perfil de: {espacio.Nombre} (ID: {espacio.idApi})");

                // ✅ Misma navegación que el método original OnEspacioSelected
                await Shell.Current.GoToAsync($"espacioPerfil?espacioId={espacio.idApi}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[EspacioView] Error en OnVerPerfilClicked: {ex.Message}");
            await DisplayAlert("Error", "No se pudo abrir el perfil del espacio.", "OK");
        }
    }
}