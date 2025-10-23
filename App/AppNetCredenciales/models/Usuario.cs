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

        [SQLite.Column("email")]
        public string Email { get; set; }
        //public UsuarioEstado Estado { get; set; }
        //public ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();

        //public ICollection<Dispositivo> Dispositivos { get; set; } = new List<Dispositivo>();
        //public ICollection<BeneficioUsuario> Beneficios { get; set; } = new List<BeneficioUsuario>();
        //public ICollection<Canje> Canjes { get; set; } = new List<Canje>();


    }
}
