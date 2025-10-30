using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using System.Linq;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace AppNetCredenciales.Views
{
    public partial class ScanView : ContentPage
    {
        string lastDetectedBarcode = string.Empty;
        DateTime lastDetectedTime = DateTime.MinValue;

        public ScanView()
        {
            InitializeComponent();
            cameraBarcodeReaderView.Options = new ZXing.Net.Maui.BarcodeReaderOptions
            {
                Formats = ZXing.Net.Maui.BarcodeFormat.Ean13, 
                AutoRotate = true,
                Multiple = true 
            };
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

            Dispatcher.DispatchAsync(async () =>
            {
                await DisplayAlert("Barcode Detected", first.Value, "OK");
            });
        }
    }
}