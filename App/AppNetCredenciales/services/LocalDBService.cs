
using AppNetCredenciales.models;
using AppNetCredenciales.services;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AppNetCredenciales.Data
{
    public class LocalDBService
    {

        private const string DBName = "LocalDB.db3";
        private readonly SQLiteAsyncConnection _connection;

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

        public async Task<List<models.Espacio>> GetEspaciosFuturos()
        {
            var today = DateTime.Today;
            return await _connection.Table<models.Espacio>()
                .Where(e => e.Fecha >= today)
                .ToListAsync();
        }


        // CRUD operaciones para credenciales
        public async Task<List<models.Credencial>> GetCredencialesAsync()
        {
            return await _connection.Table<models.Credencial>().ToListAsync();
        }

        public async Task<int> SaveCredencialAsync(models.Credencial credencial)
        {
            if (credencial.CredencialId == 0)
            {
                return await _connection.InsertAsync(credencial);
            }
            else
            {
                return await _connection.UpdateAsync(credencial);
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
    }
}