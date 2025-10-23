using AppNetCredenciales.models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
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
      
            _connection.CreateTableAsync<models.Usuario>().GetAwaiter().GetResult();
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