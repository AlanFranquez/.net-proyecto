using AppNetCredenciales.Views;
using Microsoft.Maui.ApplicationModel;
using AppNetCredenciales.Services;
using AppNetCredenciales.Data;
using AppNetCredenciales.models;

namespace AppNetCredenciales
{
    public partial class AppShell : Shell
    {
        private readonly ConnectivityService _connectivityService;
        private readonly LocalDBService _localDBService;
        private bool _offlineAlertShown;
        private bool _isSyncing = false;

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
            _localDBService = App.Services.GetRequiredService<LocalDBService>();
            _connectivityService.ConnectivityChanged += ConnectivityService_ConnectivityChanged;
        }

        private void ConnectivityService_ConnectivityChanged(object? sender, bool isConnected)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (!isConnected && !_offlineAlertShown)
                {
                    _offlineAlertShown = true;
                    await Current.DisplayAlert("Sin conexión",
                        "No hay conexión a Internet. Los datos se guardarán localmente.", "OK");
                }
                else if (isConnected && !_isSyncing)
                {
                    _offlineAlertShown = false;
                    await SyncOfflineDataAsync();
                }
            });
        }

        private async Task SyncOfflineDataAsync()
        {
            try
            {
                _isSyncing = true;

                System.Diagnostics.Debug.WriteLine("[AppShell] === INICIANDO SINCRONIZACIÓN OFFLINE ===");

                var syncResults = new SyncResults();

                await SyncUsuariosOfflineAsync(syncResults);

                await SyncEventosAccesoOfflineAsync(syncResults);

                await SyncBeneficiosOfflineAsync(syncResults);

                await ShowSyncResultsAsync(syncResults);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AppShell] Error en sincronización: {ex.Message}");
                await Current.DisplayAlert("Error", "Error al sincronizar datos offline.", "OK");
            }
            finally
            {
                _isSyncing = false;
            }
        }

        private async Task SyncUsuariosOfflineAsync(SyncResults results)
        {
            try
            {
                var usuariosPendientes = await _localDBService.GetUsuariosPendientesSyncAsync();
                results.UsuariosTotal = usuariosPendientes.Count;

                System.Diagnostics.Debug.WriteLine($"[AppShell] === SINCRONIZACIÓN DE USUARIOS ===");
                System.Diagnostics.Debug.WriteLine($"[AppShell] Total usuarios pendientes: {results.UsuariosTotal}");

                foreach (var usuario in usuariosPendientes)
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine($"[AppShell] === PROCESANDO USUARIO ===");
                        System.Diagnostics.Debug.WriteLine($"[AppShell] Email: {usuario.Email}");
                        System.Diagnostics.Debug.WriteLine($"[AppShell] idApi actual: '{usuario.idApi}'");
                        System.Diagnostics.Debug.WriteLine($"[AppShell] FaltaCargar: {usuario.FaltaCargar}");

                        var apiId = await _localDBService.CreateUsuarioRemoteAsync(usuario);

                        System.Diagnostics.Debug.WriteLine($"[AppShell] === RESULTADO CREATE USUARIO ===");
                        System.Diagnostics.Debug.WriteLine($"[AppShell] API ID devuelto: '{apiId}'");

                        if (!string.IsNullOrWhiteSpace(apiId))
                        {
                            results.UsuariosSuccess++;
                            System.Diagnostics.Debug.WriteLine($"[AppShell] ✅ Usuario creado exitosamente: {usuario.Email} -> API ID: {apiId}");

                            // Actualizar credenciales relacionadas
                            var credenciales = await _localDBService.GetCredencialesAsync();
                            System.Diagnostics.Debug.WriteLine($"[AppShell] Verificando {credenciales.Count} credenciales para actualizar...");

                            foreach (var c in credenciales)
                            {
                                if (string.IsNullOrWhiteSpace(c.usuarioIdApi) || c.usuarioIdApi == usuario.Email)
                                {
                                    System.Diagnostics.Debug.WriteLine($"[AppShell] Actualizando credencial {c.CredencialId}: {c.usuarioIdApi} -> {apiId}");
                                    c.usuarioIdApi = apiId;
                                    await _localDBService.SaveCredencialAsyncApi(c);
                                }
                            }

                            await ActualizarRolesConNuevoUsuario(usuario, apiId);
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"[AppShell] ❌ Error: No se obtuvo API ID para usuario {usuario.Email}");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[AppShell] ❌ Excepción usuario {usuario.Email}:");
                        System.Diagnostics.Debug.WriteLine($"[AppShell] Mensaje: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"[AppShell] StackTrace: {ex.StackTrace}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AppShell] Error en SyncUsuariosOfflineAsync: {ex.Message}");
            }
        }

        private async Task ActualizarRolesConNuevoUsuario(Usuario usuario, string apiId)
        {
            try
            {
                if (usuario.RolesIDs == null || usuario.RolesIDs.Length == 0)
                {
                    System.Diagnostics.Debug.WriteLine($"[AppShell] Usuario {usuario.Email} no tiene roles asignados");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"[AppShell] Actualizando roles para usuario {usuario.Email} con API ID {apiId}");

                foreach (var rolId in usuario.RolesIDs)
                {
                    if (string.IsNullOrWhiteSpace(rolId)) continue;

                    try
                    {
                        await _localDBService.AgregarUsuarioARol(rolId, apiId);
                        System.Diagnostics.Debug.WriteLine($"[AppShell] ✅ Usuario {apiId} agregado al rol {rolId}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[AppShell] ❌ Error agregando usuario {apiId} al rol {rolId}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AppShell] Error en ActualizarRolesConNuevoUsuario: {ex.Message}");
            }
        }

        private async Task SyncEventosAccesoOfflineAsync(SyncResults results)
        {
            try
            {
                var eventosPendientes = await _localDBService.GetEventosAccesoPendientesSyncAsync();
                results.EventosTotal = eventosPendientes.Count;

                System.Diagnostics.Debug.WriteLine($"[AppShell] Sincronizando {results.EventosTotal} eventos de acceso...");

                foreach (var evento in eventosPendientes)
                {
                    try
                    {
                        var eventoActualizado = await _localDBService.SaveAndPushEventoAccesoAsync(evento);

                        if (!string.IsNullOrWhiteSpace(eventoActualizado.idApi))
                        {
                            results.EventosSuccess++;
                            System.Diagnostics.Debug.WriteLine($"[AppShell] Evento sincronizado: {eventoActualizado.idApi}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"[AppShell] Error sincronizando evento: {evento.EventoId}");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[AppShell] Excepción evento {evento.EventoId}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AppShell] Error en SyncEventosAccesoOfflineAsync: {ex.Message}");
            }
        }

        private async Task SyncBeneficiosOfflineAsync(SyncResults results)
        {
            try
            {
                var beneficiosPendientes = await _localDBService.GetBeneficiosPendientesSyncAsync();
                results.BeneficiosTotal = beneficiosPendientes.Count;

                System.Diagnostics.Debug.WriteLine($"[AppShell] Sincronizando {results.BeneficiosTotal} beneficios...");

                var beneficiosEnApi = await _localDBService.GetBeneficiosAsync();

                foreach (var beneficio in beneficiosPendientes)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(beneficio.idApi))
                        {
                            continue;
                        }

                        var existeEnApi = beneficiosEnApi?.Any(b => b.idApi == beneficio.idApi) ?? false;

                        if (existeEnApi)
                        {
                            System.Diagnostics.Debug.WriteLine($"[AppShell] Actualizando beneficio existente: {beneficio.idApi}");

                            var beneficioDto = new ApiService.BeneficioDto
                            {
                                Id = beneficio.idApi,
                                Descripcion = beneficio.Descripcion ?? string.Empty,
                                Tipo = beneficio.Tipo ?? string.Empty,
                                Nombre = beneficio.Nombre ?? string.Empty,
                                VigenciaInicio = beneficio.VigenciaInicio,
                                VigenciaFin = beneficio.VigenciaFin,
                                CupoTotal = beneficio.CupoTotal,
                                CupoPorUsuario = beneficio.CupoPorUsuario,
                                RequiereBiometria = beneficio.RequiereBiometria,
                                EspaciosIDs = beneficio.EspaciosIDs ?? Array.Empty<string>(),
                                UsuariosIDs = beneficio.UsuariosIDs ?? Array.Empty<string>()
                            };

                            var beneficioActualizado = await _localDBService.updateBeneficio(beneficioDto);

                            if (beneficioActualizado != null)
                            {
                                results.BeneficiosSuccess++;
                                System.Diagnostics.Debug.WriteLine($"[AppShell] ✅ Beneficio actualizado exitosamente: {beneficio.idApi}");
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"[AppShell] ❌ Error actualizando beneficio: {beneficio.idApi}");
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"[AppShell] Creando nuevo beneficio en API: {beneficio.idApi}");

                            var nuevoBeneficioCreado = await _localDBService.CrearBeneficioEnApiAsync(beneficio);

                            if (nuevoBeneficioCreado)
                            {
                                results.BeneficiosSuccess++;
                                System.Diagnostics.Debug.WriteLine($"[AppShell] ✅ Beneficio creado exitosamente: {beneficio.idApi}");
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"[AppShell] ❌ Error creando beneficio: {beneficio.idApi}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[AppShell] ❌ Excepción beneficio {beneficio?.idApi ?? "unknown"}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AppShell] Error en SyncBeneficiosOfflineAsync: {ex.Message}");
            }
        }

        private async Task ShowSyncResultsAsync(SyncResults results)
        {
            if (results.TotalItems == 0)
            {
                await Current.DisplayAlert("Conectado",
                    "Se recuperó la conexión. No había datos pendientes por sincronizar.", "OK");
                return;
            }

            var message = "Conectado. Sincronización completada:\n\n";

            if (results.UsuariosTotal > 0)
                message += $"Usuarios: {results.UsuariosSuccess}/{results.UsuariosTotal}\n";

            if (results.EventosTotal > 0)
                message += $"Eventos de acceso: {results.EventosSuccess}/{results.EventosTotal}\n";

            if (results.BeneficiosTotal > 0)
                message += $"Beneficios: {results.BeneficiosSuccess}/{results.BeneficiosTotal}\n";

            var title = results.IsFullSuccess ? "Sincronizacion exitosa" : "ADVERTENCIA: Sincronizacion parcial";

            await Current.DisplayAlert(title, message, "OK");
        }

        private class SyncResults
        {
            public int UsuariosTotal { get; set; }
            public int UsuariosSuccess { get; set; }
            public int EventosTotal { get; set; }
            public int EventosSuccess { get; set; }
            public int BeneficiosTotal { get; set; }
            public int BeneficiosSuccess { get; set; }

            public int TotalItems => UsuariosTotal + EventosTotal + BeneficiosTotal;
            public int TotalSuccess => UsuariosSuccess + EventosSuccess + BeneficiosSuccess;
            public bool IsFullSuccess => TotalItems > 0 && TotalItems == TotalSuccess;
        }
    }
}