using AppNetCredenciales.models;
using AppNetCredenciales.services;
using AppNetCredenciales.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.Communication;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        }

        public async Task<EventoAcceso> SaveAndPushEventoAccesoAsync(EventoAcceso evento)
        {
            if (evento == null) throw new ArgumentNullException(nameof(evento));

            await SaveEventoAccesoAsync(evento);

            try
            {
                if (!connectivityService.IsConnected)
                {
                    System.Diagnostics.Debug.WriteLine("[LocalDBService] Offline: saved evento locally, will sync later.");
                    return evento;
                }

                var dto = new ApiService.EventoAccesoDto
                {
                    EventoAccesoId = evento.idApi,
                    MomentoDeAcceso = evento.MomentoDeAcceso,
                    CredencialId = evento.Credencial?.idApi ?? evento.CredencialIdApi,
                    EspacioId = evento.Espacio?.idApi ?? evento.EspacioIdApi,
                    Resultado = evento.ResultadoStr,
                    Motivo = evento.Motivo,
                    Modo = evento.ModoStr,
                    Firma = evento.Firma
                };

                var created = await apiService.CreateEventoAccesoAsync(dto);
                if (created != null)
                {
                 
                    if (!string.IsNullOrWhiteSpace(created.EventoAccesoId))
                    {
                        evento.idApi = created.EventoAccesoId;
                        await SaveEventoAccesoAsync(evento);
                    }

                    System.Diagnostics.Debug.WriteLine($"[LocalDBService] Evento pushed to API, id={evento.idApi}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[LocalDBService] API did not return a created EventoAcceso.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LocalDBService] Push evento error: {ex}");
                // keep local record; will try again on next sync
            }

            return evento;
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
            var apiRoles = await apiService.GetRolesAsync();
            if (apiRoles == null)
            {
                return await GetRolesAsync();
            }
            var localList = await GetRolesAsync();
            foreach (var a in localList)
            {
                await DeleteRolAsync(a);
            }
            foreach (var a in apiRoles)
            {
                var nuevo = new Rol
                {
                    idApi = a.RolId,
                    Tipo = a.Tipo,
                    Prioridad = a.Prioridad,
                    FechaAsignado = a.fechaAsignado,
                };
                await SaveRolAsync(nuevo);
            }
            return await GetRolesAsync();
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
                Debug.WriteLine($"ESPACIO A GENERAR => {a.EspacioId}");
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

        public async Task<List<models.EventoAcceso>> GetEventosAccesoAsync()
        {
            return await _connection.Table<models.EventoAcceso>().ToListAsync();
        }

        public async Task<int> SaveEventoAccesoAsync(models.EventoAcceso evento) { 
        

            if (evento.EventoId == 0)
            {
                return await _connection.InsertAsync(evento);
            }
            else
            {
                return await _connection.UpdateAsync(evento);
            }
        }

        public async Task<bool> existeEventoAcceso(int id)
        {
            EventoAcceso ea = await _connection.GetAsync<EventoAcceso>(id);

            if (ea.Equals(null)) return false;

            return true;
        }

        public async Task<int> DeleteEventoAccesoAsync(models.EventoAcceso evento)
        {
            return await _connection.DeleteAsync(evento);
        }

        public async Task<models.EventoAcceso> GetEventoAccesoByIdAsync(int id)
        {
            return await _connection.Table<models.EventoAcceso>()
                .Where(e => e.EventoId == id)
                .FirstOrDefaultAsync();
        }

        public async Task<int> ModifyEventoAcceso(int id, EventoAcceso e)
        {
            var ev = await _connection.Table<models.EventoAcceso>()
                .Where(e => e.EventoId == id)
                .FirstOrDefaultAsync();

            return await _connection.UpdateAsync(ev);
        }


        // Rol

        public async Task<bool> UsuarioRolExistsAsync(int usuarioId, int rolId)
        {
            var ur = await _connection.Table<UsuarioRol>()
                .Where(x => x.UsuarioId == usuarioId && x.RolId == rolId)
                .FirstOrDefaultAsync();
            return ur != null;
        }

        public async Task ChangeUserSelectedRole(string email, int idRole)
        {
            var user = await GetUsuarioByEmailAsync(email);
            if (user == null) return;

            // if the UsuarioRol relation does not exist, create it
            if (!await UsuarioRolExistsAsync(user.UsuarioId, idRole))
            {
                return;
            }

            user.RolId = idRole;
            await SaveUsuarioAsync(user);

            if (await SessionManager.IsLoggedAsync())
            {
                var emailLogged = await SessionManager.GetUserEmailAsync();
                if (emailLogged == email)
                {
                    await SessionManager.SaveUserRoleAsync(idRole);
                }
            }
        }

        public async Task<models.Rol> GetLoggedUserRoleAsync()
        {
            var user = await GetLoggedUserAsync();
            if (user != null && user.RolId != null)
            {
                return await GetRolByIdAsync(user.RolId.Value);
            }
            return null;
        }

        public async Task<List<Rol>> GetRolsByUserAsync(int id)
        {
            var usuario = await GetUsuarioByIdAsync(id);

            var usuarioRols = await _connection.Table<UsuarioRol>()
                .Where(ur => ur.UsuarioId == usuario.UsuarioId)
                .ToListAsync();


            var listarTodosUsuariosRols = await _connection.Table<UsuarioRol>()
                .ToListAsync();

            foreach (var ur in listarTodosUsuariosRols)
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


        public async Task<List<models.Rol>> GetRolesAsync()
        {

             if(connectivityService.IsConnected)
            {
                List<Rol> listaRoles = new List<Rol>();

                var listApi = await apiService.GetRolesAsync();
                foreach (var a in listApi)
                {
                    var rol = new Rol
                    {
                        idApi = a.RolId,
                        Tipo = a.Tipo,
                        Prioridad = a.Prioridad,
                        FechaAsignado = a.fechaAsignado,
                    };
                    listaRoles.Add(rol);
                }

                return listaRoles;
            }

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

        public async Task<models.Rol> GetRolByIdAsync(int id)
        {
            return await _connection.Table<models.Rol>()
                .Where(r => r.RolId == id)
                .FirstOrDefaultAsync();
        }


        // CRUD operaciones para Espacios

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

        public async Task<models.Espacio> GetEspacioByIdAsync(int id)
        {
            return await _connection.Table<models.Espacio>()
                .Where(e => e.EspacioId == id)
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
            if (usuario == null) return null;
            if (!connectivityService.IsConnected) return null;

            var newUserDto = new ApiService.NewUsuarioDto
            {
                Nombre = usuario.Nombre ?? string.Empty,
                Apellido = usuario.Apellido ?? string.Empty,
                Email = usuario.Email ?? string.Empty,
                Documento = usuario.Documento ?? string.Empty,
                Password = usuario.Password ?? string.Empty
            };

            var created = await apiService.CreateUsuarioAsync(newUserDto);
            if (created != null && !string.IsNullOrWhiteSpace(created.UsuarioId))
            {
                usuario.idApi = created.UsuarioId;
                await SaveUsuarioAsync(usuario);
                return usuario.idApi;
            }

            return null;
        }

        public async Task<int> SaveCredencialAsync(models.Credencial credencial)
        {
            if (credencial == null) return 0;

            // Try remote creation only when connected
            if (connectivityService.IsConnected)
            {
                // Ensure we have a valid usuarioIdApi (GUID). If not, try to get/create it.
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

            if (usuario.CredencialId != null)
            {
                return await GetCredencialByIdAsync(usuario.CredencialId);
            }
            else
            {
                return null;

            }
        }

        public async Task<models.Credencial?> GetCredencialByCryptoIdAsync(string idCriptografico)
        {
            if (string.IsNullOrWhiteSpace(idCriptografico))
                return null;

            idCriptografico = idCriptografico.Trim();

            var exact = await _connection.Table<models.Credencial>()
                                         .Where(c => c.IdCriptografico == idCriptografico)
                                         .FirstOrDefaultAsync();
            if (exact != null)
                return exact;

            // Fallback: load all and match in-memory using trimmed, case-insensitive comparison
            var all = await GetCredencialesAsync();
            var found = all.FirstOrDefault(c => string.Equals(c.IdCriptografico?.Trim(),
                                                             idCriptografico,
                                                             StringComparison.OrdinalIgnoreCase));
            if (found != null)
                return found;

            System.Diagnostics.Debug.WriteLine($"[LocalDBService] GetCredencialByCryptoIdAsync: '{idCriptografico}' not found. DB Creds: {string.Join(", ", all.Select(c => c.IdCriptografico ?? "<null>"))}");
            return null;
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
            var usuarios = await GetUsuariosAsync();
            if (usuarios.Count == 0)
            {
                var admin = new models.Usuario
                {
                    Email = "admin@test.com",
                    Password = "1234",
                    Nombre = "Administrador"
                };

                var user = new models.Usuario
                {
                    Email = "user@test.com",
                    Password = "abcd",
                    Nombre = "Usuario"
                };

                await SaveUsuarioAsync(admin);
                await SaveUsuarioAsync(user);
            }

            var roles = await GetRolesAsync();
            if(roles.Count == 0)
            {
                var rolAdmin = new models.Rol
                {
                    Tipo = "Administrador",
                    Prioridad = 1,
                    FechaAsignado = DateTime.Now
                };
                var rolUser = new models.Rol
                {
                    Tipo = "Usuario",
                    Prioridad = 1,
                    FechaAsignado = DateTime.Now
                };
                var rolFuncionario = new models.Rol
                {
                    Tipo = "Funcionario",
                    Prioridad = 1,
                    FechaAsignado = DateTime.Now
                };
                await SaveRolAsync(rolAdmin);
                await SaveRolAsync(rolUser);
                await SaveRolAsync(rolFuncionario);
            }
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
    }
}