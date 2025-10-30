using System;
using System.Formats.Asn1;
using AppNetCredenciales.Data;
using AppNetCredenciales.services;
using AppNetCredenciales.ViewModel;

namespace AppNetCredenciales.Views;

public partial class LoginView : ContentPage
{
    public LoginView(AuthService auth, LocalDBService db)
    {
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

    private async void chequearConectividad(object sender, EventArgs e)
    {

        NetworkAccess networkAccess = Connectivity.Current.NetworkAccess;

        if (networkAccess == NetworkAccess.Internet)
        {
            await DisplayAlert("Conectividad", "Estás conectado a Internet.", "OK");
        }
        else
        {
            await DisplayAlert("Conectividad", "No estás conectado a Internet.", "OK");
        }
    }
}