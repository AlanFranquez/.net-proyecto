using AppNetCredenciales.Data;
using AppNetCredenciales.services;
using AppNetCredenciales.Services;
using AppNetCredenciales.ViewModel;
using AppNetCredenciales.Views;
using Camera.MAUI;
using CommunityToolkit.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using System;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace AppNetCredenciales
{
    public static class MauiProgram
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCameraView()
                .UseMauiCommunityToolkit()
                .UseBarcodeReader()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<LocalDBService>();
            builder.Services.AddTransient<MainPage>();

            // 🔔 SERVICIOS DE NOTIFICACIONES PUSH
            builder.Services.AddSingleton<ApiService>();
            builder.Services.AddSingleton<PushNotificationService>();
            builder.Services.AddSingleton<BeneficiosWatcherService>();
            builder.Services.AddSingleton<BackgroundBeneficiosService>();

            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<LoginViewModel>();
            builder.Services.AddSingleton<LoginView>();
            builder.Services.AddSingleton<App>();
            builder.Services.AddSingleton<RegisterView>();
            builder.Services.AddSingleton<EspacioView>();
            builder.Services.AddSingleton<EspacioViewModel>();
            builder.Services.AddSingleton<ConectivityService>();
            builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);
            builder.Services.AddSingleton<CredencialView>();
            builder.Services.AddSingleton<CredencialViewModel>();
            builder.Services.AddSingleton<EspacioPerfilView>();
            builder.Services.AddSingleton<EspacioPerfilViewModel>();
            builder.Services.AddSingleton<ScanView>();
            builder.Services.AddSingleton<HistorialView>();
            builder.Services.AddSingleton<AppNetCredenciales.Services.ConnectivityService>();
            
            // Nuevos servicios de seguridad
            builder.Services.AddSingleton<BiometricService>();
            builder.Services.AddSingleton<NFCService>();
            
            // Vista del lector NFC
            builder.Services.AddSingleton<NFCReaderView>();
            
#if DEBUG
            builder.Logging.AddDebug();
#endif 

            var app = builder.Build();

            ServiceProvider = app.Services;
            App.Services = app.Services;

            // 🚀 INICIALIZAR SERVICIOS DE NOTIFICACIONES AUTOMÁTICAMENTE
            InitializeNotificationServices(app.Services);

            return app;
        }

        /// <summary>
        /// Inicializa los servicios de notificaciones automáticamente al startup
        /// </summary>
        private static async void InitializeNotificationServices(IServiceProvider serviceProvider)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[MauiProgram] 🚀 Inicializando servicios de notificaciones...");

                // Obtener servicios del contenedor DI
                var pushService = serviceProvider.GetService<PushNotificationService>();
                var watcherService = serviceProvider.GetService<BeneficiosWatcherService>();
                var backgroundService = serviceProvider.GetService<BackgroundBeneficiosService>();

                if (pushService != null)
                {
                    await pushService.InitializeAsync();
                    System.Diagnostics.Debug.WriteLine("[MauiProgram] ✅ PushNotificationService inicializado");
                }

                if (backgroundService != null)
                {
                    await backgroundService.StartAsync();
                    System.Diagnostics.Debug.WriteLine("[MauiProgram] ✅ BackgroundBeneficiosService iniciado");
                }

                System.Diagnostics.Debug.WriteLine("[MauiProgram] 🔔 Sistema de notificaciones automáticas activo");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MauiProgram] ❌ Error inicializando servicios: {ex.Message}");
            }
        }
    }
}