using System;
using System.Formats.Asn1;
using AppNetCredenciales.services;
using AppNetCredenciales.ViewModel;

namespace AppNetCredenciales.Views;

public partial class LoginView : ContentPage
{
    public LoginView(AuthService auth)
    {
        SessionManager.Logout();
        InitializeComponent();
        BindingContext = new LoginViewModel(this, auth);
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
            await DisplayAlert("Conectividad", "Est�s conectado a Internet.", "OK");
        }
        else
        {
            await DisplayAlert("Conectividad", "No est�s conectado a Internet.", "OK");
        }
    }
}