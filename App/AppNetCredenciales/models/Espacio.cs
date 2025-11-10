using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;

namespace AppNetCredenciales.models
{

    public enum EspacioTipo
    {
        Aula = 0,
        Laboratorio = 1,
        Biblioteca = 2,
        Gimnasio = 3,
        Auditorio = 4,
        Otro = 5
    }


    [SQLite.Table("Espacios")]
    public class Espacio
    {

        [PrimaryKey]
        [AutoIncrement]
        [SQLite.Column("id")]
        public int EspacioId { get; set; }
        public string Nombre { get; set; }
        public bool Activo { get; set; } = true;
        public EspacioTipo Tipo { get; set; }

        public int EspacioTipoInt
        {
            get => (int)Tipo;
            set => Tipo = (EspacioTipo)value;
        }


        public bool faltaCarga { get; set; }
        
        public string idApi { get; set; }




    }
}
