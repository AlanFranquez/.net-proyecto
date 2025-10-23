using AppNetCredenciales.ViewModel;
using AppNetCredenciales.Views;
using AppNetCredenciales.services;
using AppNetCredenciales.Data;

namespace AppNetCredenciales
{
    public partial class App : Application
    {
        public App(LocalDBService db, AuthService auth, LoginView loginView, LoginViewModel loginViewModel)
        {
            InitializeComponent();

            MainPage = new NavigationPage(loginView);
        }
    }
}