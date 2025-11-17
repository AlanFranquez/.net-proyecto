using AppNetCredenciales.Views;
using Microsoft.Maui.ApplicationModel; // for MainThread
using AppNetCredenciales.Services;

namespace AppNetCredenciales
{
    public partial class AppShell : Shell
    {
        private readonly ConnectivityService _connectivityService;
        private bool _offlineAlertShown;

        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("register", typeof(AppNetCredenciales.Views.RegisterView));
            Routing.RegisterRoute("espacio", typeof(AppNetCredenciales.Views.EspacioView));
            Routing.RegisterRoute("credencial", typeof(AppNetCredenciales.Views.CredencialView));
            Routing.RegisterRoute("espacioPerfil", typeof(AppNetCredenciales.Views.EspacioPerfilView));
            Routing.RegisterRoute("scan", typeof(AppNetCredenciales.Views.ScanView));
            Routing.RegisterRoute("historial", typeof(AppNetCredenciales.Views.HistorialView));
            Routing.RegisterRoute("accesoPerfil", typeof(AppNetCredenciales.Views.AccesoPerfilView));
            Routing.RegisterRoute("nfcReader", typeof(AppNetCredenciales.Views.NFCReaderView));


            _connectivityService = App.Services.GetRequiredService<ConnectivityService>();
            _connectivityService.ConnectivityChanged += ConnectivityService_ConnectivityChanged;
        }

        private void ConnectivityService_ConnectivityChanged(object? sender, bool isConnected)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (!isConnected && !_offlineAlertShown)
                {
                    _offlineAlertShown = true;
                    await Current.DisplayAlert("Sin conexión", "No hay conexión a Internet.", "OK");
                }
                else if (isConnected)
                {
                    _offlineAlertShown = false;
                    // optionally show reconnection message or refresh data
                    // await Current.DisplayAlert("Conectado", "Se recuperó la conexión.", "OK");
                }
            });
        }
    }
}
