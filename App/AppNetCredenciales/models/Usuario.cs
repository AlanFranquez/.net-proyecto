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
        public int Id { get; set; }

        [SQLite.Column("nombre")]
        public string Nombre { get; set; }
        
        [SQLite.Column("apellido")]
        public string Apellido { get; set; }

        [SQLite.Column("email")]
        public string email { get; set; }


    }
}
