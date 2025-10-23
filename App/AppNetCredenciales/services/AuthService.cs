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

        public async Task<bool> isUserLogged(string email)
        {
            var consulta = await SessionManager.isLogged();

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


        public async Task<bool> loginUsuario(string email, string password)
        {

            var consulta = await _db.loggeoCorrecto(email, password);
            if (consulta)
            {
                var usuario = await _db.GetUsuarioByEmailAsync(email);
                await SessionManager.SaveUserAsync(usuario.UsuarioId, usuario.Email);
                return true;
            }

            return false;

        }
    }
}
