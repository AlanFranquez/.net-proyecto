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

            _biometricAuthenticated = false;

           
            bool userWantsToAuthenticate = await DisplayAlert(
                "Autenticaci�n Requerida",
                "Debes verificar tu identidad con huella digital antes de escanear credenciales.\n\n�Deseas continuar?",
                "Autenticar",
                "Cancelar");

            if (!userWantsToAuthenticate)
            {
                await DisplayAlert("Autenticaci�n Cancelada", 
                    "Debes autenticarte con tu huella digital para usar el esc�ner.", 
                    "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            var biometricResult = await _biometricService.AuthenticateAsync(
                "Verificar tu identidad para escanear credenciales");

            //if (!biometricResult.Success)
            //{
            //    await DisplayAlert("Autenticaci�n Fallida", 
            //        biometricResult.ErrorMessage ?? "No se pudo verificar tu identidad.", 
            //        "OK");
            //    await Shell.Current.GoToAsync("..");
            //    return;
            //}

            _biometricAuthenticated = true;

     
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.Camera>();

            if (status == PermissionStatus.Granted)
            {
                await StartDetectionSafeAsync();
            }
            else
            {
                await DisplayAlert("Permission required", "Son requeridos permisos de camara para escanear el QR", "OK");
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

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert("Camera error", "Unable to start the camera. Try reopening the page or using a physical device.", "OK");
            });
        }

        protected async void BarcodesDetected(object? sender, ZXing.Net.Maui.BarcodeDetectionEventArgs e)
        {
            
            if (!_biometricAuthenticated)
            {
                System.Diagnostics.Debug.WriteLine("[Scan] Intento de escaneo sin autenticaci�n biom�trica");
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
            try
            {
                System.Diagnostics.Debug.WriteLine($"[Scan] === PROCESSING SCANNED PAYLOAD ===");
                System.Diagnostics.Debug.WriteLine($"[Scan] Payload: '{payload}'");

                var parts = payload?.Split('|');
                if (parts == null || parts.Length < 2)
                {
                    await DisplayAlert("QR Inválido", "El código QR no tiene el formato correcto.", "OK");
                    return;
                }

                var cryptoId = parts[0].Trim();
                var eventoId = parts[1].Trim();

                System.Diagnostics.Debug.WriteLine($"[Scan] CryptoId: '{cryptoId}'");
                System.Diagnostics.Debug.WriteLine($"[Scan] EventoId: '{eventoId}'");

                var usuario = await _db.GetLoggedUserAsync();
                if (usuario == null)
                {
                    await DisplayAlert("Error", "Usuario no encontrado.", "OK");
                    return;
                }

                // Buscar credencial por IdCriptografico
                var credenciales = await _db.GetCredencialesAsync();
                Credencial cred = null;

                foreach (var c in credenciales)
                {
                    if (c == null) continue;
                    if (c.IdCriptografico == cryptoId)
                    {
                        cred = c;
                        break;
                    }
                }

                // Buscar espacio por idApi
                Espacio espacio = null;
                var espacios = await _db.GetEspaciosAsync();

                foreach (var esp in espacios)
                {
                    if (esp == null) continue;
                    if (esp.idApi == eventoId)
                    {
                        espacio = esp;
                        break;
                    }
                }

                

                // Validaciones
                if (cred == null && espacio == null)
                {
                    await DisplayAlert("Credencial y Espacio no reconocidos",
                        $"No se encontró la credencial para '{cryptoId}' ni el espacio para '{eventoId}'.", "Cerrar");
                    return;
                }

                if (cred == null || espacio == null)
                {
                    await DisplayAlert("Acceso Denegado",
                        $"No se encontró {(cred == null ? "la credencial" : "el espacio")}.", "Cerrar");

                    // Solo crear evento denegado si tenemos al menos el espacio
                    if (espacio != null)
                    {
                        // Para evento denegado:
                        var evNegado = new EventoAcceso
                        {
                            MomentoDeAcceso = DateTime.UtcNow,
                            CredencialIdApi = cred?.idApi, // Puede ser null
                            EspacioIdApi = espacio.idApi,
                            Espacio = espacio,
                            Resultado = AccesoTipo.Denegar, // ✅ Esto se convierte en "Denegar"
                            Motivo = cred == null ? "Credencial no encontrada" : "Credencial inválida"
                        };

                        await _db.SaveAndPushEventoAccesoAsync(evNegado);
                    }
                    return;
                }

                if (string.IsNullOrEmpty(cred.idApi))
                {
                    System.Diagnostics.Debug.WriteLine($"[Scan] ⚠️ Credencial sin idApi - ID: {cred.CredencialId}");
                    await DisplayAlert("Error de Credencial",
                        "La credencial no tiene ID de API válido. Contacte al administrador.", "OK");
                    return;
                }


                if (cred.FechaExpiracion.HasValue && cred.FechaExpiracion.Value.Date < DateTime.Today)
                {
                    await DisplayAlert("Credencial Expirada",
                        $"La credencial expiró el {cred.FechaExpiracion.Value:dd/MM/yyyy}.\nAcceso denegado.",
                        "Cerrar");

                   
                    var eventoExpirado = new EventoAcceso
                    {
                        MomentoDeAcceso = DateTime.UtcNow,
                        CredencialIdApi = cred.idApi,
                        EspacioIdApi = espacio.idApi,
                        Credencial = cred,
                        Espacio = espacio,
                        Resultado = AccesoTipo.Denegar,
                        Motivo = $"Credencial expirada el {cred.FechaExpiracion.Value:dd/MM/yyyy}"
                    };

                    await _db.SaveAndPushEventoAccesoAsync(eventoExpirado);
                    return;
                }

                var popupOk = new ScanResultPopup("Credencial reconocida",
                    $"El usuario tiene permiso para acceder al espacio '{espacio.Nombre}'.", true);
                await this.ShowPopupAsync(popupOk);

                var ev = new EventoAcceso
                {
                    MomentoDeAcceso = DateTime.UtcNow, 

                    CredencialId = cred.CredencialId,
                    EspacioId = espacio.EspacioId,

                    CredencialIdApi = cred.idApi,
                    EspacioIdApi = espacio.idApi,

                    Credencial = cred,
                    Espacio = espacio,

                    Resultado = AccesoTipo.Permitir, 
                    Motivo = "Acceso autorizado"     
                };

                

                await _db.SaveAndPushEventoAccesoAsync(ev);

                System.Diagnostics.Debug.WriteLine($"[Scan] Event created successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Scan] ERROR HandleScannedPayloadAsync: {ex}");
                await DisplayAlert("Error", $"Error procesando el codigo QR: {ex.Message}", "OK");
            }
        }
    }   
}