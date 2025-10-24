using AppNetCredenciales.ViewModel;
using AppNetCredenciales.Views;
using AppNetCredenciales.services;
using AppNetCredenciales.Data;
using SQLitePCL;

namespace AppNetCredenciales
{
    public partial class App : Application
    {
        private readonly AuthService _auth;
        public App(LocalDBService db, AuthService auth, LoginView loginView, LoginViewModel loginViewModel)
        {
            InitializeComponent();
            MainPage = new AppShell();

            Shell.Current.GoToAsync("//login");
        }



    }
}