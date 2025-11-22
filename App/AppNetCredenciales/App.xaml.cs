using System.Diagnostics;
using System.Threading.Tasks;
using AppNetCredenciales.ViewModel;
using AppNetCredenciales.Views;
using AppNetCredenciales.services;
using AppNetCredenciales.Services;
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
        private readonly NfcService _nfcService;

        public static IServiceProvider Services { get; set; }

        public App(LocalDBService db, AuthService auth, NfcService nfcService, LoginView loginView, LoginViewModel loginViewModel)
        {
            InitializeComponent();
            this._db = db;
            this._nfcService = nfcService;
            MainPage = new AppShell();

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
                    System.Diagnostics.Debug.WriteLine("[App] DB error: " + ex);
                }
            });
        }

        private async Task InitializeAppAsync()
        {
            try
            {
                await _db.InitializeAsync();
                
                _nfcService.Initialize();
              
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[App]: {ex}");
            }
        }
    }
}