using SQLite;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AppNetCredenciales.models
{
    public enum BeneficioTipo
    {
        Descuento = 0,
        Promocion = 1,
        Regalo = 2,
        Puntos = 3,
        Acceso = 4,
        Otro = 5
    }

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
        public int CupoTotal { get; set; }

        [SQLite.Column("cupo_usado")]
        public int CupoUsado { get; set; } = 0;

        [SQLite.Column("activo")]
        public bool Activo { get; set; } = true;

        [SQLite.Column("falta_carga")]
        public bool FaltaCarga { get; set; } = false;

        // Enum handling
        [Ignore]
        public BeneficioTipo Tipo { get; set; }

        [Column("tipo")]
        public string TipoStr
        {
            get => Tipo.ToString();
            set => Tipo = Enum.TryParse<BeneficioTipo>(value, true, out var result) ? result : BeneficioTipo.Otro;
        }

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

        // Computed properties
        [Ignore]
        public int CupoDisponible => CupoTotal - CupoUsado;

        [Ignore]
        public bool EstaVigente
        {
            get
            {
                var ahora = DateTime.Now;
                return ahora >= VigenciaInicio && ahora <= VigenciaFin && Activo;
            }
        }

        [Ignore]
        public bool TieneCupo => CupoDisponible > 0 || CupoTotal == 0;

        [Ignore]
        public bool EstaDisponible => EstaVigente && TieneCupo;
    }
}