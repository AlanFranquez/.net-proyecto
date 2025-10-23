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
            _connection.CreateTableAsync<models.Usuario>();
        }

        public async Task<List<models.Usuario>> GetUsuariosAsync()
        {
            return await _connection.Table<models.Usuario>().ToListAsync();
        }

        public async Task<int> SaveUsuarioAsync(models.Usuario usuario)
        {
            if (usuario.Id != 0)
            {
                return await _connection.UpdateAsync(usuario);
            }
            else
            {
                return await _connection.InsertAsync(usuario);
            }
        }

        public async Task<int> DeleteUsuarioAsync(models.Usuario usuario)
        {
            return await _connection.DeleteAsync(usuario);
        }

        public async Task<models.Usuario> GetUsuarioByIdAsync(int id)
        {
            return await _connection.Table<models.Usuario>()
                .Where(u => u.Id == id)
                .FirstOrDefaultAsync();
        }
    }
}
