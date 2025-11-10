using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
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

        [Ignore]
        public Rol Rol { get; set; }



    }
}
