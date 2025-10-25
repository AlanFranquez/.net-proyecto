using Microsoft.Maui.Controls;
using QRCoder;
using System.IO;

namespace AppNetCredenciales.Views;

public partial class QRModalPage : ContentPage
{
    public QRModalPage(string qrData)
    {
        InitializeComponent();
        GenerateQR(qrData);
    }

    private void GenerateQR(string data)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        var qrBytes = qrCode.GetGraphic(20);

        QrImage.Source = ImageSource.FromStream(() => new MemoryStream(qrBytes));
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        try
        {
            // safe guard: only pop if there is a modal
            if (Navigation?.ModalStack?.Count > 0)
                await Navigation.PopModalAsync();
            else
                await Task.CompletedTask;
        }
        catch (Exception)
        {
            // swallow until root fix applied
        }
    }
}