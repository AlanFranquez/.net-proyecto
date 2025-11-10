using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppNetCredenciales.models {

    [SQLite.Table("EspacioReglasDeAccesos")]
    public class EspacioReglaDeAcceso
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int ReglaId { get; set; }

        [SQLite.Column("idApi")]
        public string idApi { get; set; }
        public int EspacioId { get; set; }
        
        [Ignore]
        public Espacio Espacio { get; set; } = default!;

        [Ignore]
        public ReglaDeAcceso Regla { get; set; } = default!;

    }
    

}