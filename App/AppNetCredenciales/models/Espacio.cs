using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppNetCredenciales.models
{
    [SQLite.Table("Espacios")]
    public class Espacio
    {
        [PrimaryKey]
        [AutoIncrement]
        [SQLite.Column("id")]
        public int EspacioId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Lugar { get; set; } = string.Empty;
        public int Stock { get; set; } = 0;
        public bool Disponible { get; set; } = false;
        public bool Publicado { get; set; } = false;

    }
}
