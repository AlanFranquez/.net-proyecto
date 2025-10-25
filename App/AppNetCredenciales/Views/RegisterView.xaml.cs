using AppNetCredenciales.Data;
using AppNetCredenciales.services;
using AppNetCredenciales.ViewModel;

namespace AppNetCredenciales.Views;

public partial class RegisterView : ContentPage
{
	public RegisterView(AuthService auth, LocalDBService db)
	{
        SessionManager.Logout();
        InitializeComponent();
		BindingContext = new RegisterViewModel(this, auth, db);
	}
}