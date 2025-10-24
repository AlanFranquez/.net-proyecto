using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppNetCredenciales.models
{

    [SQLite.Table("RolUsuarios")]
    public class UsuarioRol
    {

        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }
        public int UsuarioId { get; set; }

        [Ignore]
        public Usuario Usuario { get; set; } = default!;

        public int RolId { get; set; }


        [Ignore]
        public Rol Rol { get; set; } = default!;

        public DateTime FechaAsignado { get; set; }
    }
}
