using AppNetCredenciales.ViewModel;
using AppNetCredenciales.Data;
using AppNetCredenciales.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AppNetCredenciales.Views
{
    public partial class NfcReaderActiveView : ContentPage
    {
        private bool _isAnimating;

        public NfcReaderActiveView()
        {
            InitializeComponent();

            var db = App.Services?.GetRequiredService<LocalDBService>();
            var nfcService = App.Services?.GetRequiredService<NfcService>();
            var apiService = App.Services?.GetRequiredService<ApiService>();
            var biometricService = App.Services?.GetRequiredService<BiometricService>();

            if (db != null && nfcService != null && apiService != null && biometricService != null)
            {
                BindingContext = new NfcReaderActiveViewModel(db, nfcService, apiService, biometricService);
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _isAnimating = true;
            _ = StartNfcAnimationAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _isAnimating = false;
        }

        private async Task StartNfcAnimationAsync()
        {
            try
            {
                while (_isAnimating)
                {
                    // Animación de ondas
                    _ = AnimateWaveAsync(WaveLabel1, 0);
                    await Task.Delay(300);
                    if (!_isAnimating) break;

                    _ = AnimateWaveAsync(WaveLabel2, 0);
                    await Task.Delay(300);
                    if (!_isAnimating) break;

                    _ = AnimateWaveAsync(WaveLabel3, 0);
                    await Task.Delay(700);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActiveView] Error en animación: {ex}");
            }
        }

        private async Task AnimateWaveAsync(Label wave, int delay)
        {
            try
            {
                if (delay > 0)
                    await Task.Delay(delay);

                // Reset
                wave.Scale = 0.5;
                wave.Opacity = 0;

                // Animate
                await Task.WhenAll(
                    wave.ScaleTo(2.0, 1500, Easing.CubicOut),
                    wave.FadeTo(1, 300),
                    Task.Delay(500).ContinueWith(_ => wave.FadeTo(0, 800))
                );

                // Reset final
                wave.Scale = 0.5;
                wave.Opacity = 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NfcReaderActiveView] Error en AnimateWave: {ex}");
            }
        }
    }
}
