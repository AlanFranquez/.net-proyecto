using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppNetCredenciales.models
{
    public class Rol
    {

        [PrimaryKey]
        [AutoIncrement]
        public int RolId { get; set; }
        public string Tipo { get; set; } = default!;
        public int Prioridad { get; set; }
        public DateTime FechaAsignado { get; set; }
    }
}
