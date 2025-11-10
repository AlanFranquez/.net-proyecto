using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppNetCredenciales.models
{

    public enum CredencialEstado
    {
        Emitida,
        Activada,
        Suspendida,
        Expirada
    }

    public enum CredencialTipo
    {
        Campus,
        Empresa
    }
    [SQLite.Table("Credenciales")]
    public class Credencial
    {
        [PrimaryKey]
        [AutoIncrement]
        [SQLite.Column("id")]
        public int CredencialId { get; set; }

        [SQLite.Column("idApi")]
        public string idApi { get; set; }


        [Column("tipo")]
        public string TipoStr {
            get => Tipo.ToString();
            set => Tipo = Enum.Parse<CredencialTipo>(value);
        }

        

        [Ignore]
        public CredencialTipo Tipo { get; set; }
        
        [Column("estado")]
        public string EstadoStr
        {
            get => Estado.ToString();
            set => Estado = Enum.Parse<CredencialEstado>(value);
        }

        [Ignore]
        public CredencialEstado Estado { get; set; }
        public string? IdCriptografico { get; set; }

        public bool FaltaCarga { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime? FechaExpiracion { get; set; }

        public string usuarioIdApi { get; set; }



        }
}
