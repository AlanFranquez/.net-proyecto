using AppNetCredenciales.Data;
using AppNetCredenciales.models;
using AppNetCredenciales.Services;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AppNetCredenciales.Views
{
    public partial class NFCReaderView : ContentPage
    {
        private readonly NFCService _nfcService;
        private readonly LocalDBService _db;
        private readonly BiometricService _biometricService;
        private bool _isReading = false;
        private string? _lastTagId = null;
        private string? _lastTagData = null;

        public NFCReaderView()
        {
            InitializeComponent();

            _nfcService = App.Services?.GetRequiredService<NFCService>()
                ?? throw new InvalidOperationException("NFCService not registered in DI.");
            
            _db = App.Services?.GetRequiredService<LocalDBService>()
                ?? throw new InvalidOperationException("LocalDBService not registered in DI.");
            
            _biometricService = App.Services?.GetRequiredService<BiometricService>()
                ?? new BiometricService();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Verificar que el usuario sea Funcionario
            var usuario = await _db.GetLoggedUserAsync();
            if (usuario == null)
            {
                await DisplayAlert("Error", "No hay usuario autenticado.", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            // Verificar que tiene rol de Funcionario
            var roles = await _db.GetRolesAsync();
            var userRoleIds = usuario.RolesIDs ?? Array.Empty<string>();
            bool isFuncionario = roles.Any(r =>
                string.Equals(r.Tipo?.Trim(), "Funcionario", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(r.idApi)
                && userRoleIds.Contains(r.idApi, StringComparer.OrdinalIgnoreCase));

            if (!isFuncionario)
            {
                await DisplayAlert("Acceso Denegado", 
                    "Esta funcionalidad es exclusiva para Funcionarios.", 
                    "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            // Autenticación biométrica
            bool userWantsToAuthenticate = await DisplayAlert(
                "Autenticación Requerida",
                "Debes verificar tu identidad con huella digital antes de usar el lector NFC.\n\n¿Deseas continuar?",
                "Autenticar",
                "Cancelar");

            if (!userWantsToAuthenticate)
            {
                await DisplayAlert("Autenticación Cancelada", 
                    "Debes autenticarte para usar el lector NFC.", 
                    "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            var biometricResult = await _biometricService.AuthenticateAsync(
                "Verificar tu identidad para usar el lector NFC");

            if (!biometricResult.Success)
            {
                await DisplayAlert("Autenticación Fallida", 
                    biometricResult.ErrorMessage ?? "No se pudo verificar tu identidad.", 
                    "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            // Verificar disponibilidad de NFC
            bool nfcAvailable = await _nfcService.IsNFCAvailableAsync();
            if (!nfcAvailable)
            {
                await DisplayAlert("NFC No Disponible", 
                    "Este dispositivo no tiene NFC o está deshabilitado. Por favor, activa NFC en la configuración de tu dispositivo.", 
                    "OK");
            }
        }

        protected override void OnDisappearing()
        {
            if (_isReading)
            {
                _nfcService.StopReading();
                _isReading = false;
            }
            base.OnDisappearing();
        }

        private async void OnStartReadClicked(object sender, EventArgs e)
        {
            try
            {
                // Resetear información previa
                TagInfoFrame.IsVisible = false;
                ProcessButton.IsVisible = false;
                _lastTagId = null;
                _lastTagData = null;

                // Cambiar UI a modo lectura
                StartReadButton.IsVisible = false;
                StopReadButton.IsVisible = true;
                StatusLabel.Text = "?? Esperando tag NFC... Acerca el dispositivo del usuario";
                StatusLabel.TextColor = Colors.Orange;

                // Iniciar animación
                _ = StartNfcAnimation();

                _isReading = true;

                // Iniciar lectura NFC
                var result = await _nfcService.StartReadingAsync();

                // Detener animación
                await StopNfcAnimation();

                if (result.Success)
                {
                    _lastTagId = result.TagId;
                    _lastTagData = result.Data;

                    // Mostrar información del tag
                    TagIdLabel.Text = result.TagId ?? "N/A";
                    TagDataLabel.Text = result.Data ?? "Sin datos NDEF";
                    TagInfoFrame.IsVisible = true;
                    ProcessButton.IsVisible = true;

                    StatusLabel.Text = "? Tag NFC leído correctamente";
                    StatusLabel.TextColor = Colors.Green;

                    // Vibración de éxito
#if ANDROID || IOS
                    try
                    {
                        var vibration = Microsoft.Maui.Devices.Vibration.Default;
                        vibration.Vibrate(TimeSpan.FromMilliseconds(200));
                    }
                    catch { }
#endif
                }
                else
                {
                    StatusLabel.Text = $"? Error: {result.ErrorMessage}";
                    StatusLabel.TextColor = Colors.Red;
                    
                    await DisplayAlert("Error de Lectura", result.ErrorMessage ?? "No se pudo leer el tag NFC.", "OK");
                }

                // Restaurar botones
                StartReadButton.IsVisible = true;
                StopReadButton.IsVisible = false;
                _isReading = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NFCReaderView] Error en OnStartReadClicked: {ex}");
                StatusLabel.Text = $"? Error inesperado: {ex.Message}";
                StatusLabel.TextColor = Colors.Red;
                
                StartReadButton.IsVisible = true;
                StopReadButton.IsVisible = false;
                _isReading = false;
                
                await StopNfcAnimation();
            }
        }

        private void OnStopReadClicked(object sender, EventArgs e)
        {
            if (_isReading)
            {
                _nfcService.StopReading();
                _isReading = false;
                
                StartReadButton.IsVisible = true;
                StopReadButton.IsVisible = false;
                
                StatusLabel.Text = "?? Lectura cancelada";
                StatusLabel.TextColor = Colors.Gray;
                
                _ = StopNfcAnimation();
            }
        }

        private async void OnProcessClicked(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(_lastTagData))
                {
                    await DisplayAlert("Error", "No hay datos para procesar.", "OK");
                    return;
                }

                ProcessButton.IsEnabled = false;
                StatusLabel.Text = "? Procesando credencial...";
                StatusLabel.TextColor = Colors.Blue;

                // Procesar los datos del tag (similar a cómo se procesa un QR)
                await HandleNFCDataAsync(_lastTagData, _lastTagId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NFCReaderView] Error en OnProcessClicked: {ex}");
                await DisplayAlert("Error", $"Error al procesar: {ex.Message}", "OK");
            }
            finally
            {
                ProcessButton.IsEnabled = true;
            }
        }

        private async Task HandleNFCDataAsync(string data, string? tagId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[NFCReaderView] === PROCESANDO DATOS NFC ===");
                System.Diagnostics.Debug.WriteLine($"[NFCReaderView] Datos: '{data}'");
                System.Diagnostics.Debug.WriteLine($"[NFCReaderView] TagId: '{tagId}'");

                // Intentar parsear el formato: "CryptoId|EspacioId" (similar al QR)
                var parts = data?.Split('|');
                
                if (parts == null || parts.Length < 2)
                {
                    // Si no tiene el formato esperado, buscar por TagId
                    await ProcessByTagIdAsync(tagId);
                    return;
                }

                var cryptoId = parts[0].Trim();
                var espacioId = parts[1].Trim();

                System.Diagnostics.Debug.WriteLine($"[NFCReaderView] CryptoId: '{cryptoId}'");
                System.Diagnostics.Debug.WriteLine($"[NFCReaderView] EspacioId: '{espacioId}'");

                // Buscar credencial por IdCriptografico
                var credenciales = await _db.GetCredencialesAsync();
                Credencial? cred = credenciales.FirstOrDefault(c => c?.IdCriptografico == cryptoId);

                // Buscar espacio por idApi
                var espacios = await _db.GetEspaciosAsync();
                Espacio? espacio = espacios.FirstOrDefault(e => e?.idApi == espacioId);

                if (cred != null && espacio != null)
                {
                    await ProcessAccessAsync(cred, espacio);
                }
                else
                {
                    string errorMsg = "";
                    if (cred == null) errorMsg += "Credencial no encontrada. ";
                    if (espacio == null) errorMsg += "Espacio no encontrado.";
                    
                    await DisplayAlert("Datos No Encontrados", errorMsg, "OK");
                    StatusLabel.Text = "? Credencial o espacio no válidos";
                    StatusLabel.TextColor = Colors.Red;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NFCReaderView] Error en HandleNFCDataAsync: {ex}");
                await DisplayAlert("Error", $"Error procesando datos NFC: {ex.Message}", "OK");
            }
        }

        private async Task ProcessByTagIdAsync(string? tagId)
        {
            if (string.IsNullOrEmpty(tagId))
            {
                await DisplayAlert("Error", "No se pudo procesar el tag NFC.", "OK");
                return;
            }

            // Buscar credencial que tenga este TagId almacenado
            var credenciales = await _db.GetCredencialesAsync();
            var cred = credenciales.FirstOrDefault(c => c?.IdCriptografico == tagId);

            if (cred != null)
            {
                // Si encontramos la credencial, permitir al funcionario seleccionar el espacio
                await DisplayAlert("Credencial Encontrada", 
                    $"Se detectó una credencial válida.\n\nPor favor, procede manualmente a verificar el acceso.", 
                    "OK");
                
                StatusLabel.Text = "? Credencial válida detectada";
                StatusLabel.TextColor = Colors.Green;
            }
            else
            {
                await DisplayAlert("Credencial No Registrada", 
                    "El tag NFC leído no está asociado a ninguna credencial registrada.", 
                    "OK");
                
                StatusLabel.Text = "? Tag NFC no registrado";
                StatusLabel.TextColor = Colors.Red;
            }
        }

        private async Task ProcessAccessAsync(Credencial credencial, Espacio espacio)
        {
            try
            {
                // Verificar estado de la credencial
                if (credencial.Estado != CredencialEstado.Activada)
                {
                    await DisplayAlert("Credencial Inactiva", 
                        $"La credencial está en estado: {credencial.Estado}", 
                        "OK");
                    
                    StatusLabel.Text = $"? Credencial {credencial.Estado}";
                    StatusLabel.TextColor = Colors.Red;
                    return;
                }

                // Por ahora, permitir acceso si la credencial está activa
                // TODO: Implementar verificación de reglas de acceso
                bool accesoPermitido = true;
                string razonDenegacion = "";

                // Registrar evento de acceso
                var eventoAcceso = new EventoAcceso
                {
                    CredencialId = credencial.CredencialId,
                    CredencialIdApi = credencial.idApi,
                    EspacioId = espacio.EspacioId,
                    EspacioIdApi = espacio.idApi,
                    MomentoDeAcceso = DateTime.Now,
                    Resultado = accesoPermitido ? AccesoTipo.Permitir : AccesoTipo.Denegar,
                    Motivo = accesoPermitido ? "Acceso mediante NFC" : $"Denegado: {razonDenegacion}",
                    Modo = Modo.Online
                };

                await _db.SaveEventoAccesoAsync(eventoAcceso);

                // Mostrar resultado
                if (accesoPermitido)
                {
                    await DisplayAlert("? Acceso Permitido", 
                        $"Credencial válida para:\n{espacio.Nombre}\n\nAcceso registrado correctamente.", 
                        "OK");
                    
                    StatusLabel.Text = "? Acceso concedido y registrado";
                    StatusLabel.TextColor = Colors.Green;

                    // Vibración de éxito
#if ANDROID || IOS
                    try
                    {
                        var vibration = Microsoft.Maui.Devices.Vibration.Default;
                        vibration.Vibrate(TimeSpan.FromMilliseconds(500));
                    }
                    catch { }
#endif
                }
                else
                {
                    await DisplayAlert("? Acceso Denegado", 
                        $"Acceso denegado a:\n{espacio.Nombre}\n\nMotivo: {razonDenegacion}", 
                        "OK");
                    
                    StatusLabel.Text = "? Acceso denegado";
                    StatusLabel.TextColor = Colors.Red;

                    // Vibración de error
#if ANDROID || IOS
                    try
                    {
                        var vibration = Microsoft.Maui.Devices.Vibration.Default;
                        vibration.Vibrate(TimeSpan.FromMilliseconds(100));
                        await Task.Delay(150);
                        vibration.Vibrate(TimeSpan.FromMilliseconds(100));
                    }
                    catch { }
#endif
                }

                // Limpiar para próxima lectura
                await Task.Delay(2000);
                TagInfoFrame.IsVisible = false;
                ProcessButton.IsVisible = false;
                _lastTagId = null;
                _lastTagData = null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NFCReaderView] Error en ProcessAccessAsync: {ex}");
                await DisplayAlert("Error", $"Error al procesar acceso: {ex.Message}", "OK");
            }
        }

        private async Task StartNfcAnimation()
        {
            try
            {
                while (_isReading)
                {
                    // Animación de ondas
                    _ = AnimateWave(WaveLabel1, 0);
                    await Task.Delay(300);
                    if (!_isReading) break;
                    
                    _ = AnimateWave(WaveLabel2, 300);
                    await Task.Delay(300);
                    if (!_isReading) break;
                    
                    _ = AnimateWave(WaveLabel3, 600);
                    await Task.Delay(700);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NFCReaderView] Error en animación: {ex}");
            }
        }

        private async Task AnimateWave(Label wave, int delay)
        {
            try
            {
                await Task.Delay(delay);
                
                // Reset position
                wave.TranslationX = -100;
                wave.Opacity = 0;

                // Animate
                await Task.WhenAll(
                    wave.TranslateTo(100, 0, 1000, Easing.Linear),
                    wave.FadeTo(1, 200),
                    Task.Delay(500).ContinueWith(_ => wave.FadeTo(0, 500))
                );
            }
            catch { }
        }

        private async Task StopNfcAnimation()
        {
            try
            {
                await Task.WhenAll(
                    WaveLabel1.FadeTo(0, 200),
                    WaveLabel2.FadeTo(0, 200),
                    WaveLabel3.FadeTo(0, 200)
                );
            }
            catch { }
        }
    }
}
