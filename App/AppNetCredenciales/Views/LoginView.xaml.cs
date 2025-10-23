using System;
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
}