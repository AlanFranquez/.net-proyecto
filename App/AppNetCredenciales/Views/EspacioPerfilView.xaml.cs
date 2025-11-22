using AppNetCredenciales.Data;
using AppNetCredenciales.models;
using AppNetCredenciales.Services;
using CommunityToolkit.Maui.Views;
using System.Diagnostics;

namespace AppNetCredenciales.Views;

[QueryProperty(nameof(EspacioId), "espacioId")]
public partial class EspacioPerfilView : ContentPage
{
    private string _espacioId;
    private readonly LocalDBService _db;
    private readonly ConnectivityService connectiviyService = new ConnectivityService();
    private readonly BiometricService _biometricService;
    private Espacio _currentEspacio;

    public EspacioPerfilView() : this(
        MauiProgram.ServiceProvider?.GetService<LocalDBService>() ?? throw new InvalidOperationException("LocalDBService not registered"),
        MauiProgram.ServiceProvider?.GetService<BiometricService>() ?? new BiometricService())
    { }

    public EspacioPerfilView(LocalDBService db, BiometricService biometricService)
    {
        this._db = db ?? throw new ArgumentNullException(nameof(db));
        this._biometricService = biometricService ?? throw new ArgumentNullException(nameof(biometricService));
        InitializeComponent();
    }

    public string EspacioId
    {
        get => _espacioId;
        set
        {
            _espacioId = value;
            _ = CargarEspacioAsync(_espacioId);
        }
    }

    private async Task CargarEspacioAsync(string id)
    {
        try
        {
            if (id == null)
            {
                Debug.WriteLine($"[EspacioPerfil] Invalid id: {id}");
                return;
            }

            Debug.WriteLine($"[EspacioPerfil] Loading Espacio with id: {id}");

            var espacios = await this._db.GetEspaciosAsync();

            Debug.WriteLine($"[EspacioPerfil] Total espacios found: {espacios?.Count ?? 0}");

            foreach (var e in espacios)
            {
                Debug.WriteLine($"[EspacioPerfil] Found Espacio: idApi={e.idApi}, Nombre='{e.Nombre}', Tipo={e.Tipo}, Activo={e.Activo}, EspacioId={e.EspacioId}");
            }

            var espacio = espacios.FirstOrDefault(e => e.idApi == id);

            if (espacio == null)
            {
                Debug.WriteLine($"[EspacioPerfil] Espacio not found for id {id}");
                await DisplayAlert("Error", $"Espacio con ID {id} no encontrado.", "OK");
                return;
            }

            if (string.IsNullOrEmpty(espacio.Nombre) || espacio.Nombre == "string")
            {
                Debug.WriteLine($"[EspacioPerfil] WARNING: Espacio has invalid or corrupted name data!");

                if (connectiviyService.IsConnected)
                {
                    Debug.WriteLine($"[EspacioPerfil] Attempting to refresh espacios from API...");
                    try
                    {
                        var refreshedEspacios = await this._db.SincronizarEspaciosFromBack();
                        Debug.WriteLine($"[EspacioPerfil] Refreshed {refreshedEspacios?.Count ?? 0} espacios from API");

                        espacio = refreshedEspacios.FirstOrDefault(e => e.idApi == id);
                        
                    }
                    catch (Exception refreshEx)
                    {
                        Debug.WriteLine($"[EspacioPerfil] Failed to refresh from API: {refreshEx.Message}");
                    }
                }
            }

            _currentEspacio = espacio;
            BindingContext = espacio;

            Debug.WriteLine($"[EspacioPerfil] BindingContext set successfully");

            OnPropertyChanged(nameof(BindingContext));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[EspacioPerfil] CargarEspacioAsync error: {ex}");
            await DisplayAlert("Error", $"Ocurrió un error cargando el espacio: {ex.Message}", "OK");
        }
    }

    /// <summary>
    /// Handles access to space with biometric authentication
    /// </summary>
    private async void OnAccessSpaceClicked(object sender, EventArgs e)
    {
        if (_currentEspacio == null)
        {
            await DisplayAlert("Error", "No se ha cargado la información del espacio.", "OK");
            return;
        }

        try
        {
            // Step 1: Ask user confirmation
            bool userConfirmed = await DisplayAlert(
                "Acceso con Autenticación Biométrica",
                $"Para acceder a '{_currentEspacio.Nombre}' debes verificar tu identidad con huella digital.\n\n¿Deseas continuar?",
                "Autenticar",
                "Cancelar");

            if (!userConfirmed)
            {
                return;
            }

            // Step 2: Perform biometric authentication
            var biometricResult = await _biometricService.AuthenticateAsync(
                $"Verificar tu identidad para acceder a {_currentEspacio.Nombre}");

            if (!biometricResult.Success)
            {
                await DisplayAlert(
                    "Autenticación Fallida",
                    biometricResult.ErrorMessage ?? "No se pudo verificar tu identidad.",
                    "OK");
                return;
            }

            // Step 3: Get user credential
            var usuario = await _db.GetLoggedUserAsync();
            if (usuario == null)
            {
                await DisplayAlert("Error", "No hay usuario logueado.", "OK");
                return;
            }

            var credencial = await _db.GetLoggedUserCredential();
            if (credencial == null)
            {
                await DisplayAlert("Sin Credencial", "No tienes una credencial válida para acceder.", "OK");
                
                // Register denied access
                await RegistrarAccesoAsync(usuario, _currentEspacio, AccesoTipo.Denegar, "Usuario sin credencial");
                return;
            }

            // Step 4: Validate active credential
            if (credencial.FechaExpiracion.HasValue && credencial.FechaExpiracion.Value < DateTime.Now)
            {
                await DisplayAlert("Credencial Expirada", "Tu credencial ha expirado. Por favor, renuévala.", "OK");
                await RegistrarAccesoAsync(usuario, _currentEspacio, AccesoTipo.Denegar, "Credencial expirada");
                return;
            }

            // Step 5: Show success and register access
            var successPopup = new ScanResultPopup(
                "Acceso Permitido",
                $"Bienvenido a {_currentEspacio.Nombre}",
                true);
            
            await this.ShowPopupAsync(successPopup);

            // Register successful access
            await RegistrarAccesoAsync(usuario, _currentEspacio, AccesoTipo.Permitir, "Acceso autorizado con autenticación biométrica");

            Debug.WriteLine($"[EspacioPerfil] Access granted to {_currentEspacio.Nombre} for user {usuario.Email}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[EspacioPerfil] OnAccessSpaceClicked error: {ex}");
            await DisplayAlert("Error", $"Ocurrió un error: {ex.Message}", "OK");
        }
    }

    /// <summary>
    /// Registers an access event in the database
    /// </summary>
    private async Task RegistrarAccesoAsync(Usuario usuario, Espacio espacio, AccesoTipo resultado, string motivo)
    {
        try
        {
            var evento = new EventoAcceso
            {
                MomentoDeAcceso = DateTime.Now,
                CredencialId = usuario.CredencialId,
                Credencial = usuario.Credencial,
                Espacio = espacio,
                EspacioId = espacio.EspacioId,
                EspacioIdApi = espacio.idApi,
                Resultado = resultado,
                Motivo = motivo
            };

            await _db.SaveAndPushEventoAccesoAsync(evento);
            Debug.WriteLine($"[EspacioPerfil] Access event registered: {resultado} - {motivo}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[EspacioPerfil] Error registering access event: {ex}");
        }
    }

    private async void OnShowQRClicked(object sender, EventArgs e)
    {
        try
        {
            //Debug.WriteLine("[EspacioPerfil] OnShowQRClicked - Starting...");

            //// PASO 1: Solicitar confirmación para autenticación biométrica
            //bool userConfirmed = await DisplayAlert(
            //    "Verificación de Identidad",
            //    "Debes verificar tu identidad con huella digital para mostrar tu código QR.\n\n¿Deseas continuar?",
            //    "Autenticar",
            //    "Cancelar");

            //if (!userConfirmed)
            //{
            //    Debug.WriteLine("[EspacioPerfil] User cancelled biometric authentication");
            //    return;
            //}

            //// PASO 2: Realizar autenticación biométrica
            //var biometricResult = await _biometricService.AuthenticateAsync(
            //    "Verificar tu identidad para mostrar tu código QR");

            //if (!biometricResult.Success)
            //{
            //    Debug.WriteLine($"[EspacioPerfil] Biometric authentication failed: {biometricResult.ErrorMessage}");
            //    await DisplayAlert(
            //        "Autenticación Fallida",
            //        biometricResult.ErrorMessage ?? "No se pudo verificar tu identidad.",
            //        "OK");
            //    return;
            //}

            //Debug.WriteLine("[EspacioPerfil] Biometric authentication successful");

            // PASO 3: Obtener usuario logueado
            var usuario = await _db.GetLoggedUserAsync();
            if (usuario == null)
            {
                Debug.WriteLine("[EspacioPerfil] No logged user found");
                await DisplayAlert("Error", "No hay usuario logueado.", "OK");
                return;
            }

            Debug.WriteLine($"[EspacioPerfil] Logged user: {usuario.Email}, idApi: {usuario.idApi}");

            // PASO 4: Buscar credencial del usuario
            Credencial cred = null;
            var getAllCredenciales = await _db.GetCredencialesAsync();
            
            Debug.WriteLine($"[EspacioPerfil] Total credenciales found: {getAllCredenciales?.Count ?? 0}");

            foreach (var a in getAllCredenciales)
            {
                Debug.WriteLine($"[EspacioPerfil] Checking credencial: {a.CredencialId}, usuarioIdApi: {a.usuarioIdApi}, IdCripto: {a.IdCriptografico}");
                
                if (a.usuarioIdApi == usuario.idApi)
                {
                    cred = a;
                    Debug.WriteLine($"[EspacioPerfil] Found matching credencial!");
                    break;
                }
            }

            if (cred == null)
            {
                Debug.WriteLine("[EspacioPerfil] No credential found for user");
                await DisplayAlert("Sin Credencial", 
                    "No tienes una credencial asignada. Por favor, solicita una credencial primero.", 
                    "OK");
                return;
            }

            // PASO 5: Validar credencial activa y no expirada
            if (cred.FechaExpiracion.HasValue && cred.FechaExpiracion.Value < DateTime.Now)
            {
                Debug.WriteLine("[EspacioPerfil] Credential expired");
                await DisplayAlert("Credencial Expirada", 
                    "Tu credencial ha expirado. Por favor, renuévala antes de continuar.", 
                    "OK");
                return;
            }

            // PASO 6: Obtener espacio
            var espacio = BindingContext as Espacio;
            if (espacio == null)
            {
                Debug.WriteLine("[EspacioPerfil] Espacio is null in BindingContext");
                await DisplayAlert("Error", "No se ha cargado la información del espacio.", "OK");
                return;
            }

            Debug.WriteLine($"[EspacioPerfil] Espacio: {espacio.Nombre}, idApi: {espacio.idApi}");

            if (string.IsNullOrEmpty(cred.IdCriptografico))
            {
                Debug.WriteLine("[EspacioPerfil] IdCriptografico is empty");
                await DisplayAlert("Error", "La credencial no tiene un ID criptográfico válido.", "OK");
                return;
            }

            if (string.IsNullOrEmpty(espacio.idApi))
            {
                Debug.WriteLine("[EspacioPerfil] Espacio idApi is empty");
                await DisplayAlert("Error", "El espacio no tiene un ID válido.", "OK");
                return;
            }

            // PASO 7: Generar y mostrar QR
            string qrData = $"{cred.IdCriptografico}|{espacio.idApi}";
            Debug.WriteLine($"[EspacioPerfil] Generating QR with data: {qrData}");

            var modal = new QRModalPage(qrData);
            await Navigation.PushModalAsync(modal);
            
            Debug.WriteLine("[EspacioPerfil] QR Modal opened successfully");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[EspacioPerfil] OnShowQRClicked error: {ex}");
            await DisplayAlert("Error", $"Ocurrió un error al mostrar el QR: {ex.Message}", "OK");
        }
    }
}