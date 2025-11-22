using AppNetCredenciales.models;
using AppNetCredenciales.services;
using AppNetCredenciales.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.Communication;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AppNetCredenciales.Data
{
    public class LocalDBService
    {

        private const string DBName = "LocalDB.db3";
        private readonly SQLiteAsyncConnection _connection;
        private readonly ApiService apiService = new ApiService();
        private readonly ConnectivityService connectivityService = new ConnectivityService();


        public LocalDBService()
        {
            _connection = new SQLiteAsyncConnection(Path.Combine(FileSystem.AppDataDirectory, DBName));
      
            _connection.CreateTableAsync<Usuario>().GetAwaiter().GetResult();
            _connection.CreateTableAsync<Rol>().GetAwaiter().GetResult();
            _connection.CreateTableAsync<UsuarioRol>().GetAwaiter().GetResult();
            _connection.CreateTableAsync<Credencial>().GetAwaiter().GetResult();
            _connection.CreateTableAsync<Espacio>().GetAwaiter().GetResult();
            _connection.CreateTableAsync<EventoAcceso>().GetAwaiter().GetResult();
            _connection.CreateTableAsync<EspacioReglaDeAcceso>().GetAwaiter().GetResult();
            _connection.CreateTableAsync<ReglaDeAcceso>().GetAwaiter().GetResult();
            _connection.CreateTableAsync<Beneficio>().GetAwaiter().GetResult();


        }



        public async Task<Beneficio> CanjearBeneficio(string idUsuario, string idBeneficio)
        {
            Beneficio beneficio = null;

            // Intentar obtener beneficios de la API si hay conexión
            if (connectivityService.IsConnected)
            {
                try
                {
                    var beneficios = await apiService.GetBeneficiosAsync();
                    foreach (var b in beneficios)
                    {
                        if (b.Id == idBeneficio)
                        {
                            beneficio = await _connection.Table<Beneficio>()
                                .Where(ben => ben.idApi == b.Id)
                                .FirstOrDefaultAsync();
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[LocalDBService] Error obteniendo beneficios de API: {ex.Message}");
                }
            }

            // Si no se encontró en API o no hay conexión, buscar en local
            if (beneficio == null)
            {
                System.Diagnostics.Debug.WriteLine("[LocalDBService] Buscando beneficio en base local...");
                beneficio = await _connection.Table<Beneficio>()
                    .Where(ben => ben.idApi == idBeneficio)
                    .FirstOrDefaultAsync();
            }

            // Si aún no se encuentra, crear un registro temporal
            if (beneficio == null)
            {
                System.Diagnostics.Debug.WriteLine($"[LocalDBService] ⚠️ Beneficio {idBeneficio} no encontrado, creando registro temporal");
                beneficio = new Beneficio
                {
                    idApi = idBeneficio,
                    Nombre = "Beneficio Temporal",
                    Descripcion = "Canje offline pendiente de sincronización",
                    UsuariosIDsJson = "[]",
                    FaltaCarga = true,
                    VigenciaInicio = DateTime.Now,
                    VigenciaFin = DateTime.Now.AddYears(1)
                };
                await SaveBeneficioAsync(beneficio);
            }

            // Procesar el canje local
            var lista = string.IsNullOrWhiteSpace(beneficio.UsuariosIDsJson)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(beneficio.UsuariosIDsJson);

            if (!lista.Contains(idUsuario))
            {
                lista.Add(idUsuario);
                System.Diagnostics.Debug.WriteLine($"[LocalDBService] Usuario {idUsuario} agregado al beneficio {beneficio.Nombre}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[LocalDBService] Usuario {idUsuario} ya tenía el beneficio {beneficio.Nombre}");
            }

            beneficio.UsuariosIDsJson = JsonSerializer.Serialize(lista);

            // Intentar llamar al API si hay conexión
            if (connectivityService.IsConnected)
            {
                try
                {
                    var canje = new ApiService.CanjeDto
                    {
                        beneficioId = idBeneficio,
                        usuarioId = idUsuario
                    };

                    var resultadoCanje = await apiService.CanjearBeneficio(canje);

                    if (resultadoCanje != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"[LocalDBService] ✅ Canje enviado exitosamente al API");
                        beneficio.FaltaCarga = false;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[LocalDBService] ❌ Error enviando canje al API, marcando para sincronización posterior");
                        beneficio.FaltaCarga = true;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[LocalDBService] ❌ Excepción enviando canje al API: {ex.Message}");
                    beneficio.FaltaCarga = true;
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[LocalDBService] Offline: beneficio canjeado locally, will sync later.");
                beneficio.FaltaCarga = true;
            }

            // Guardar cambios localmente
            await _connection.UpdateAsync(beneficio);

            System.Diagnostics.Debug.WriteLine($"[LocalDBService] Canje completado para usuario {idUsuario} en beneficio {beneficio.Nombre} (FaltaCarga: {beneficio.FaltaCarga})");

            return beneficio;
        }


        public async Task<Beneficio> updateBeneficio(ApiService.BeneficioDto beneficio)
        {
            var ben = new Beneficio
            {
                idApi = beneficio.Id,
                Descripcion = beneficio.Descripcion,
                Tipo = beneficio.Tipo,
                Nombre = beneficio.Nombre,
                VigenciaInicio = beneficio.VigenciaInicio ?? default(DateTime),
                VigenciaFin = beneficio.VigenciaFin ?? default(DateTime),
                CupoTotal = beneficio.CupoTotal,
                CupoPorUsuario = beneficio.CupoPorUsuario,
                RequiereBiometria = beneficio.RequiereBiometria,
                EspaciosIDsJson = JsonSerializer.Serialize(beneficio.EspaciosIDs)
            };
         

            if(connectivityService.IsConnected)
            {
                await apiService.UpdateBeneficioAsync(beneficio);
            } else
            {
                System.Diagnostics.Debug.WriteLine("[LocalDBService] Offline: beneficio updated locally, will sync later.");
                ben.FaltaCarga = true;
            }

                await _connection.UpdateAsync(ben);

            return ben;
        } 

        public async Task<EventoAcceso> SaveAndPushEventoAccesoAsync(EventoAcceso evento)
        {
            if (evento == null) throw new ArgumentNullException(nameof(evento));

            if (evento.MomentoDeAcceso.Kind != DateTimeKind.Utc)
            {
                evento.MomentoDeAcceso = evento.MomentoDeAcceso.ToUniversalTime();
            }

            await SaveEventoAccesoAsync(evento);

            try
            {
                if (!connectivityService.IsConnected)
                {
                    System.Diagnostics.Debug.WriteLine("[LocalDBService] Offline: saved evento locally, will sync later.");
                    return evento;
                }

                string? credencialApiId = evento.CredencialIdApi;
                string? espacioApiId = evento.EspacioIdApi;

                System.Diagnostics.Debug.WriteLine($"[LocalDBService] === PREPARING TO PUSH EVENTO ===");
                System.Diagnostics.Debug.WriteLine($"[LocalDBService] - CredencialIdApi: '{credencialApiId}'");
                System.Diagnostics.Debug.WriteLine($"[LocalDBService] - EspacioIdApi: '{espacioApiId}'");
                System.Diagnostics.Debug.WriteLine($"[LocalDBService] - Resultado: {evento.Resultado}");

                // Verificar que tenemos los IDs necesarios
                if (string.IsNullOrEmpty(credencialApiId))
                {
                    System.Diagnostics.Debug.WriteLine("[LocalDBService] ⚠️ Cannot push evento: missing CredencialIdApi");
                    return evento;
                }

                if (string.IsNullOrEmpty(espacioApiId))
                {
                    return evento;
                }

                var dto = new ApiService.EventoAccesoDto
                {
                    // ❌ NO enviar EventoAccesoId en la creación
                    MomentoDeAcceso = evento.MomentoDeAcceso, // Ya está en UTC
                    CredencialId = credencialApiId,
                    EspacioId = espacioApiId,
                    Resultado = evento.ResultadoStr, // "Permitir" o "Denegar"
                    Motivo = evento.Motivo ?? "Acceso procesado",
                    Modo = evento.ModoStr ?? "Online", // "Online" u "Offline"
                    Firma = evento.Firma ?? ""
                };

                System.Diagnostics.Debug.WriteLine($"[LocalDBService] === SENDING DTO TO API ===");
                System.Diagnostics.Debug.WriteLine($"[LocalDBService] - MomentoDeAcceso: {dto.MomentoDeAcceso:yyyy-MM-ddTHH:mm:ss.fffZ}");
                System.Diagnostics.Debug.WriteLine($"[LocalDBService] - CredencialId: '{dto.CredencialId}'");
                System.Diagnostics.Debug.WriteLine($"[LocalDBService] - EspacioId: '{dto.EspacioId}'");
                System.Diagnostics.Debug.WriteLine($"[LocalDBService] - Resultado: '{dto.Resultado}'");
                System.Diagnostics.Debug.WriteLine($"[LocalDBService] - Motivo: '{dto.Motivo}'");
                System.Diagnostics.Debug.WriteLine($"[LocalDBService] - Modo: '{dto.Modo}'");
                System.Diagnostics.Debug.WriteLine($"[LocalDBService] - Firma: '{dto.Firma}'");

                var created = await apiService.CreateEventoAccesoAsync(dto);
                if (created != null)
                {
                    // ✅ Usar el ID devuelto por el API
                    string? apiId = created.EventoAccesoId ?? created.Id;
                    if (!string.IsNullOrWhiteSpace(apiId))
                    {
                        evento.idApi = apiId;
                        await SaveEventoAccesoAsync(evento);
                    }

                    System.Diagnostics.Debug.WriteLine($"[LocalDBService] ✅ Evento pushed successfully, id={evento.idApi}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[LocalDBService] ❌ API did not return a created EventoAcceso.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LocalDBService] ❌ Push evento error: {ex}");
                System.Diagnostics.Debug.WriteLine($"[LocalDBService] Exception details: {ex}");
            }

            return evento;
        }

        public async Task<List<Beneficio>> GetBeneficiosAsyncApi()
        {
            var dtos =  await apiService.GetBeneficiosAsync();

            var beneficios = new List<Beneficio>();

            foreach(var dto in dtos)
            {
                var beneficio = new Beneficio
                {
                    idApi = dto.Id,
                    Descripcion = dto.Descripcion,
                    Tipo = dto.Tipo,
                    Nombre = dto.Nombre,
                    VigenciaInicio = dto.VigenciaInicio ?? default(DateTime),
                    VigenciaFin = dto.VigenciaFin ?? default(DateTime),
                    CupoTotal = dto.CupoTotal,
                    CupoPorUsuario = dto.CupoPorUsuario,
                    RequiereBiometria = dto.RequiereBiometria,
                    EspaciosIDsJson = JsonSerializer.Serialize(dto.EspaciosIDs),
                    UsuariosIDsJson = JsonSerializer.Serialize(dto.UsuariosIDs)
                };
                beneficios.Add(beneficio);
            }


            return beneficios;
        }


        public async Task<List<Beneficio>> GetBeneficiosAsync()
        {
            return await _connection.Table<models.Beneficio>().ToListAsync();
        }

        public async Task<List<Beneficio>> SincronizarBeneficiosFromBack(bool removeMissing = false)
        {
            // 1) Pedir datos al backend
            var apiBeneficios = await apiService.GetBeneficiosAsync();

            if (apiBeneficios == null)
            {
                // Si falla el API → devolver lo que haya localmente
                return await GetBeneficiosLocalAsync();
            }

            // 2) Obtener lista local actual
            var localList = await GetBeneficiosLocalAsync();

         
            foreach (var local in localList)
                await DeleteBeneficioAsync(local);


            foreach (var api in apiBeneficios)
            {
                var nuevo = new Beneficio
                {
                    idApi = api.Id,
                    Descripcion = api.Descripcion,
                    Tipo = api.Tipo,
                    Nombre = api.Nombre,
                    VigenciaInicio = api.VigenciaInicio ?? default(DateTime),
                    VigenciaFin = api.VigenciaFin ?? default(DateTime),
                    CupoTotal = api.CupoTotal,
                    CupoPorUsuario = api.CupoPorUsuario,
                    RequiereBiometria = api.RequiereBiometria,
                    EspaciosIDsJson = JsonSerializer.Serialize(api.EspaciosIDs),
                    UsuariosIDsJson = JsonSerializer.Serialize(api.UsuariosIDs)
                };

                await SaveBeneficioAsync(nuevo);
            }



            return await GetBeneficiosLocalAsync();
        }


        public async Task<int> SaveBeneficioAsync(Beneficio beneficio)
        {
            if (beneficio.BeneficioId == 0)
                return await _connection.InsertAsync(beneficio);

            return await _connection.UpdateAsync(beneficio);
        }

        public async Task<int> DeleteBeneficioAsync(Beneficio beneficio)
        {
            return await _connection.DeleteAsync(beneficio);
        }

        public async Task<List<Beneficio>> GetBeneficiosLocalAsync()
        {
            return await _connection.Table<Beneficio>().ToListAsync();
        }



        public async Task<List<EventoAcceso>> SincronizarEventosFromBack(bool removeMissing = false)
        {
            var apiEventos = await apiService.GetEventosAccesoAsync();
            if (apiEventos == null)
            {
                return await GetEventosAccesoAsync();
            }
            var localList = await GetEventosAccesoAsync();
            foreach (var a in localList)
            {
                await DeleteEventoAccesoAsync(a);
            }
            foreach (var a in apiEventos)
            {
                var nuevo = new EventoAcceso
                {
                    idApi = a.EventoAccesoId,
                    MomentoDeAcceso = a.MomentoDeAcceso,
                    CredencialIdApi = a.CredencialId,
                    EspacioIdApi = a.EspacioId,
                    Resultado = ParseEnumOrDefault<AccesoTipo>(a.Resultado),
                    Motivo = a.Motivo
                };
                await SaveEventoAccesoAsync(nuevo);
            }
            return await GetEventosAccesoAsync();
        }

        // Roles

        public async Task<List<Rol>> SincronizarRolesFromBack(bool removeMissing = false)
        {
            try
            {
                var apiRoles = await apiService.GetRolesAsync();
                if (apiRoles == null || apiRoles.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("[LocalDBService] No se obtuvieron roles del API");
                    return await _connection.Table<models.Rol>().ToListAsync();
                }

                var localList = await _connection.Table<models.Rol>().ToListAsync();
                foreach (var local in localList)
                {
                    await _connection.DeleteAsync(local);
                }
                int insertados = 0;
                foreach (var apiRol in apiRoles)
                {
                    var nuevo = new Rol
                    {
                        idApi = apiRol.RolId,
                        Tipo = apiRol.Tipo,
                        Prioridad = apiRol.Prioridad,
                        FechaAsignado = apiRol.fechaAsignado,
                    };
                    await _connection.InsertAsync(nuevo);
                    insertados++;
                    System.Diagnostics.Debug.WriteLine($"[LocalDBService] Rol sincronizado: {nuevo.Tipo} (API ID: {nuevo.idApi})");
                }

                System.Diagnostics.Debug.WriteLine($"[LocalDBService] ✅ Sincronización completada: {insertados} roles insertados");
                return await _connection.Table<models.Rol>().ToListAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LocalDBService] ❌ Error en SincronizarRolesFromBack: {ex.Message}");
                return await _connection.Table<models.Rol>().ToListAsync();
            }
        }

        // Credencial

        public async Task<List<Credencial>> SincronizarCredencialesFromBack(bool removeMissing = false)
        {
            var apiCredenciales = await apiService.GetCredencialesAsync();
            if (apiCredenciales == null)
            {
                return await GetCredencialesAsync();
            }
            var localList = await GetCredencialesAsync();
            foreach (var a in localList)
            {
                await DeleteCredencialAsync(a);
            }
            foreach (var a in apiCredenciales)
            {
                var nuevo = new Credencial
                {
                    idApi = a.CredencialId,
                    Tipo = ParseEnumOrDefault<CredencialTipo>(a.Tipo),
                    Estado = ParseEnumOrDefault<CredencialEstado>(a.Estado),
                    IdCriptografico = a.IdCriptografico,
                    FechaEmision = a.FechaEmision,
                    FechaExpiracion = a.FechaExpiracion,
                    FaltaCarga = false,
                    usuarioIdApi = a.usuarioIdApi
                };
                await SaveCredencialAsync(nuevo);
            }
            return await GetCredencialesAsync();
        }

        // Sincronizacion Maui to back
        private static T ParseEnumOrDefault<T>(string? value) where T : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(value))
                return default;

            value = value.Trim();

            if (int.TryParse(value, out var intVal))
            {
                if (Enum.IsDefined(typeof(T), intVal))
                    return (T)Enum.ToObject(typeof(T), intVal);
            }

            if (Enum.TryParse<T>(value, ignoreCase: true, out var parsed))
                return parsed;

            
            return default;
        }
        public async Task<List<Usuario>> SincronizarUsuariosFromBack(bool removeMissing = false)
        {

            var apiEspacios = await apiService.GetUsuariosAsync();

            if (apiEspacios == null)
            {
                return await GetUsuariosAsync();
            }
            var localList = await GetUsuariosAsync();

            foreach (var a in localList)
            {
                await DeleteUsuarioAsync(a);
            }

            

                foreach (var a in apiEspacios)
                {
                    var nuevo = new Usuario
                    {
                        Nombre = a.Nombre,
                        Apellido = a.Apellido,
                        Email = a.Email,
                        FaltaCargar = false,
                        Documento = a.Documento,
                        RolesIDs = a.RolesIDs,
                        idApi = a.UsuarioId,
                        Password = a.Password
                    };

                    await SaveUsuarioAsync(nuevo);
                }
                    
            

            // Devuelve la lista actualizada
            return await GetUsuariosAsync();
        }
        public async Task<List<Espacio>> SincronizarEspaciosFromBack(bool removeMissing = false)
        {

            var apiEspacios = await apiService.GetEspaciosAsync();

            if (apiEspacios == null)
            {
                return await GetEspaciosAsync();
            }
            var localList = await GetEspaciosAsync();

            foreach (var a in localList)
            {
                await DeleteEspacioAsync(a);
            }

            foreach (var a in apiEspacios)
            {
                var nuevo = new Espacio
                {
                    idApi = a.EspacioId,
                    Nombre = a.Nombre,
                    Activo = a.Activo,
                    Tipo = ParseEnumOrDefault<EspacioTipo>(a.Tipo),
                    faltaCarga = false
                };
                await SaveEspacioAsync(nuevo);
            }

            // Devuelve la lista actualizada
            return await GetEspaciosAsync();
        }

        public async Task<Espacio> GetEventoByIdAsync(int id)
        {
         
                return await _connection.Table<Espacio>()
                    .Where(e => e.EspacioId == id)
                    .FirstOrDefaultAsync();
         
        }


        public async Task<List<EventoAcceso>> GetEventosAccesoByUsuarioIdAsync(int credencialId)
        {
            return await _connection.Table<EventoAcceso>()
                .Where(e => e.CredencialId == credencialId)
                .ToListAsync();
        }

        public async Task EnsureSchemaAndDataAsync()
        {
            try
            {
                // Inspect the local DB rows directly instead of calling GetRolesAsync()
                // (GetRolesAsync may return API data when online and that list will have RolId == 0,
                // causing an unwanted schema reset).
                var localRoles = await _connection.Table<Rol>().ToListAsync();

                // Only recreate schema when the local DB contains broken/placeholder rows (RolId == 0)
                // coming from the local DB itself.
                if (localRoles.Any(r => r.RolId == 0))
                {
                    await _connection.DropTableAsync<UsuarioRol>();
                    await _connection.DropTableAsync<Rol>();
                    await _connection.DropTableAsync<Usuario>();
                    await _connection.DropTableAsync<Credencial>();
                    await _connection.DropTableAsync<EspacioReglaDeAcceso>();
                    await _connection.DropTableAsync<ReglaDeAcceso>();
                    await _connection.DropTableAsync<Espacio>();
                    await _connection.DropTableAsync<EventoAcceso>();

                    // Recreate tables
                    await _connection.CreateTableAsync<Usuario>();
                    await _connection.CreateTableAsync<Rol>();
                    await _connection.CreateTableAsync<UsuarioRol>();
                    await _connection.CreateTableAsync<Credencial>();
                    await _connection.CreateTableAsync<Espacio>();
                    await _connection.CreateTableAsync<EventoAcceso>();
                    await _connection.CreateTableAsync<EspacioReglaDeAcceso>();
                    await _connection.CreateTableAsync<ReglaDeAcceso>();

                    // Seed default data again
                    await InitializeAsync();

                    System.Diagnostics.Debug.WriteLine("[LocalDBService] Schema recreated and data seeded.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[LocalDBService] EnsureSchemaAndDataAsync error: " + ex);
            }
        }


        // usuario logueado
        public async Task<models.Usuario> GetLoggedUserAsync()
        {
            if (await SessionManager.IsLoggedAsync())
            {
                var email = await SessionManager.GetUserEmailAsync();
                return await GetUsuarioByEmailAsync(email);
            }
            return null;
        }


        // Evento Acceso CRUD

        // CRUD operaciones para EventoAcceso
        
        public async Task<List<EventoAcceso>> GetEventosAccesoAsync()
        {
            return await _connection.Table<EventoAcceso>().ToListAsync();
        }

        public async Task<int> SaveEventoAccesoAsync(EventoAcceso evento)
        {
            if (evento == null) throw new ArgumentNullException(nameof(evento));

            // Asegurar que la fecha esté en UTC
            if (evento.MomentoDeAcceso.Kind != DateTimeKind.Utc)
            {
                evento.MomentoDeAcceso = evento.MomentoDeAcceso.ToUniversalTime();
            }

            // Si tiene ID de API y ya existe localmente, actualizar
            if (!string.IsNullOrWhiteSpace(evento.idApi))
            {
                var existing = await _connection.Table<EventoAcceso>()
                    .Where(e => e.idApi == evento.idApi)
                    .FirstOrDefaultAsync();

                if (existing != null)
                {
                    existing.MomentoDeAcceso = evento.MomentoDeAcceso;
                    existing.CredencialIdApi = evento.CredencialIdApi;
                    existing.EspacioIdApi = evento.EspacioIdApi;
                    existing.Resultado = evento.Resultado;
                    existing.Motivo = evento.Motivo;
                    existing.Modo = evento.Modo;
                    existing.Firma = evento.Firma;
                    return await _connection.UpdateAsync(existing);
                }
            }

            // Si no existe, insertar
            return await _connection.InsertAsync(evento);
        }

        public async Task<int> DeleteEventoAccesoAsync(EventoAcceso evento)
        {
            return await _connection.DeleteAsync(evento);
        }

        public async Task<EventoAcceso?> GetEventoAccesoByIdAsync(string idApi)
        {
            return await _connection.Table<EventoAcceso>()
                .Where(e => e.idApi == idApi)
                .FirstOrDefaultAsync();
        }

        public async Task<EventoAcceso?> GetEventoAccesoByIdAsync(int eventoId)
        {
            return await _connection.Table<EventoAcceso>()
                .Where(e => e.EventoId == eventoId)
                .FirstOrDefaultAsync();
        }

        // CRUD operaciones para Roles

        public async Task<List<models.Rol>> GetRolesAsync()
        {
            return await _connection.Table<models.Rol>().ToListAsync();
        }

        public async Task<int> SaveRolAsync(models.Rol rol)
        {
            if (rol.RolId == 0)
            {
                return await _connection.InsertAsync(rol);
            }
            else
            {
                return await _connection.UpdateAsync(rol);
            }
        }

        public async Task<int> DeleteRolAsync(models.Rol rol)
        {
            return await _connection.DeleteAsync(rol);
        }

        public async Task<models.Rol?> GetRolByIdAsync(int id)
        {
            return await _connection.Table<models.Rol>()
                .Where(r => r.RolId == id)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Rol>> GetRolsByUserAsync(int usuarioId)
        {
            var usuarioRols = await _connection.Table<UsuarioRol>()
                .Where(ur => ur.UsuarioId == usuarioId)
                .ToListAsync();
                
            System.Diagnostics.Debug.WriteLine($"[LocalDBService] GetRolsByUserAsync - UsuarioId: {usuarioId}, UsuarioRols encontrados: {usuarioRols.Count}");

            foreach (var ur in usuarioRols)
            {
                System.Diagnostics.Debug.WriteLine($"UsuarioRol - Id: {ur.Id}, UsuarioId: {ur.UsuarioId}, RolId: {ur.RolId}, FechaAsignado: {ur.FechaAsignado}");
            }

            var rolIds = usuarioRols.Select(ur => ur.RolId).ToList();

            var roles = new List<Rol>();
            foreach (var rolId in rolIds)
            {
                var rol = await GetRolByIdAsync(rolId);
                if (rol != null)
                {
                    roles.Add(rol);
                }
            }
            return roles;
        }

        public async Task ChangeUserSelectedRole(string email, int newRoleId)
        {
            var usuario = await _connection.Table<Usuario>()
                .Where(u => u.Email == email)
                .FirstOrDefaultAsync();

            if (usuario != null)
            {
                usuario.RolId = newRoleId;
                await _connection.UpdateAsync(usuario);
                System.Diagnostics.Debug.WriteLine($"[LocalDBService] Changed user {email} to role {newRoleId}");
            }
        }

        // CRUD operaciones para espacios

        public async Task<List<models.Espacio>> GetEspaciosAsync()
        {
            
                
            return await _connection.Table<models.Espacio>().ToListAsync();
        }

        public async Task<int> SaveEspacioAsync(models.Espacio espacio)
        {
            if (espacio.EspacioId == 0)
            {
                return await _connection.InsertAsync(espacio);
            }
            else
            {
                return await _connection.UpdateAsync(espacio);
            }
        }

        public async Task<int> DeleteEspacioAsync(models.Espacio espacio)
        {
            return await _connection.DeleteAsync(espacio);
        }

        public async Task<models.Espacio> GetEspacioByIdAsync(string id)
        {
            return await _connection.Table<models.Espacio>()
                .Where(e => e.idApi == id)
                .FirstOrDefaultAsync();
        }

        private async Task<int> UpsertEspacioAsync(Espacio espacio)
        {
            if (espacio == null) return 0;

            if (!string.IsNullOrWhiteSpace(espacio.idApi))
            {
                var existing = await _connection.Table<Espacio>()
                    .Where(e => e.idApi == espacio.idApi)
                    .FirstOrDefaultAsync();

                if (existing != null)
                {
                    existing.Nombre = espacio.Nombre;
                    existing.Activo = espacio.Activo;
                    existing.Tipo = espacio.Tipo;
                    existing.faltaCarga = espacio.faltaCarga;
                    return await _connection.UpdateAsync(existing);
                }
            }

            return await _connection.InsertAsync(espacio);
        }


        // CRUD operaciones para credenciales
        public async Task<List<models.Credencial>> GetCredencialesAsync()
        {
            if(connectivityService.IsConnected)
            {
                var listApi = await apiService.GetCredencialesAsync();
                List<Credencial> listaCredenciales = new List<Credencial>();
                foreach (var a in listApi)
                {
                    var credencial = new Credencial
                    {
                        idApi = a.CredencialId,
                        Tipo = ParseEnumOrDefault<CredencialTipo>(a.Tipo),
                        Estado = ParseEnumOrDefault<CredencialEstado>(a.Estado),
                        IdCriptografico = a.IdCriptografico,
                        FechaEmision = a.FechaEmision,
                        FechaExpiracion = a.FechaExpiracion,
                        FaltaCarga = false,
                        usuarioIdApi = a.usuarioIdApi
                    };
                    listaCredenciales.Add(credencial);
                }
                return listaCredenciales;
            }

            return await _connection.Table<models.Credencial>().ToListAsync();
        }

        // Add these members inside the LocalDBService class

        // Upsert a credencial into local DB using idApi if present.
        private async Task<int> UpsertCredencialAsync(Credencial credencial)
        {
            if (credencial == null) return 0;

            if (!string.IsNullOrWhiteSpace(credencial.idApi))
            {
                var existing = await _connection.Table<Credencial>()
                                               .Where(c => c.idApi == credencial.idApi)
                                               .FirstOrDefaultAsync();
                if (existing != null)
                {
                    // copy relevant fields
                    existing.IdCriptografico = credencial.IdCriptografico;
                    existing.Tipo = credencial.Tipo;
                    existing.Estado = credencial.Estado;
                    existing.FaltaCarga = credencial.FaltaCarga;
                    existing.FechaEmision = credencial.FechaEmision;
                    existing.FechaExpiracion = credencial.FechaExpiracion;
                    existing.usuarioIdApi = credencial.usuarioIdApi;
                    return await _connection.UpdateAsync(existing);
                }
            }

            return await _connection.InsertAsync(credencial);
        }


        public async Task<string?> CreateUsuarioRemoteAsync(models.Usuario usuario)
        {
            if (usuario == null)
            {
                System.Diagnostics.Debug.WriteLine("[LocalDBService] ❌ CreateUsuarioRemoteAsync: usuario es null");
                return null;
            }

            if (!connectivityService.IsConnected)
            {
                System.Diagnostics.Debug.WriteLine("[LocalDBService] ❌ CreateUsuarioRemoteAsync: sin conexión");
                return null;
            }

            System.Diagnostics.Debug.WriteLine("[LocalDBService] === INICIANDO CREACIÓN DE USUARIO EN API ===");
            System.Diagnostics.Debug.WriteLine($"[LocalDBService] Usuario a crear:");
            System.Diagnostics.Debug.WriteLine($"[LocalDBService]   - Nombre: '{usuario.Nombre}'");
            System.Diagnostics.Debug.WriteLine($"[LocalDBService]   - Apellido: '{usuario.Apellido}'");
            System.Diagnostics.Debug.WriteLine($"[LocalDBService]   - Email: '{usuario.Email}'");
            System.Diagnostics.Debug.WriteLine($"[LocalDBService]   - Documento: '{usuario.Documento}'");
            System.Diagnostics.Debug.WriteLine($"[LocalDBService]   - Password: '{usuario.Password}'");

            var newUserDto = new ApiService.NewUsuarioDto
            {
                Nombre = usuario.Nombre ?? string.Empty,
                Apellido = usuario.Apellido ?? string.Empty,
                Email = usuario.Email ?? string.Empty,
                Documento = usuario.Documento ?? string.Empty,
                Password = usuario.Password ?? string.Empty,
                RolesIDs = usuario.RolesIDs // ✅ Agregar RolesIDs
            };

            System.Diagnostics.Debug.WriteLine("[LocalDBService] === DTO CREADO ===");
            System.Diagnostics.Debug.WriteLine($"[LocalDBService] DTO RolesIDs: [{string.Join(", ", newUserDto.RolesIDs ?? new string[0])}]");

            try
            {
                System.Diagnostics.Debug.WriteLine("[LocalDBService] Llamando a apiService.CreateUsuarioAsync...");

                var created = await apiService.CreateUsuarioAsync(newUserDto);

                System.Diagnostics.Debug.WriteLine("[LocalDBService] === RESPUESTA DEL API ===");

                if (created == null)
                {
                    System.Diagnostics.Debug.WriteLine("[LocalDBService] ❌ API devolvió NULL");
                    return null;
                }

                System.Diagnostics.Debug.WriteLine($"[LocalDBService] created.UsuarioId: '{created.UsuarioId}'");
                System.Diagnostics.Debug.WriteLine($"[LocalDBService] created.Nombre: '{created.Nombre}'");
                System.Diagnostics.Debug.WriteLine($"[LocalDBService] created.Email: '{created.Email}'");

                if (!string.IsNullOrWhiteSpace(created.UsuarioId))
                {
                    System.Diagnostics.Debug.WriteLine($"[LocalDBService] ✅ Usuario creado exitosamente con ID: {created.UsuarioId}");

                    // Actualizar el usuario local
                    usuario.idApi = created.UsuarioId;
                    usuario.FaltaCargar = false; // ✅ Marcar como sincronizado
                    await SaveUsuarioAsync(usuario);

                    System.Diagnostics.Debug.WriteLine($"[LocalDBService] Usuario local actualizado: idApi = {usuario.idApi}, FaltaCargar = {usuario.FaltaCargar}");

                    return usuario.idApi;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[LocalDBService] ❌ UsuarioId está vacío o null");
                    return null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LocalDBService] ❌ EXCEPCIÓN en CreateUsuarioRemoteAsync:");
                System.Diagnostics.Debug.WriteLine($"[LocalDBService] Mensaje: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[LocalDBService] StackTrace: {ex.StackTrace}");
                System.Diagnostics.Debug.WriteLine($"[LocalDBService] InnerException: {ex.InnerException?.Message}");
                return null;
            }
        }

        public async Task AgregarUsuarioARol(string rolIdApi, string usuarioIdApi)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(rolIdApi) || string.IsNullOrWhiteSpace(usuarioIdApi))
                {
                    System.Diagnostics.Debug.WriteLine("[LocalDBService] ❌ AgregarUsuarioARol: IDs inválidos");
                    return;
                }

                if (!connectivityService.IsConnected)
                {
                    System.Diagnostics.Debug.WriteLine("[LocalDBService] Sin conexión, no se puede actualizar rol en API");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"[LocalDBService] Agregando usuario {usuarioIdApi} al rol {rolIdApi}");

                // 1. Obtener el rol actual de la API
                var rolesApi = await apiService.GetRolesAsync();
                var rolEncontrado = rolesApi?.FirstOrDefault(r => r.RolId == rolIdApi);

                if (rolEncontrado == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[LocalDBService] ❌ Rol no encontrado en API: {rolIdApi}");
                    return;
                }

                // 2. Agregar el usuario a la lista si no está ya incluido
                var usuariosActuales = rolEncontrado.usuariosIDs?.ToList() ?? new List<string>();

                if (!usuariosActuales.Contains(usuarioIdApi))
                {
                    usuariosActuales.Add(usuarioIdApi);

                    // 3. Crear DTO para actualizar el rol
                    var rolActualizado = new ApiService.UpdateRolDto
                    {
                        RolId = rolIdApi,
                        Tipo = rolEncontrado.Tipo,
                        Prioridad = rolEncontrado.Prioridad,
                        FechaAsignado = rolEncontrado.fechaAsignado,
                        UsuariosIDs = usuariosActuales.ToArray()
                    };

                    // 4. Enviar actualización a la API
                    var resultado = await apiService.UpdateRolAsync(rolActualizado);

                    if (resultado != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"[LocalDBService] ✅ Rol actualizado exitosamente: {rolIdApi}");

                        // 5. Actualizar también en la base de datos local
                        var rolLocal = await _connection.Table<Rol>()
                            .Where(r => r.idApi == rolIdApi)
                            .FirstOrDefaultAsync();

                        if (rolLocal != null)
                        {
                            
                            rolLocal.UsuariosIDsJson = JsonSerializer.Serialize(usuariosActuales.ToArray());
                            await _connection.UpdateAsync(rolLocal);
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[LocalDBService] ❌ Error actualizando rol en API: {rolIdApi}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[LocalDBService] Usuario {usuarioIdApi} ya está en el rol {rolIdApi}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LocalDBService] Error en AgregarUsuarioARol: {ex.Message}");
            }
        }

        public async Task<string> SaveCredencialAsyncApi(models.Credencial credencial)
        {
            ApiService.CredentialDto crdDto = new ApiService.CredentialDto
            {
                CredencialId = credencial.idApi,
                Tipo = credencial.Tipo.ToString(),
                Estado = credencial.Estado.ToString(),
                IdCriptografico = credencial.IdCriptografico,
                FechaEmision = credencial.FechaEmision,
                FechaExpiracion = credencial.FechaExpiracion,
                usuarioIdApi = credencial.usuarioIdApi
            };

            var res = await apiService.crearCredencial(crdDto);

            return res;
        }

        public async Task<int> SaveCredencialAsync(models.Credencial credencial)
        {
            if (credencial == null) return 0;

            if (connectivityService.IsConnected)
            {
                
                if (string.IsNullOrWhiteSpace(credencial.usuarioIdApi) || !Guid.TryParse(credencial.usuarioIdApi, out _))
                {
                    try
                    {
                        if (await SessionManager.IsLoggedAsync())
                        {
                            var emailLogged = await SessionManager.GetUserEmailAsync();
                            if (!string.IsNullOrWhiteSpace(emailLogged))
                            {
                                var localUser = await GetUsuarioByEmailAsync(emailLogged);
                                if (localUser != null)
                                {
                                    // If the local user has no idApi, create the user on the backend first
                                    if (string.IsNullOrWhiteSpace(localUser.idApi))
                                    {
                                        var newUserDto = new ApiService.NewUsuarioDto
                                        {
                                            Nombre = localUser.Nombre ?? string.Empty,
                                            Apellido = localUser.Apellido ?? string.Empty,
                                            Email = localUser.Email ?? string.Empty,
                                            Documento = localUser.Documento ?? string.Empty,
                                            Password = localUser.Password ?? string.Empty
                                        };

                                        var created = await apiService.CreateUsuarioAsync(newUserDto);
                                        if (created != null && !string.IsNullOrWhiteSpace(created.UsuarioId))
                                        {
                                            localUser.idApi = created.UsuarioId;
                                            await SaveUsuarioAsync(localUser);
                                        }
                                        else
                                        {
                                            System.Diagnostics.Debug.WriteLine("[LocalDBService] Failed to create user on backend; will not create credencial remotely.");
                                        }
                                    }

                                    // Use the now-populated idApi if valid
                                    if (!string.IsNullOrWhiteSpace(localUser.idApi) && Guid.TryParse(localUser.idApi, out _))
                                    {
                                        credencial.usuarioIdApi = localUser.idApi;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[LocalDBService] Error ensuring usuarioIdApi: {ex.Message}");
                    }
                }

                // If we have a valid usuarioIdApi, attempt to create the credencial on the backend
                if (!string.IsNullOrWhiteSpace(credencial.usuarioIdApi) && Guid.TryParse(credencial.usuarioIdApi, out _))
                {
                    try
                    {
                        ApiService.CredentialDto dto = new ApiService.CredentialDto
                        {
                            CredencialId = credencial.idApi,
                            Tipo = credencial.Tipo.ToString(),
                            Estado = credencial.Estado.ToString(),
                            IdCriptografico = credencial.IdCriptografico,
                            FechaEmision = credencial.FechaEmision,
                            FechaExpiracion = credencial.FechaExpiracion,
                            usuarioIdApi = credencial.usuarioIdApi
                        };

                        var res = await apiService.crearCredencial(dto);
                        if (res == null)
                        {
                            System.Diagnostics.Debug.WriteLine("[LocalDBService] Error al crear la credencial en el back");
                            // fallback: continue and save locally
                        }
                        else
                        {
                            credencial.idApi = res;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[LocalDBService] crearCredencial exception: {ex.Message}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[LocalDBService] usuarioIdApi still invalid or absent after attempts; skipping remote create.");
                }
            }

            // Persist locally (always)
            if (credencial.CredencialId == 0)
            {
                await _connection.InsertAsync(credencial);
                return credencial.CredencialId;
            }
            else
            {
                await _connection.UpdateAsync(credencial);
                return credencial.CredencialId;
            }
        }

        public async Task<int> DeleteCredencialAsync(models.Credencial credencial)
        {
            return await _connection.DeleteAsync(credencial);
        }

        public async Task<models.Credencial> GetCredencialByIdAsync(int id)
        {
            return await _connection.Table<models.Credencial>()
                .Where(c => c.CredencialId == id)
                .FirstOrDefaultAsync();
        }

        public async Task<models.Credencial> GetCredencialByUser(string email)
        {
            var usuario = await GetUsuarioByEmailAsync(email);
            
            if (usuario == null)
            {
                return null;
            }

            // ✅ CORRECTO: Buscar credencial donde usuarioIdApi == usuario.idApi
            var credenciales = await GetCredencialesAsync();
            
            foreach (var c in credenciales)
            {
                if (c.usuarioIdApi == usuario.idApi)
                {
                    return c;
                }
            }

            return null;
        }

        /// <summary>
        /// Busca la credencial de un usuario por su idApi
        /// </summary>
        public async Task<models.Credencial?> GetCredencialByUsuarioIdApiAsync(string usuarioIdApi)
        {
            var credenciales = await GetCredencialesAsync();
            
            return credenciales.FirstOrDefault(c => c.usuarioIdApi == usuarioIdApi);
        }

        public async Task<bool> LoggedUserHasCredential()
        {
            if (await SessionManager.IsLoggedAsync())
            {
                var email = await SessionManager.GetUserEmailAsync();
                var credencial = await GetCredencialByUser(email);
                return credencial != null;
            }

            return false;
        }

        public async Task<Credencial> GetLoggedUserCredential()
        {
            if (await SessionManager.IsLoggedAsync())
            {
                var email = await SessionManager.GetUserEmailAsync();
                return await GetCredencialByUser(email);
            }
            return null;
        }




        // CRUD operaciones para Usuario
        public async Task<models.Rol?> GetRolByTipoAsync(string tipo)
        {
            if (string.IsNullOrWhiteSpace(tipo))
                return null;
                
            return await _connection.Table<models.Rol>()
                .Where(r => r.Tipo == tipo)
                .FirstOrDefaultAsync();
        }

        public async Task<List<models.Usuario>> GetUsuariosAsync()
        {
            return await _connection.Table<models.Usuario>().ToListAsync();
        }

        public async Task<int> SaveUsuarioAsync(models.Usuario usuario)
        {
            if (usuario.UsuarioId == 0)
            {
                return await _connection.InsertAsync(usuario);
            }
            else
            {
                return await _connection.UpdateAsync(usuario);
            }
        }

        public async Task<int> DeleteUsuarioAsync(models.Usuario usuario)
        {
            return await _connection.DeleteAsync(usuario);
        }


      

        public async Task InitializeAsync()
        {
            
        }

       
        

       

        // Nuevo: guardar relacion UsuarioRol
        public async Task<int> SaveUsuarioRolAsync(models.UsuarioRol usuarioRol)
        {
            if (usuarioRol == null) return 0;
            if (usuarioRol.Id == 0)
            {
                return await _connection.InsertAsync(usuarioRol);
            }
            else
            {
                return await _connection.UpdateAsync(usuarioRol);
            }
        }

        public async Task<int> DeleteUsuarioRolesForUserAsync(int usuarioId)
        {
            var lista = await _connection.Table<UsuarioRol>()
                .Where(ur => ur.UsuarioId == usuarioId)
                .ToListAsync();

            var removed = 0;
            foreach (var ur in lista)
            {
                removed += await _connection.DeleteAsync(ur);
            }
            return removed;
        }

        public async Task<models.Usuario> GetUsuarioByIdAsync(int id)
        {
            return await _connection.Table<models.Usuario>()
                .Where(u => u.UsuarioId == id)
                .FirstOrDefaultAsync();
        }

        public async Task<models.Usuario> GetUsuarioByEmailAsync(string email)
        {
            return await _connection.Table<models.Usuario>()
                .Where(u => u.Email == email)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> loggeoCorrecto(string email, string password)
        {
            var u = await GetUsuarioByEmailAsync(email);
            return u != null && u.Password == password;
        }

        public async Task<string> DumpDatabaseAsync()
        {
            try
            {
                var dump = new
                {
                    Usuarios = await GetUsuariosAsync(),
                    Roles = await GetRolesAsync(),
                    UsuarioRols = await _connection.Table<UsuarioRol>().ToListAsync(),
                    Credenciales = await GetCredencialesAsync(),
                    Espacios = await GetEspaciosAsync(),
                    EventosAcceso = await GetEventosAccesoAsync(),
                    ReglasDeAcceso = await _connection.Table<ReglaDeAcceso>().ToListAsync(),
                    EspacioReglas = await _connection.Table<EspacioReglaDeAcceso>().ToListAsync()
                };

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    ReferenceHandler = ReferenceHandler.IgnoreCycles
                };

                var json = JsonSerializer.Serialize(dump, options);
                System.Diagnostics.Debug.WriteLine(json);
                return json;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[LocalDBService] DumpDatabaseAsync error: " + ex);
                return $"ERROR: {ex.Message}";
            }
        }

        public async Task<List<Usuario>> GetUsuariosPendientesSyncAsync()
        {
            return await _connection.Table<Usuario>()
                .Where(u => u.FaltaCargar == true)
                .ToListAsync();
        }

        public async Task<List<EventoAcceso>> GetEventosAccesoPendientesSyncAsync()
        {
            return await _connection.Table<EventoAcceso>()
                .Where(e => string.IsNullOrEmpty(e.idApi) && 
                           !string.IsNullOrEmpty(e.CredencialIdApi) && 
                           !string.IsNullOrEmpty(e.EspacioIdApi))
                .ToListAsync();
        }

    
        public async Task<List<Beneficio>> GetBeneficiosPendientesSyncAsync()
        {
            return await _connection.Table<Beneficio>()
                .Where(b => b.FaltaCarga == true)
                .ToListAsync();
        }

        public async Task<bool> CrearBeneficioEnApiAsync(Beneficio beneficio)
        {
            try
            {
                if (!connectivityService.IsConnected)
                {
                    System.Diagnostics.Debug.WriteLine("[LocalDBService] Sin conexión, no se puede crear beneficio en API");
                    return false;
                }

                var nuevoBeneficioDto = new ApiService.BeneficioDto
                {
                    Id = beneficio.idApi,
                    Descripcion = beneficio.Descripcion,
                    Tipo = beneficio.Tipo,
                    Nombre = beneficio.Nombre,
                    VigenciaInicio = beneficio.VigenciaInicio,
                    VigenciaFin = beneficio.VigenciaFin,
                    CupoTotal = beneficio.CupoTotal,
                    CupoPorUsuario = beneficio.CupoPorUsuario,
                    RequiereBiometria = beneficio.RequiereBiometria,
                    EspaciosIDs = beneficio.EspaciosIDs,
                    UsuariosIDs = beneficio.UsuariosIDs
                };

                var resultado = await apiService.CreateBeneficioAsync(nuevoBeneficioDto);

                if (resultado != null)
                {
                    // Marcar como sincronizado
                    beneficio.FaltaCarga = false;
                    await SaveBeneficioAsync(beneficio);

                    System.Diagnostics.Debug.WriteLine($"[LocalDBService] ✅ Beneficio creado en API: {beneficio.idApi}");
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[LocalDBService] ❌ Error creando beneficio en API: {beneficio.idApi}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LocalDBService] Excepción creando beneficio en API: {ex.Message}");
                return false;
            }
        }
    }
}