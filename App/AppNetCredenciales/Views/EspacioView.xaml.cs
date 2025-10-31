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

        await _viewModel.LoadEspaciosAsync();
    }

    private async void OnEspacioSelected(object sender, SelectionChangedEventArgs e)
    {
        var espacio = e.CurrentSelection.FirstOrDefault() as Espacio;
        if (espacio == null)
            return;


        // Mostrar informaci�n en consola / Output
        Debug.WriteLine($"ID: {espacio.EspacioId}");
        Debug.WriteLine($"T�tulo: {espacio.Titulo}");
        Debug.WriteLine($"Descripci�n: {espacio.Descripcion}");
        Debug.WriteLine($"Fecha: {espacio.Fecha}");
        Debug.WriteLine($"Lugar: {espacio.Lugar}");
        Debug.WriteLine($"Stock: {espacio.Stock}");
        Debug.WriteLine($"Disponible: {espacio.Disponible}");
        Debug.WriteLine($"Publicado: {espacio.Publicado}");

        await Shell.Current.GoToAsync($"espacioPerfil?id={espacio.EspacioId}");

        if (sender is CollectionView cv)
            cv.SelectedItem = null;
    }

    
}