using System.Diagnostics;
using System.Threading.Tasks;
using AppNetCredenciales.ViewModel;
using AppNetCredenciales.Views;
using AppNetCredenciales.services;
using AppNetCredenciales.Data;
using SQLitePCL;
using System;
using Microsoft.Maui.Controls;

namespace AppNetCredenciales
{
    public partial class App : Application
    {
        private readonly AuthService _auth;
        private readonly LocalDBService _db;

        // Expose the IServiceProvider so XAML-created views can resolve services
        public static IServiceProvider Services { get; set; }

        public App(LocalDBService db, AuthService auth, LoginView loginView, LoginViewModel loginViewModel)
        {
            InitializeComponent();
            this._db = db;
            MainPage = new AppShell();

            // Fire-and-forget an async initializer (avoid blocking the UI thread)
            _ = InitializeAppAsync();
            _ = Task.Run(async () =>
            {
                try
                {
#if DEBUG
                    var db = MauiProgram.ServiceProvider?.GetService<LocalDBService>();
                    if (db != null)
                    {
                        await db.EnsureSchemaAndDataAsync();
                    }
#endif
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("[App] DB migration error: " + ex);
                }
            });
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