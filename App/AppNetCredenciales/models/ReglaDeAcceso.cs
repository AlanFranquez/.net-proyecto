using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AppNetCredenciales.models
{
    [SQLite.Table("ReglasDeAccesos")]
    public class ReglaDeAcceso
    {
        [PrimaryKey]
        [AutoIncrement]
        [SQLite.Column("id")]
        public int ReglaId { get; set; }

        [SQLite.Column("idApi")]
        public string? idApi { get; set; }

        [SQLite.Column("ventana_horaria")]
        public string? VentanaHoraria { get; set; }

        [SQLite.Column("vigencia_inicio")]
        public DateTime? VigenciaInicio { get; set; }

        [SQLite.Column("vigencia_fin")]
        public DateTime? VigenciaFin { get; set; }

        [SQLite.Column("prioridad")]
        public int Prioridad { get; set; }

        [Ignore]
        public AccesoTipo Politica { get; set; }

        [SQLite.Column("politica")]
        public string PoliticaStr
        {
            get => Politica.ToString();
            set => Politica = Enum.Parse<AccesoTipo>(value);
        }

        [SQLite.Column("requiere_biometria_confirmacion")]
        public bool RequiereBiometriaConfirmacion { get; set; }

        [SQLite.Column("rol")]
        public string? Rol { get; set; }

        // Para almacenar el array de espaciosIDs como JSON en SQLite
        [SQLite.Column("espacios_ids_json")]
        public string? EspaciosIDsJson { get; set; }

        [Ignore]
        public string[]? EspaciosIDs
        {
            get
            {
                if (string.IsNullOrWhiteSpace(EspaciosIDsJson))
                    return Array.Empty<string>();
                try
                {
                    return JsonSerializer.Deserialize<string[]>(EspaciosIDsJson) ?? Array.Empty<string>();
                }
                catch
                {
                    return Array.Empty<string>();
                }
            }
            set
            {
                EspaciosIDsJson = (value == null || value.Length == 0) ? null : JsonSerializer.Serialize(value);
            }
        }

        // Propiedades heredadas que ya no necesitas
        public string? ObjetivoTipo { get; set; }
    }
}