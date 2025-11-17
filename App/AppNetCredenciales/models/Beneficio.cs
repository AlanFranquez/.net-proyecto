using SQLite;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AppNetCredenciales.models
{
    

    [SQLite.Table("Beneficios")]
    public class Beneficio
    {
        [PrimaryKey]
        [AutoIncrement]
        [SQLite.Column("id")]
        public int BeneficioId { get; set; }

        [SQLite.Column("idApi")]
        public string? idApi { get; set; }

        [SQLite.Column("nombre")]
        public string? Nombre { get; set; }

        [SQLite.Column("descripcion")]
        public string? Descripcion { get; set; }

        [SQLite.Column("vigencia_inicio")]
        public DateTime VigenciaInicio { get; set; }

        [SQLite.Column("vigencia_fin")]
        public DateTime VigenciaFin { get; set; }

        [SQLite.Column("cupo_total")]
        public int? CupoTotal { get; set; }

        [SQLite.Column("requiere_biometria")]
        public bool? RequiereBiometria { get; set; }


        [SQLite.Column("activo")]
        public bool Activo { get; set; } = true;

        [SQLite.Column("falta_carga")]
        public bool FaltaCarga { get; set; } = false;

        public int? CupoPorUsuario { get; set; }

        public string Tipo { get; set; }

        // Fix: Convert string arrays to JSON for SQLite storage
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

        [SQLite.Column("usuarios_ids_json")]
        public string? UsuariosIDsJson { get; set; }

        [Ignore]
        public string[]? UsuariosIDs
        {
            get
            {
                if (string.IsNullOrWhiteSpace(UsuariosIDsJson))
                    return Array.Empty<string>();
                try
                {
                    return JsonSerializer.Deserialize<string[]>(UsuariosIDsJson) ?? Array.Empty<string>();
                }
                catch
                {
                    return Array.Empty<string>();
                }
            }
            set
            {
                UsuariosIDsJson = (value == null || value.Length == 0) ? null : JsonSerializer.Serialize(value);
            }
        }

        

        [Ignore]
        public bool EstaVigente
        {
            get
            {
                var ahora = DateTime.Now;
                return ahora >= VigenciaInicio && ahora <= VigenciaFin && Activo;
            }
        }

   

    }
}