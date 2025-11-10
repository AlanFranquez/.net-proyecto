using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AppNetCredenciales.models
{
    [SQLite.Table("Usuarios")]
    public class Usuario
    {
        [PrimaryKey]
        [AutoIncrement]
        [SQLite.Column("id")]
        public int UsuarioId { get; set; }
        
        [SQLite.Column("documento")]
        public string Documento { get; set; }

        [SQLite.Column("Password")]
        public string Password { get; set; }

        [SQLite.Column("nombre")]
        public string Nombre { get; set; }

        [SQLite.Column("apellido")]
        public string Apellido { get; set; }

        [SQLite.Column("idApi")]
        public string idApi { get; set; }


        [SQLite.Column("email")]
        public string Email { get; set; }



        [SQLite.Column("falta_cargar")]
        public bool FaltaCargar { get; set; }

        [SQLite.Column("credencial_id")]
        public int CredencialId { get; set; }

        [Ignore]
        public Credencial Credencial { get; set; }

        [SQLite.Column("rol_id")]
        public int? RolId { get; set; }


        [SQLite.Column("rolesIDs")]
        public string RolesIDsJson { get; set; }

        [Ignore]
        public string[] RolesIDs
        {
            get
            {
                if (string.IsNullOrWhiteSpace(RolesIDsJson))
                    return Array.Empty<string>();
                try
                {
                    return JsonSerializer.Deserialize<string[]>(RolesIDsJson) ?? Array.Empty<string>();
                }
                catch
                {
                    return Array.Empty<string>();
                }
            }
            set
            {
                RolesIDsJson = (value == null || value.Length == 0) ? null : JsonSerializer.Serialize(value);
            }
        }

        [Ignore]
        public Rol Rol { get; set; }



    }
}
