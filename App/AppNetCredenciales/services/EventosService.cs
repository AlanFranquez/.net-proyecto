using AppNetCredenciales.Data;
using AppNetCredenciales.models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AppNetCredenciales.Services
{
    /// <summary>
    /// Servicio para gestión de eventos de acceso mediante NFC
    /// </summary>
    public class EventosService : IEventosService
    {
        private readonly LocalDBService _db;
        private readonly ApiService _apiService;
        private readonly ConnectivityService _connectivityService;

        public EventosService(LocalDBService db, ApiService apiService, ConnectivityService connectivityService)
        {
            _db = db;
            _apiService = apiService;
            _connectivityService = connectivityService;
        }

        /// <summary>
        /// Valida una credencial y registra el evento de acceso
        /// </summary>
        public async Task<EventoAccesoResult> ValidarYRegistrarAcceso(string idCriptografico, int espacioId)
        {
            try
            {
                Debug.WriteLine($"???????????????????????????????????????????");
                Debug.WriteLine($"[EventosService] ?? VALIDANDO ACCESO");
                Debug.WriteLine($"[EventosService] IdCriptografico recibido: '{idCriptografico}'");
                Debug.WriteLine($"[EventosService] Longitud: {idCriptografico?.Length ?? 0} caracteres");
                Debug.WriteLine($"[EventosService] EspacioId: {espacioId}");
                Debug.WriteLine($"???????????????????????????????????????????");

                // 1. Buscar la credencial por IdCriptografico
                var credencial = await _db.GetCredencialByIdCriptograficoAsync(idCriptografico);
                
                if (credencial == null)
                {
                    Debug.WriteLine($"[EventosService] ? CREDENCIAL NO ENCONTRADA");
                    Debug.WriteLine($"[EventosService] Se buscó: '{idCriptografico}'");
                    
                    // Listar todas las credenciales para debug
                    var todasCredenciales = await _db.GetCredencialesAsync();
                    Debug.WriteLine($"[EventosService] Total credenciales en BD: {todasCredenciales.Count}");
                    
                    if (todasCredenciales.Count > 0)
                    {
                        Debug.WriteLine($"[EventosService] === LISTADO DE CREDENCIALES EN BD ===");
                        foreach (var c in todasCredenciales)
                        {
                            Debug.WriteLine($"[EventosService]   - ID: {c.CredencialId}");
                            Debug.WriteLine($"[EventosService]     IdCripto: '{c.IdCriptografico ?? "NULL"}'");
                            Debug.WriteLine($"[EventosService]     Estado: {c.Estado}");
                            Debug.WriteLine($"[EventosService]     Usuario: {c.usuarioIdApi}");
                            Debug.WriteLine($"[EventosService]     ---");
                        }
                        Debug.WriteLine($"[EventosService] ===================================");
                    }
                    else
                    {
                        Debug.WriteLine($"[EventosService] ?? NO HAY CREDENCIALES EN LA BD LOCAL");
                        Debug.WriteLine($"[EventosService] ?? ¿Se ha sincronizado la base de datos?");
                    }
                    
                    var eventoNoEncontrada = await RegistrarEventoNoEncontrada(idCriptografico, espacioId);
                    
                    return new EventoAccesoResult
                    {
                        AccesoConcedido = false,
                        Motivo = "Credencial no encontrada en el sistema",
                        Evento = eventoNoEncontrada,
                        NombreCompleto = "Credencial no registrada",
                        Documento = idCriptografico.Length > 10 ? idCriptografico.Substring(0, 10) + "..." : idCriptografico
                    };
                }

                Debug.WriteLine($"[EventosService] ? Credencial ENCONTRADA:");
                Debug.WriteLine($"[EventosService]   - ID: {credencial.CredencialId}");
                Debug.WriteLine($"[EventosService]   - Estado: {credencial.Estado}");
                Debug.WriteLine($"[EventosService]   - IdCriptografico: '{credencial.IdCriptografico}'");
                Debug.WriteLine($"[EventosService]   - FechaExpiracion: {credencial.FechaExpiracion}");
                Debug.WriteLine($"[EventosService]   - Usuario: {credencial.usuarioIdApi}");
                Debug.WriteLine($"[EventosService]   - Tipo: {credencial.Tipo}");
                Debug.WriteLine($"???????????????????????????????????????????");

                // 2. Obtener el usuario de la credencial
                var usuario = await _db.GetUsuarioByIdApiAsync(credencial.usuarioIdApi);
                
                if (usuario == null)
                {
                    Debug.WriteLine("[EventosService] Usuario no encontrado");
                    var eventoUsuarioNoEncontrado = await RegistrarEventoDenegado(credencial, espacioId, "Usuario no encontrado");
                    
                    return new EventoAccesoResult
                    {
                        AccesoConcedido = false,
                        Motivo = "Usuario no encontrado",
                        Credencial = credencial,
                        Evento = eventoUsuarioNoEncontrado
                    };
                }

                Debug.WriteLine($"[EventosService] Usuario encontrado: {usuario.Nombre} {usuario.Apellido}");

                // 3. Validar estado de la credencial
                if (credencial.Estado != CredencialEstado.Activada)
                {
                    Debug.WriteLine($"[EventosService] Credencial no activada: {credencial.Estado}");
                    var eventoDenegado = await RegistrarEventoDenegado(credencial, espacioId, $"Credencial en estado: {credencial.Estado}");
                    
                    return new EventoAccesoResult
                    {
                        AccesoConcedido = false,
                        Motivo = $"Credencial {credencial.Estado}",
                        Credencial = credencial,
                        Usuario = usuario,
                        Evento = eventoDenegado,
                        NombreCompleto = $"{usuario.Nombre} {usuario.Apellido}",
                        Documento = usuario.Documento
                    };
                }

                // 4. Verificar si la credencial está expirada
                if (credencial.FechaExpiracion.HasValue && credencial.FechaExpiracion.Value < DateTime.Now)
                {
                    Debug.WriteLine("[EventosService] Credencial expirada");
                    var eventoDenegado = await RegistrarEventoDenegado(credencial, espacioId, "Credencial expirada");
                    
                    return new EventoAccesoResult
                    {
                        AccesoConcedido = false,
                        Motivo = "Credencial expirada",
                        Credencial = credencial,
                        Usuario = usuario,
                        Evento = eventoDenegado,
                        NombreCompleto = $"{usuario.Nombre} {usuario.Apellido}",
                        Documento = usuario.Documento
                    };
                }

                // 5. Obtener el espacio
                var espacio = await _db.GetEspacioByIdAsync(espacioId);
                
                if (espacio == null)
                {
                    Debug.WriteLine("[EventosService] Espacio no encontrado");
                    var eventoDenegado = await RegistrarEventoDenegado(credencial, espacioId, "Espacio no encontrado");
                    
                    return new EventoAccesoResult
                    {
                        AccesoConcedido = false,
                        Motivo = "Espacio no encontrado",
                        Credencial = credencial,
                        Usuario = usuario,
                        Evento = eventoDenegado,
                        NombreCompleto = $"{usuario.Nombre} {usuario.Apellido}",
                        Documento = usuario.Documento
                    };
                }

                // 6. Validar permisos (verificar reglas de acceso)
                var tienePermiso = await ValidarPermisosAcceso(credencial, espacio);
                
                if (!tienePermiso)
                {
                    Debug.WriteLine("[EventosService] Sin permisos para este espacio");
                    var eventoDenegado = await RegistrarEventoDenegado(credencial, espacioId, "Sin permisos para este espacio");
                    
                    return new EventoAccesoResult
                    {
                        AccesoConcedido = false,
                        Motivo = "Sin permisos de acceso",
                        Credencial = credencial,
                        Usuario = usuario,
                        Evento = eventoDenegado,
                        NombreCompleto = $"{usuario.Nombre} {usuario.Apellido}",
                        Documento = usuario.Documento
                    };
                }

                // 7. TODO VÁLIDO - Registrar evento de ingreso exitoso
                Debug.WriteLine("[EventosService] Acceso concedido");
                var eventoExitoso = await RegistrarEventoExitoso(credencial, espacioId);
                
                return new EventoAccesoResult
                {
                    AccesoConcedido = true,
                    Motivo = "Acceso concedido",
                    Credencial = credencial,
                    Usuario = usuario,
                    Evento = eventoExitoso,
                    NombreCompleto = $"{usuario.Nombre} {usuario.Apellido}",
                    Documento = usuario.Documento
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[EventosService] Error validando acceso: {ex.Message}");
                return new EventoAccesoResult
                {
                    AccesoConcedido = false,
                    Motivo = $"Error del sistema: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Valida si la credencial tiene permisos para acceder al espacio
        /// </summary>
        private async Task<bool> ValidarPermisosAcceso(Credencial credencial, Espacio espacio)
        {
            try
            {
                // Obtener las reglas de acceso del espacio
                var reglasEspacio = await _db.GetReglasDeAccesoByEspacioIdAsync(espacio.EspacioId);
                
                if (reglasEspacio == null || !reglasEspacio.Any())
                {
                    // Si no hay reglas definidas, permitir el acceso por defecto
                    Debug.WriteLine("[EventosService] No hay reglas definidas, acceso permitido por defecto");
                    return true;
                }

                // Obtener el usuario de la credencial
                var usuario = await _db.GetUsuarioByIdApiAsync(credencial.usuarioIdApi);
                if (usuario == null) return false;

                // Verificar cada regla
                foreach (var reglaEspacio in reglasEspacio)
                {
                    var regla = await _db.GetReglaDeAccesoByIdAsync(reglaEspacio.ReglaId);
                    if (regla == null) continue;

                    // Validar según el tipo de credencial requerido
                    if (!string.IsNullOrEmpty(regla.TipoCredencialRequerida))
                    {
                        if (credencial.Tipo.ToString() == regla.TipoCredencialRequerida)
                        {
                            Debug.WriteLine($"[EventosService] Regla cumplida: TipoCredencial={credencial.Tipo}");
                            return true;
                        }
                    }

                    // Validar según roles del usuario
                    if (!string.IsNullOrEmpty(regla.RolRequerido))
                    {
                        var rolesUsuario = usuario.RolesIDs;
                        if (rolesUsuario != null && rolesUsuario.Contains(regla.RolRequerido))
                        {
                            Debug.WriteLine($"[EventosService] Regla cumplida: Rol={regla.RolRequerido}");
                            return true;
                        }
                    }
                }

                // Si no cumple ninguna regla
                Debug.WriteLine("[EventosService] No cumple ninguna regla de acceso");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[EventosService] Error validando permisos: {ex.Message}");
                // En caso de error, denegar por seguridad
                return false;
            }
        }

        /// <summary>
        /// Registra un evento de acceso exitoso
        /// </summary>
        private async Task<EventoAcceso> RegistrarEventoExitoso(Credencial credencial, int espacioId)
        {
            var espacio = await _db.GetEspacioByIdAsync(espacioId);
            
            var evento = new EventoAcceso
            {
                MomentoDeAcceso = DateTime.Now,
                CredencialId = credencial.CredencialId,
                CredencialIdApi = credencial.idApi,
                EspacioId = espacioId,
                EspacioIdApi = espacio?.idApi,
                Resultado = AccesoTipo.Permitir,
                Motivo = "Acceso concedido",
                Modo = await _connectivityService.IsConnectedAsync() ? Modo.Online : Modo.Offline
            };

            await _db.SaveEventoAccesoAsync(evento);
            Debug.WriteLine($"[EventosService] Evento exitoso registrado: ID={evento.EventoId}");

            // Intentar sincronizar con la API si hay conexión
            await TrySincronizarEvento(evento);

            return evento;
        }

        /// <summary>
        /// Registra un evento de acceso denegado
        /// </summary>
        private async Task<EventoAcceso> RegistrarEventoDenegado(Credencial credencial, int espacioId, string motivo)
        {
            var espacio = await _db.GetEspacioByIdAsync(espacioId);
            
            var evento = new EventoAcceso
            {
                MomentoDeAcceso = DateTime.Now,
                CredencialId = credencial.CredencialId,
                CredencialIdApi = credencial.idApi,
                EspacioId = espacioId,
                EspacioIdApi = espacio?.idApi,
                Resultado = AccesoTipo.Denegar,
                Motivo = motivo,
                Modo = await _connectivityService.IsConnectedAsync() ? Modo.Online : Modo.Offline
            };

            await _db.SaveEventoAccesoAsync(evento);
            Debug.WriteLine($"[EventosService] Evento denegado registrado: ID={evento.EventoId}, Motivo={motivo}");

            await TrySincronizarEvento(evento);

            return evento;
        }

        /// <summary>
        /// Registra un evento cuando la credencial no se encuentra
        /// </summary>
        private async Task<EventoAcceso> RegistrarEventoNoEncontrada(string idCriptografico, int espacioId)
        {
            var espacio = await _db.GetEspacioByIdAsync(espacioId);
            
            var evento = new EventoAcceso
            {
                MomentoDeAcceso = DateTime.Now,
                EspacioId = espacioId,
                EspacioIdApi = espacio?.idApi,
                Resultado = AccesoTipo.Denegar,
                Motivo = $"Credencial no encontrada: {idCriptografico}",
                Modo = await _connectivityService.IsConnectedAsync() ? Modo.Online : Modo.Offline
            };

            await _db.SaveEventoAccesoAsync(evento);
            Debug.WriteLine($"[EventosService] Evento no encontrada registrado: ID={evento.EventoId}");

            await TrySincronizarEvento(evento);

            return evento;
        }

        /// <summary>
        /// Obtiene el historial de eventos de un espacio
        /// </summary>
        public async Task<List<EventoAcceso>> ObtenerHistorial(int espacioId, DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            try
            {
                var eventos = await _db.GetEventosAccesoByEspacioIdAsync(espacioId);
                
                if (fechaInicio.HasValue)
                {
                    eventos = eventos.Where(e => e.MomentoDeAcceso >= fechaInicio.Value).ToList();
                }
                
                if (fechaFin.HasValue)
                {
                    eventos = eventos.Where(e => e.MomentoDeAcceso <= fechaFin.Value).ToList();
                }

                return eventos.OrderByDescending(e => e.MomentoDeAcceso).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[EventosService] Error obteniendo historial: {ex.Message}");
                return new List<EventoAcceso>();
            }
        }

        /// <summary>
        /// Registra un evento manualmente
        /// </summary>
        public async Task<EventoAcceso> RegistrarEvento(EventoAcceso evento)
        {
            await _db.SaveEventoAccesoAsync(evento);
            await TrySincronizarEvento(evento);
            return evento;
        }

        /// <summary>
        /// Sincroniza eventos locales con la API
        /// </summary>
        public async Task<bool> SincronizarEventos()
        {
            try
            {
                if (!await _connectivityService.IsConnectedAsync())
                {
                    Debug.WriteLine("[EventosService] Sin conexión, no se puede sincronizar");
                    return false;
                }

                var eventosLocales = await _db.GetEventosAccesoNoSincronizadosAsync();
                
                foreach (var evento in eventosLocales)
                {
                    await TrySincronizarEvento(evento);
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[EventosService] Error sincronizando eventos: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Intenta sincronizar un evento con la API
        /// </summary>
        private async Task TrySincronizarEvento(EventoAcceso evento)
        {
            try
            {
                if (!await _connectivityService.IsConnectedAsync())
                {
                    return;
                }

                // Aquí se enviaría el evento a la API
                // var resultado = await _apiService.EnviarEventoAccesoAsync(evento);
                
                Debug.WriteLine($"[EventosService] Evento sincronizado (simulado): ID={evento.EventoId}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[EventosService] Error sincronizando evento: {ex.Message}");
            }
        }
    }
}
