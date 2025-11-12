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


        public string? Nombre { get; set; }
        public bool Activo { get; set; } = true;
        public EspacioTipo Tipo { get; set; }

        public int EspacioTipoInt
        {
            get => (int)Tipo;
            set => Tipo = (EspacioTipo)value;
        }

        public bool faltaCarga { get; set; }

        // API id may be null/empty; make it nullable
        public string? idApi { get; set; }

        // Properties referenced by XAML bindings — keep them nullable/defaults to avoid binding warnings.
        public string? Descripcion { get; set; }
        public string? Lugar { get; set; }
        public DateTime? Fecha { get; set; }
        public int Stock { get; set; }
        public int Disponible { get; set; }
        public bool Publicado { get; set; }

        // If some XAML binds Emitida to a Color property, expose a Color-typed property or convert in XAML.
        // Add a string representation as safe default; if XAML expects Color, change type accordingly.
        public string? Emitida { get; set; }

    }
}
