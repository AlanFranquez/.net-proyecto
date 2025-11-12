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

    private async void OnEspacioSelected(object sender, SelectionChangedEventArgs e)
    {
        var seleccionado = e.CurrentSelection?.FirstOrDefault() as Espacio;
        if (seleccionado == null)
            return;

        // Match the route registered in AppShell.xaml.cs (espacioPerfil)
        await Shell.Current.GoToAsync($"espacioPerfil?espacioId={seleccionado.EspacioId}");

        if (sender is CollectionView cv) cv.SelectedItem = null;
    }
}