using System.Diagnostics;
using System.Threading.Tasks;
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
        private readonly LocalDBService _db;
        public App(LocalDBService db, AuthService auth, LoginView loginView, LoginViewModel loginViewModel)
        {
            InitializeComponent();
            this._db = db;
            MainPage = new AppShell();

            // Fire-and-forget an async initializer (avoid blocking the UI thread)
            _ = InitializeAppAsync();
        }

        private async Task InitializeAppAsync()
        {
            try
            {
                await _db.InitializeAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DB initialization failed: {ex}");
            }
        }
    }
}