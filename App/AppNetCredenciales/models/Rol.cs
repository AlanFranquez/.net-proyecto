using SQLite;
using System;
using System.Text.Json;

namespace AppNetCredenciales.models
{
    [SQLite.Table("Roles")]
    public class Rol
    {
        [PrimaryKey]
        [AutoIncrement]
        public int RolId { get; set; }

        public string Tipo { get; set; } = default!;

        public int Prioridad { get; set; }

        [SQLite.Column("idApi")]
        public string idApi { get; set; }

        public DateTime FechaAsignado { get; set; }

        [SQLite.Column("usuariosIDsJson")]
        public string UsuariosIDsJson { get; set; }

        [Ignore]
        public string[] UsuariosIDs
        {
            get
            {
                if (string.IsNullOrWhiteSpace(UsuariosIDsJson))
                    return Array.Empty<string>();
                try
                {
                    return JsonSerializer.Deserialize<string[]>(UsuariosIDsJson) ?? Array.Empty<string>();
                }
                catch
                {
                    return Array.Empty<string>();
                }
            }
            set
            {
                UsuariosIDsJson = (value == null || value.Length == 0) ? null : JsonSerializer.Serialize(value);
            }
        }
    }
}