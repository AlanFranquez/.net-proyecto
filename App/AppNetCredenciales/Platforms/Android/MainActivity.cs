using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Content;
using Android.Nfc;
using AppNetCredenciales.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AppNetCredenciales
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    [IntentFilter(new[] { NfcAdapter.ActionNdefDiscovered, NfcAdapter.ActionTagDiscovered, NfcAdapter.ActionTechDiscovered }, Categories = new[] { Intent.CategoryDefault })]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            // Inicializar NFCService con la Activity
            var nfcService = App.Services?.GetService<NFCService>();
            if (nfcService != null)
            {
                nfcService.Initialize(this);
                System.Diagnostics.Debug.WriteLine("[MainActivity] NFCService inicializado");
            }
        }

        protected override void OnNewIntent(Intent? intent)
        {
            base.OnNewIntent(intent);
            
            if (intent != null)
            {
                System.Diagnostics.Debug.WriteLine($"[MainActivity] OnNewIntent - Action: {intent.Action}");
                
                // Procesar intent NFC
                var nfcService = App.Services?.GetService<NFCService>();
                if (nfcService != null && 
                    (intent.Action == NfcAdapter.ActionNdefDiscovered ||
                     intent.Action == NfcAdapter.ActionTagDiscovered ||
                     intent.Action == NfcAdapter.ActionTechDiscovered))
                {
                    nfcService.ProcessNfcIntent(intent);
                }
            }
        }
    }
}
