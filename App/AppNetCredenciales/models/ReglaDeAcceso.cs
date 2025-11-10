using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppNetCredenciales.models
{

    [SQLite.Table("ReglasDeAccesos")]
    public class ReglaDeAcceso
    {
        public int ReglaId { get; set; }
        public string? ObjetivoTipo { get; set; }
        public string? VentanaHoraria { get; set; }

        [SQLite.Column("idApi")]
        public string idApi { get; set; }
        public DateTime? VigenciaInicio { get; set; }
        public DateTime? VigenciaFin { get; set; }
        public int Prioridad { get; set; }

        [Ignore]
        public AccesoTipo Politica { get; set; }

        public string PoliticaStr
        {
            get => Politica.ToString();
            set => Politica = Enum.Parse<AccesoTipo>(value);
        }
        public bool RequiereBiometriaConfirmacion { get; set; }
    }
}
