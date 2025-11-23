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

        // ID local de la credencial (para relaciones locales)
        public int CredencialId { get; set; }

        // ID de API de la credencial (GUID para API)
        [SQLite.Column("credencial_id_api")]
        public string? CredencialIdApi { get; set; }

        [SQLite.Column("idApi")]
        public string? idApi { get; set; }

        [Ignore]
        public Credencial? Credencial { get; set; }

        // ID local del espacio (entero para relaciones locales)
        [SQLite.Column("espacio_id")]
        public int EspacioId { get; set; }

        // ID de API del espacio (GUID para API)
        [SQLite.Column("espacio_id_api")]
        public string? EspacioIdApi { get; set; }

        [Ignore]
        public Espacio? Espacio { get; set; }

        [Ignore]
        public AccesoTipo Resultado { get; set; }

        [Column("resultado")]
        public string ResultadoStr
        {
            get => Resultado.ToString();
            set => Resultado = Enum.Parse<AccesoTipo>(value);
        }

        public bool faltaCarga { get; set; } = false;
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