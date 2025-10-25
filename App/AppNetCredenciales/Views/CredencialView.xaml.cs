using AppNetCredenciales.Data;
using AppNetCredenciales.services;
using AppNetCredenciales.ViewModel;

namespace AppNetCredenciales.Views;

public partial class CredencialView : ContentPage
{
    private readonly AuthService _auth;
    private readonly CredencialViewModel _vm;

    public CredencialView(AuthService auth, LocalDBService db)
    {
        InitializeComponent();

        _auth = auth;

        _vm = new CredencialViewModel(auth, db);
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
}
