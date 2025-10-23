using AppNetCredenciales.services;
using AppNetCredenciales.ViewModel;

namespace AppNetCredenciales.Views;

public partial class EventoView : ContentPage
{

    private readonly AuthService _auth;
	public EventoView(AuthService auth)
	{
		InitializeComponent();
        this._auth = auth;
		BindingContext = new EventoViewModel(this, auth);
    }
    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool confirmar = await DisplayAlert("Cerrar sesión", "¿Deseas cerrar sesión?", "Sí", "No");
        if (confirmar)
        {
            SessionManager.Logout();
            // Redirige al login
            await Navigation.PushAsync(new LoginView(_auth));
        }
    }
}