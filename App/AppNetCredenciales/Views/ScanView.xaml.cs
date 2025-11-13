using AppNetCredenciales.Data;
using AppNetCredenciales.models;
using AppNetCredenciales.Services;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using SQLite;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace AppNetCredenciales.Views
{
    public partial class ScanView : ContentPage
    {
        string lastDetectedBarcode = string.Empty;
        DateTime lastDetectedTime = DateTime.MinValue;
        CancellationTokenSource? _retryCts;
        private readonly LocalDBService _db;
        private readonly BiometricService _biometricService;
        private bool _biometricAuthenticated = false;

        public ScanView()
        {
            InitializeComponent();
            cameraBarcodeReaderView.IsDetecting = false;
            cameraBarcodeReaderView.Options = new ZXing.Net.Maui.BarcodeReaderOptions
            {
                Formats = ZXing.Net.Maui.BarcodeFormat.QrCode,
                AutoRotate = true,
                Multiple = false
            };

            _db = App.Services?.GetRequiredService<LocalDBService>()
                  ?? throw new InvalidOperationException("LocalDBService not registered in DI.");
            
            _biometricService = App.Services?.GetRequiredService<BiometricService>()
                  ?? new BiometricService();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Resetear autenticación biométrica cada vez que aparece la vista
            _biometricAuthenticated = false;

            // Solicitar autenticación biométrica PRIMERO usando DisplayAlert
            bool userWantsToAuthenticate = await DisplayAlert(
                "Autenticación Requerida",
                "Debes verificar tu identidad con huella digital antes de escanear credenciales.\n\n¿Deseas continuar?",
                "Autenticar",
                "Cancelar");

            if (!userWantsToAuthenticate)
            {
                await DisplayAlert("Autenticación Cancelada", 
                    "Debes autenticarte con tu huella digital para usar el escáner.", 
                    "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            // Realizar autenticación biométrica real
            var biometricResult = await _biometricService.AuthenticateAsync(
                "Verificar tu identidad para escanear credenciales");

            if (!biometricResult.Success)
            {
                await DisplayAlert("Autenticación Fallida", 
                    biometricResult.ErrorMessage ?? "No se pudo verificar tu identidad.", 
                    "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            _biometricAuthenticated = true;

            // Ahora sí, solicitar permisos de cámara
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.Camera>();

            if (status == PermissionStatus.Granted)
            {
                await StartDetectionSafeAsync();
            }
            else
            {
                await DisplayAlert("Permission required", "Camera permission is required to scan QR codes.", "OK");
            }
        }

        protected override void OnDisappearing()
        {
            _retryCts?.Cancel();
            cameraBarcodeReaderView.IsDetecting = false;
            _biometricAuthenticated = false;
            base.OnDisappearing();
        }

        private async Task StartDetectionSafeAsync()
        {
            try
            {
                cameraBarcodeReaderView.IsDetecting = false;
                await Task.Delay(200);
                cameraBarcodeReaderView.IsDetecting = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StartDetectionSafeAsync error: {ex}");
                // schedule a few retries
                _retryCts?.Cancel();
                _retryCts = new CancellationTokenSource();
                _ = RetryStartAsync(_retryCts.Token);
            }
        }

        private async Task RetryStartAsync(CancellationToken ct)
        {
            const int retries = 3;
            for (int i = 0; i < retries && !ct.IsCancellationRequested; i++)
            {
                await Task.Delay(1000, ct).ContinueWith(_ => { }, TaskScheduler.Default);
                try
                {
                    cameraBarcodeReaderView.IsDetecting = true;
                    return;
                }
                catch (Exception retryEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Retry {i + 1} failed: {retryEx}");
                }
            }

            // final fallback: inform the user
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert("Camera error", "Unable to start the camera. Try reopening the page or using a physical device.", "OK");
            });
        }

        protected async void BarcodesDetected(object? sender, ZXing.Net.Maui.BarcodeDetectionEventArgs e)
        {
            // Verificar que el usuario esté autenticado biométricamente
            if (!_biometricAuthenticated)
            {
                System.Diagnostics.Debug.WriteLine("[Scan] Intento de escaneo sin autenticación biométrica");
                return;
            }

            var first = e.Results?.FirstOrDefault();
            if (first is null) return;

            var payload = (first.Value ?? string.Empty).Trim();
            System.Diagnostics.Debug.WriteLine($"[Scan] Scanned payload: '{payload}'");

            if (payload == lastDetectedBarcode && (DateTime.Now - lastDetectedTime).TotalSeconds < 1) return;
            lastDetectedBarcode = payload;
            lastDetectedTime = DateTime.Now;

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                cameraBarcodeReaderView.IsDetecting = false;
                try
                {
                    await HandleScannedPayloadAsync(payload);
                }
                finally
                {
                    cameraBarcodeReaderView.IsDetecting = true;
                }
            });
        }

        private async Task HandleScannedPayloadAsync(string payload)
        {
            var cryptoId = payload?.Split('|')[0].Trim();

            var eventoId = payload?.Split('|').Length > 1
                ? payload.Split('|')[1].Trim()
                : string.Empty;

            var usuario = await _db.GetLoggedUserAsync();
            if (usuario == null) return;

            

            var eventos = await _db.GetEspaciosAsync();
            Espacio evento = null;
            foreach (var e in eventos)
            {
                if(e == null) continue;
                if (e.idApi == eventoId)
                {
                    evento = e;
                }
            }


            var credenciales = await _db.GetCredencialesAsync();

            Credencial cred = null;

            foreach(var c in credenciales)
            {

                if (c == null) continue;
                if(c.IdCriptografico == cryptoId)
                {
                    cred = c;
                }
            }

            if (cred == null || evento == null)
            {
                await DisplayAlert("Credencial no reconocida", $"No se encontrÃ³ la credencial para '{cryptoId}'.", "Cerrar");

                var evNegado = new EventoAcceso
                {
                    MomentoDeAcceso = DateTime.Now,
                    CredencialId = usuario.CredencialId,
                    Credencial = usuario.Credencial,
                    Espacio = evento,
<<<<<<< Updated upstream
                    EspacioId = evento?.EspacioId ?? 0,
                    EspacioIdApi = evento?.idApi,
=======
                    EspacioId = evento.EspacioId,
                    EspacioIdApi = evento.idApi,
>>>>>>> Stashed changes
                    Resultado = AccesoTipo.Denegar
                };

                await _db.SaveAndPushEventoAccesoAsync(evNegado);
                return;
            }

            var popupOk = new ScanResultPopup("Credencial reconocida", $"El usuario tiene permiso para acceder al espacio.", true);
            await this.ShowPopupAsync(popupOk);

            var ev = new EventoAcceso
            {
                MomentoDeAcceso = DateTime.Now,
                CredencialId = usuario.CredencialId,
                Credencial = usuario.Credencial,
                Espacio = evento,
                EspacioId = evento.EspacioId,
                EspacioIdApi = evento.idApi,
                Resultado = AccesoTipo.Permitir
            };

            await _db.SaveAndPushEventoAccesoAsync(ev);
        }
    }   
}