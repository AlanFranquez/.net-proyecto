using AppNetCredenciales.Data;
using AppNetCredenciales.models;
using AppNetCredenciales.Services;
using AppNetCredenciales.Views;
using Microsoft.Maui.ApplicationModel;

namespace AppNetCredenciales
{
    public partial class AppShell : Shell
    {
        private readonly ConnectivityService _connectivityService;
        private readonly LocalDBService _localDBService;
        private bool _offlineAlertShown;
        private bool _isSyncing = false;
        private readonly ApiService _apiService;

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
            Routing.RegisterRoute("readerSpaceSelection", typeof(AppNetCredenciales.Views.ReaderSpaceSelectionView));
            Routing.RegisterRoute("nfcReaderActive", typeof(AppNetCredenciales.Views.NfcReaderActiveView));

            _connectivityService = App.Services.GetRequiredService<ConnectivityService>();
            _localDBService = App.Services.GetRequiredService<LocalDBService>();
            _apiService = App.Services.GetRequiredService<ApiService>();
            _connectivityService.ConnectivityChanged += ConnectivityService_ConnectivityChanged;


            _ = syncDatos();

            
        }

        private async Task syncDatos()
        {
            try
            {
                await _localDBService.SincronizarUsuariosFromBack();
                await _localDBService.SincronizarRolesFromBack();
                await _localDBService.SincronizarEspaciosFromBack();
                await _localDBService.SincronizarCredencialesFromBack();

                System.Diagnostics.Debug.WriteLine("[AppShell] Sincronización inicial completada.");   
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AppShell] Error en sincronización inicial: {ex.Message}");
               
            }
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

                


                await SyncBeneficiosOfflineAsync(syncResults);
                await SyncEventosAccesoOfflineAsync(syncResults);


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
                System.Diagnostics.Debug.WriteLine("[AppShell] === SINCRONIZACIÓN DE EVENTOS DE ACCESO ===");

                // 1. Obtener eventos locales que NO tienen idApi (creados offline)
                var eventosLocales = await _localDBService.GetEventosAccesoAsync();
                var eventosPendientes = eventosLocales.Where(e => string.IsNullOrWhiteSpace(e.idApi) &&
                                                                !string.IsNullOrWhiteSpace(e.CredencialIdApi) &&
                                                                !string.IsNullOrWhiteSpace(e.EspacioIdApi))
                                                      .ToList();

                results.EventosTotal = eventosPendientes.Count;
                System.Diagnostics.Debug.WriteLine($"[AppShell] Eventos locales pendientes de sincronización: {results.EventosTotal}");

                foreach (var evento in eventosPendientes)
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine($"[AppShell] === PROCESANDO EVENTO ===");
                        System.Diagnostics.Debug.WriteLine($"[AppShell] EventoId local: {evento.EventoId}");
                        System.Diagnostics.Debug.WriteLine($"[AppShell] CredencialIdApi: {evento.CredencialIdApi}");
                        System.Diagnostics.Debug.WriteLine($"[AppShell] EspacioIdApi: {evento.EspacioIdApi}");
                        System.Diagnostics.Debug.WriteLine($"[AppShell] MomentoDeAcceso: {evento.MomentoDeAcceso}");
                        System.Diagnostics.Debug.WriteLine($"[AppShell] Resultado: {evento.Resultado}");

                        // 2. Intentar enviar el evento a la API
                        var eventoActualizado = await _localDBService.SaveAndPushEventoAccesoAsync(evento);

                        if (!string.IsNullOrWhiteSpace(eventoActualizado.idApi))
                        {
                            results.EventosSuccess++;
                            System.Diagnostics.Debug.WriteLine($"[AppShell] ✅ Evento sincronizado exitosamente: {eventoActualizado.idApi}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"[AppShell] ⚠️ Evento aún pendiente de sincronización: {evento.EventoId}");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[AppShell] ❌ Error sincronizando evento {evento.EventoId}: {ex.Message}");
                    }
                }

                System.Diagnostics.Debug.WriteLine($"[AppShell] === RESUMEN SINCRONIZACIÓN EVENTOS ===");
                System.Diagnostics.Debug.WriteLine($"[AppShell] Total eventos procesados: {results.EventosTotal}");
                System.Diagnostics.Debug.WriteLine($"[AppShell] Eventos sincronizados exitosamente: {results.EventosSuccess}");
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
         
                var beneficiosLocalesConCanje = await _localDBService.GetBeneficiosPendientesSyncAsync();

                System.Diagnostics.Debug.WriteLine($"[AppShell] Beneficios locales con canjes pendientes: {beneficiosLocalesConCanje.Count}");

                results.BeneficiosTotal = beneficiosLocalesConCanje.Count;

                foreach (var beneficioLocal in beneficiosLocalesConCanje)
                {
                    try
                    {
                        if (beneficioLocal.UsuariosIDs != null && beneficioLocal.UsuariosIDs.Length > 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"[AppShell] === PROCESANDO BENEFICIO: {beneficioLocal.Nombre} ===");
                            System.Diagnostics.Debug.WriteLine($"[AppShell] Usuarios con canjes pendientes: [{string.Join(", ", beneficioLocal.UsuariosIDs)}]");

                            foreach (var usuarioId in beneficioLocal.UsuariosIDs)
                            {
                                if (!string.IsNullOrWhiteSpace(usuarioId))
                                {
                                    try
                                    {
                                        
                                        
                                        var resultado = await _localDBService.CanjearBeneficio(usuarioId, beneficioLocal.idApi);

                                        if (resultado != null && !resultado.FaltaCarga)
                                        {
                                            System.Diagnostics.Debug.WriteLine($"[AppShell] ✅ Canje sincronizado exitosamente: usuario {usuarioId} -> beneficio {beneficioLocal.Nombre}");
                                            results.BeneficiosSuccess++;
                                        }
                                        else
                                        {
                                            System.Diagnostics.Debug.WriteLine($"[AppShell] ⚠️ Canje aún pendiente para usuario {usuarioId}");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"[AppShell] ❌ Error sincronizando canje usuario {usuarioId}: {ex.Message}");
                                    }
                                }
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"[AppShell] Beneficio {beneficioLocal.Nombre} no tiene usuarios para sincronizar");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[AppShell] ❌ Error procesando beneficio {beneficioLocal.Nombre}: {ex.Message}");
                    }
                }

                System.Diagnostics.Debug.WriteLine($"[AppShell] === RESUMEN SINCRONIZACIÓN BENEFICIOS ===");
                System.Diagnostics.Debug.WriteLine($"[AppShell] Total beneficios procesados: {results.BeneficiosTotal}");
                System.Diagnostics.Debug.WriteLine($"[AppShell] Canjes sincronizados exitosamente: {results.BeneficiosSuccess}");
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