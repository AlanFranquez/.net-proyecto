using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppNetCredenciales.models
{

    public enum AccesoTipo
    {
        Permitir,
        Denegar
    }

    public enum Modo
    {
        Online = 0,
        Offline = 1
    }

    [SQLite.Table("EventoAccesos")]
    public class EventoAcceso
    {

        [PrimaryKey]
        [AutoIncrement]
        [SQLite.Column("id")]
        public int EventoId { get; set; }
        public DateTime MomentoDeAcceso { get; set; }
        
        public int CredencialId { get; set; }

        public string CredencialIdApi { get; set; }

        [SQLite.Column("idApi")]
        public string idApi { get; set; }

        [Ignore]        
        public Credencial Credencial { get; set; } = default!;
        public int EspacioId { get; set; }

        public string? EspacioIdApi { get; set; }

        [Ignore]
        public Espacio Espacio { get; set; } = default!;

        [Ignore]
        public AccesoTipo Resultado { get; set; }

        [Column("resultado")]
        public string ResultadoStr
        {
            get => Resultado.ToString();
            set => Resultado = Enum.Parse<AccesoTipo>(value);
        }

        public string? Motivo { get; set; }

        [Ignore]
        public Modo Modo { get; set; }

        public string ModoStr
        {
            get => Modo.ToString();
            set => Modo = Enum.Parse<Modo>(value);
        }

        public string? Firma { get; set; }
    }
}
