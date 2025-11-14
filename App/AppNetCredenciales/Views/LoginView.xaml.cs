using AppNetCredenciales.Data;
using AppNetCredenciales.services;
using AppNetCredenciales.Services;
using AppNetCredenciales.ViewModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Networking;
using System;
using System.Formats.Asn1;
using System.Runtime.CompilerServices;

namespace AppNetCredenciales.Views;

public partial class LoginView : ContentPage
{
    private readonly ApiService _apiService = new ApiService();
    private readonly LocalDBService _dbService;
    public LoginView(AuthService auth, LocalDBService db)
    {
        this._dbService = db;
        SessionManager.Logout();
        InitializeComponent();
        BindingContext = new LoginViewModel(this, auth, db);
    }

    private async void OnVerUsuariosClicked(object sender, EventArgs e)
    {
        if (BindingContext is LoginViewModel vm)
        {
            await vm.ShowUsuariosAsync();
        }
    }

    // inside chequearConectividad
    private async void chequearConectividad(object sender, EventArgs e)
    {
        await _dbService.SincronizarRolesFromBack();
        await _dbService.SincronizarUsuariosFromBack();
        await _dbService.SincronizarEspaciosFromBack();
        await _dbService.SincronizarEventosFromBack();
        var raw = await _apiService.GetUsuariosRawAsync();
        System.Diagnostics.Debug.WriteLine($"[ApiTest] Raw users response: {raw}");
        await DisplayAlert("Raw response", (raw?.Length > 100 ? raw.Substring(0, 100) + "..." : raw) ?? "<null>", "OK");
        
     

        var usuarios = await _apiService.GetUsuariosAsync();
        if (usuarios.Count > 0)
        {

            var first = usuarios[0];
            System.Diagnostics.Debug.WriteLine($"[ApiTest] First usuarioId={first.UsuarioId} email={first.Email}");
            await DisplayAlert("API test", $"usuarioId: {first.UsuarioId}\nemail: {first.Email}\nnombre: {first.Nombre}", "OK");
        }
        else
        {
            await DisplayAlert("API test", "No users returned", "OK");
        }

        var networkAccess = Connectivity.Current.NetworkAccess;
        await DisplayAlert("Conectividad", networkAccess == NetworkAccess.Internet ? "Con Internet" : "Sin Internet", "OK");
    }
}