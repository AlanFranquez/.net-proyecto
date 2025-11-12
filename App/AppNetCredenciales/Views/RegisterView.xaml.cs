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

    private void RolesCollection_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (BindingContext is not RegisterViewModel vm)
            return;

        foreach (var item in vm.Roles)
            item.IsSelected = false;

        foreach (var si in RolesCollection.SelectedItems)
        {
            if (si is RegisterViewModel.SelectableRole sr)
                sr.IsSelected = true;
        }
    }
}