using AppNetCredenciales.Data;
using AppNetCredenciales.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppNetCredenciales.services
{
    public class AuthService
    {
        private readonly LocalDBService _db;


        public AuthService(LocalDBService db)
        {
            _db = db;
        }

        public async Task<List<Usuario>> GetUsuarios()
        {
            return await _db.GetUsuariosAsync();
        }

        public async Task ChangeRoleForLoggedUser(int newRole)
        {
                
            var usuario = await GetUserLogged();

            if (usuario != null)
            {
                await SessionManager.SaveUserRoleAsync(newRole);
                await _db.ChangeUserSelectedRole(usuario.Email, newRole);

            }

        }

        public async Task<bool> registrarUsuario(models.Usuario usuario)
        {
            var existingUser = await _db.GetUsuarioByEmailAsync(usuario.Email);
            if (existingUser != null)
            {
                return false;
            }
            await _db.SaveUsuarioAsync(usuario);
            return true;
        }

       
          

        public async Task<List<Rol>> GetRolesForLoggedUser()
        {
            var user = await GetUserLogged();
            if (user == null) return new List<Rol>();

            // _db.GetLoggedUserRoleAsync returns a single Rol (or null).
            var rol = await _db.GetLoggedUserRoleAsync();
            if (rol == null) return new List<Rol>();

            return new List<Rol> { rol };
        }

        public async Task<bool> isUserLogged()
        {
            var consulta = await SessionManager.IsLoggedAsync();
            return consulta;
        }

        public void logoutUsuario()
        {
            SessionManager.Logout();
        }

        public async Task<models.Usuario> getUsuarioData(string email)
        {
            var u = await _db.GetUsuarioByEmailAsync(email);
            return u;
        }

        public async Task<models.Usuario> GetUserLogged()
        {
            var u = await _db.GetLoggedUserAsync();
            return u;
        }

        public async Task<bool> loginUsuario(string email, string password)
        {
            var consulta = await _db.loggeoCorrecto(email, password);
            if (consulta)
            {
                var usuario = await _db.GetUsuarioByEmailAsync(email);
                await SessionManager.SaveUserAsync(usuario.UsuarioId, usuario.Email);

                if (usuario.RolId.HasValue && usuario.RolId.Value != 0)
                {
                    await SessionManager.SaveUserRoleAsync(usuario.RolId.Value);
                }

                return true;
            }

            return false;
        }
    }
}