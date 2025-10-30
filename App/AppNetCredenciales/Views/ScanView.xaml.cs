using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Controls;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace AppNetCredenciales.Views
{
    public partial class ScanView : ContentPage
    {
        string lastDetectedBarcode = string.Empty;
        DateTime lastDetectedTime = DateTime.MinValue;
        CancellationTokenSource? _retryCts;

        public ScanView()
        {
            InitializeComponent();
            // start disabled until permissions are granted
            cameraBarcodeReaderView.IsDetecting = false;
            cameraBarcodeReaderView.Options = new ZXing.Net.Maui.BarcodeReaderOptions
            {
                Formats = ZXing.Net.Maui.BarcodeFormat.Ean13, 
                AutoRotate = true,
                Multiple = true 
            };
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // ensure camera permission before enabling detection
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
            // stop detection and cancel any pending retries
            _retryCts?.Cancel();
            cameraBarcodeReaderView.IsDetecting = false;
            base.OnDisappearing();
        }

        // Turn detection on with basic error handling + retry
        private async Task StartDetectionSafeAsync()
        {
            try
            {
                // toggle to ensure underlying camera is restarted cleanly
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
                    System.Diagnostics.Debug.WriteLine($"Retry {i+1} failed: {retryEx}");
                }
            }

            // final fallback: inform the user
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert("Camera error", "Unable to start the camera. Try reopening the page or using a physical device.", "OK");
            });
        }

        protected void BarcodesDetected(object sender, ZXing.Net.Maui.BarcodeDetectionEventArgs e)
        {
            var first = e.Results?.FirstOrDefault();
            if (first is null)
            {
                return;
            }

            if (first.Value == lastDetectedBarcode && (DateTime.Now - lastDetectedTime).TotalSeconds < 1)
            {
                return;
            }

            lastDetectedBarcode = first.Value;
            lastDetectedTime = DateTime.Now;

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                cameraBarcodeReaderView.IsDetecting = false;
                await DisplayAlert("Barcode Detected", first.Value, "OK");
                cameraBarcodeReaderView.IsDetecting = true;
            });
        }
    }
}