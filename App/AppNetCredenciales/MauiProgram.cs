using AppNetCredenciales.Data;
using AppNetCredenciales.services;
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
#if DEBUG
            builder.Logging.AddDebug();
#endif 

            var app = builder.Build();

            ServiceProvider = app.Services;
            App.Services = app.Services;

            return app;
        }
    }
}