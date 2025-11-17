using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AppNetCredenciales.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public ApiService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://c4e0724f9aba.ngrok-free.app/api/")
            };
        }

        public async Task<List<EventoAccesoDto>> GetEventosAccesoAsync() {             
            try
            {
                var list = await _httpClient.GetFromJsonAsync<List<EventoAccesoDto>>("eventos", _jsonOptions)
                           ?? new List<EventoAccesoDto>();
                foreach (var item in list)
                {
                    System.Diagnostics.Debug.WriteLine($"[ApiService] eventoAccesoId={item.EventoAccesoId} momentoDeAcceso={item.MomentoDeAcceso}");
                }
                return list;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ApiService] Error fetching eventosAcceso: {ex}");
                return new List<EventoAccesoDto>();
            }
        }

        public async Task<EventoAccesoDto?> CreateEventoAccesoAsync(EventoAccesoDto nuevo)
        {
            try
            {
                // ✅ Asegurar que la fecha esté en UTC antes de enviar
                if (nuevo.MomentoDeAcceso.Kind != DateTimeKind.Utc)
                {
                    nuevo.MomentoDeAcceso = nuevo.MomentoDeAcceso.ToUniversalTime();
                    System.Diagnostics.Debug.WriteLine($"[ApiService] Converted MomentoDeAcceso to UTC: {nuevo.MomentoDeAcceso:yyyy-MM-dd HH:mm:ss} UTC");
                }

                // ✅ Crear DTO solo con los campos requeridos para envío
                var requestDto = new
                {
                    momentoDeAcceso = nuevo.MomentoDeAcceso,
                    credencialId = nuevo.CredencialId,
                    espacioId = nuevo.EspacioId,
                    resultado = nuevo.Resultado,
                    motivo = nuevo.Motivo ?? "Acceso procesado",
                    modo = nuevo.Modo ?? "Online",
                    firma = nuevo.Firma ?? ""
                };

                // Debug del objeto que se va a enviar
                var requestJson = JsonSerializer.Serialize(requestDto, _jsonOptions);
                System.Diagnostics.Debug.WriteLine($"[ApiService] Sending request JSON: {requestJson}");

                var resp = await _httpClient.PostAsJsonAsync("eventos", requestDto, _jsonOptions);
                var content = await resp.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"[ApiService] Response status: {(int)resp.StatusCode} {resp.ReasonPhrase}");
                System.Diagnostics.Debug.WriteLine($"[ApiService] Response content: {content}");

                if (!resp.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"[ApiService] CreateEventoAccesoAsync failed: {resp.StatusCode} content={content}");
                    return null;
                }

                if (string.IsNullOrWhiteSpace(content))
                {
                    System.Diagnostics.Debug.WriteLine("[ApiService] Empty response content");
                    return null;
                }

                // ✅ Intentar deserializar la respuesta
                try
                {
                    var responseDto = JsonSerializer.Deserialize<EventoAccesoDto>(content, _jsonOptions);
                    if (responseDto != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"[ApiService] ✅ Successfully deserialized response");
                        return responseDto;
                    }
                }
                catch (JsonException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ApiService] Deserialization failed: {ex.Message}");
                }

                // ✅ Fallback: Si el API devuelve solo un ID como string
                var trimmed = content.Trim();
                if (trimmed.StartsWith("\"") && trimmed.EndsWith("\""))
                    trimmed = trimmed.Substring(1, trimmed.Length - 2);

                // Crear respuesta con los datos originales más el ID devuelto
                return new EventoAccesoDto
                {
                    EventoAccesoId = trimmed,
                    Id = trimmed,
                    MomentoDeAcceso = nuevo.MomentoDeAcceso,
                    CredencialId = nuevo.CredencialId,
                    EspacioId = nuevo.EspacioId,
                    Resultado = nuevo.Resultado,
                    Motivo = nuevo.Motivo,
                    Modo = nuevo.Modo,
                    Firma = nuevo.Firma
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ApiService] CreateEventoAccesoAsync error: {ex}");
                return null;
            }
        }

        public async Task<List<RolDto>>GetRolesAsync()
        {
            try
            {
                var list = await _httpClient.GetFromJsonAsync<List<RolDto>>("roles", _jsonOptions)
                           ?? new List<RolDto>();
                foreach (var item in list)
                {
                    System.Diagnostics.Debug.WriteLine($"[ApiService] rolId={item.RolId} tipo={item.Tipo}");
                }
                return list;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ApiService] Error fetching roles: {ex}");
                return new List<RolDto>();
            }
        }

        // Credencial
        public async Task<List<CredentialDto>> GetCredencialesAsync()
        {
            try
            {
                var list = await _httpClient.GetFromJsonAsync<List<CredentialDto>>("credenciales", _jsonOptions)
                           ?? new List<CredentialDto>();
                foreach (var item in list)
                {
                    System.Diagnostics.Debug.WriteLine($"[ApiService] credencialId={item.CredencialId} tipo={item.Tipo}");
                }
                return list;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ApiService] Error fetching credenciales: {ex}");
                return new List<CredentialDto>();
            }
        }

        public async Task<string?> crearCredencial(CredentialDto crd)
        {
            try
            {
                var resp = await _httpClient.PostAsJsonAsync("credenciales", crd, _jsonOptions);
                var content = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"[ApiService] crearCredencial failed: {resp.StatusCode} content={content}");
                    return null;
                }

                if (string.IsNullOrWhiteSpace(content))
                    return null;

                try
                {
                    var dto = JsonSerializer.Deserialize<CredentialDto>(content, _jsonOptions);
                    if (dto != null && !string.IsNullOrWhiteSpace(dto.CredencialId))
                        return dto.CredencialId;
                }
                catch (JsonException) { /* ignore and try other parsing strategies */ }

               
                try
                {
                    using var doc = JsonDocument.Parse(content);
                    var root = doc.RootElement;

                    if (root.ValueKind == JsonValueKind.Object)
                    {
                        if (root.TryGetProperty("credencialId", out var p) && p.ValueKind != JsonValueKind.Null)
                            return p.ValueKind == JsonValueKind.String ? p.GetString() : p.GetRawText();

                        if (root.TryGetProperty("id", out var p2) && p2.ValueKind != JsonValueKind.Null)
                            return p2.ValueKind == JsonValueKind.String ? p2.GetString() : p2.GetRawText();
                    }
                    else if (root.ValueKind == JsonValueKind.String)
                    {
                        return root.GetString();
                    }
                    else if (root.ValueKind == JsonValueKind.Number)
                    {
                        return root.GetRawText();
                    }
                }
                catch (JsonException) { /* malformed -> fallback */ }

                var trimmed = content.Trim();
                if (trimmed.StartsWith("\"") && trimmed.EndsWith("\"") && trimmed.Length >= 2)
                    trimmed = trimmed.Substring(1, trimmed.Length - 2);

                return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ApiService] crearCredencial error: {ex}");
                return null;
            }
        }
        // Espacios

        public async Task<List<EspacioDto>> GetEspaciosAsync()
        {
            try
            {
                var list = await _httpClient.GetFromJsonAsync<List<EspacioDto>>("espacios", _jsonOptions)
                           ?? new List<EspacioDto>();
                foreach (var item in list)
                {
                    System.Diagnostics.Debug.WriteLine($"[ApiService] espacioId={item.EspacioId} nombre={item.Nombre}");
                }
                return list;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ApiService] Error fetching espacios: {ex}");
                return new List<EspacioDto>();
            }
        }


        public async Task<List<UsuarioDto>> GetUsuariosAsync()
        {
            try
            {
                var list = await _httpClient.GetFromJsonAsync<List<UsuarioDto>>("usuarios", _jsonOptions)
                           ?? new List<UsuarioDto>();

                foreach (var item in list)
                {
                    System.Diagnostics.Debug.WriteLine($"[ApiService] usuarioId={item.UsuarioId} email={item.Email}");
                }

                return list;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ApiService] Error fetching usuarios: {ex}");
                return new List<UsuarioDto>();
            }
        }

        public async Task<string?> GetUsuariosRawAsync()
        {
            try
            {
                var resp = await _httpClient.GetAsync("usuarios");
                return await resp.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                return $"ERROR: {ex.Message}";
            }
        }

        public async Task<UsuarioDto?> CreateUsuarioAsync(NewUsuarioDto nuevo)
        {
            try
            {
                var requestJson = JsonSerializer.Serialize(nuevo, _jsonOptions);
                System.Diagnostics.Debug.WriteLine($"[ApiService] CreateUsuarioAsync -> request JSON: {requestJson}");

                using var req = new HttpRequestMessage(HttpMethod.Post, "usuarios/registro")
                {
                    Content = JsonContent.Create(nuevo, options: _jsonOptions)
                };

                var resp = await _httpClient.SendAsync(req);
                var content = await resp.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"[ApiService] CreateUsuarioAsync response status: {(int)resp.StatusCode} {resp.ReasonPhrase}");
                System.Diagnostics.Debug.WriteLine($"[ApiService] CreateUsuarioAsync response body: {content}");

                if (!resp.IsSuccessStatusCode)
                {
                   
                    return null;
                }

                if (string.IsNullOrWhiteSpace(content))
                    return null;

                try
                {
                    var dto = JsonSerializer.Deserialize<UsuarioDto>(content, _jsonOptions);
                    if (dto != null)
                        return dto;
                }
                catch (JsonException) { /* fallthrough to flexible parsing */ }

                try
                {
                    using var doc = JsonDocument.Parse(content);
                    var root = doc.RootElement;

                    if (root.ValueKind == JsonValueKind.Object)
                    {
                        if (root.TryGetProperty("usuarioId", out var p) && p.ValueKind != JsonValueKind.Null)
                            return new UsuarioDto { UsuarioId = p.GetString() };

                        if (root.TryGetProperty("id", out var p2) && p2.ValueKind != JsonValueKind.Null)
                            return new UsuarioDto { UsuarioId = p2.ValueKind == JsonValueKind.String ? p2.GetString() : p2.GetRawText() };
                    }
                    else if (root.ValueKind == JsonValueKind.String)
                    {
                        return new UsuarioDto { UsuarioId = root.GetString() };
                    }
                    else if (root.ValueKind == JsonValueKind.Number)
                    {
                        return new UsuarioDto { UsuarioId = root.GetRawText() };
                    }
                }
                catch (JsonException e) { 
                    Debug.WriteLine($" {e.ToString()} [ApiService] CreateUsuarioAsync: JSON parsing failed, falling back to raw content parsing.");

                }

                var trimmed = content.Trim();
                if (trimmed.StartsWith("\"") && trimmed.EndsWith("\"") && trimmed.Length >= 2)
                    trimmed = trimmed.Substring(1, trimmed.Length - 2);

                return new UsuarioDto { UsuarioId = trimmed };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ApiService] CreateUsuarioAsync error: {ex}");
                throw; 
            }
        }

        public class EspacioDto
        {
            [JsonPropertyName("id")]
            public string? EspacioId { get; set; }
            [JsonPropertyName("nombre")]
            public string? Nombre { get; set; }
            [JsonPropertyName("tipo")]
            public string? Tipo { get; set; }
            [JsonPropertyName("activo")]
            public bool Activo { get; set; }

            [JsonPropertyName("modo")]
            public string? Modo { get; set; }

            [JsonPropertyName("beneficiosIds")]
            public string[]? BeneficiosIds { get; set; }


        }


        public class RolDto
        {
            [JsonPropertyName("rolId")]
            public string? RolId { get; set; }

            [JsonPropertyName("tipo")]
            public string? Tipo { get; set; }

            [JsonPropertyName("prioridad")]
            public int Prioridad { get; set; }

            [JsonPropertyName("fechaAsignado")]
            public DateTime fechaAsignado { get; set; }

        }

        public class CredentialDto
        {
            [JsonPropertyName("credencialId")]
            public string? CredencialId { get; set; }
            [JsonPropertyName("tipo")]
            public string? Tipo { get; set; }
            [JsonPropertyName("estado")]
            public string? Estado { get; set; }
            [JsonPropertyName("fechaEmision")]
            public DateTime FechaEmision { get; set; }
            [JsonPropertyName("fechaExpiracion")]
            public DateTime? FechaExpiracion { get; set; }

            [JsonPropertyName("idCriptografico")]
            public string? IdCriptografico { get; set; }

            [JsonPropertyName("usuarioId")]
            public string? usuarioIdApi { get; set; }
        }

        public class UsuarioDto
        {
            [JsonPropertyName("usuarioId")]
            public string? UsuarioId { get; set; }

            [JsonPropertyName("nombre")]
            public string? Nombre { get; set; }

            [JsonPropertyName("apellido")]
            public string? Apellido { get; set; }

            [JsonPropertyName("email")]
            public string? Email { get; set; }

            [JsonPropertyName("password")]
            public string? Password { get; set; }

            [JsonPropertyName("documento")]
            public string? Documento { get; set; }

            [JsonPropertyName("rolesIDs")]
            public string[]? RolesIDs { get; set; }
        }

        public class EventoAccesoDto
        {
            // ❌ REMOVER: eventoAccesoId no está en la estructura requerida
            // [JsonPropertyName("eventoAccesoId")]
            // public string? EventoAccesoId { get; set; }

            [JsonPropertyName("momentoDeAcceso")]
            public DateTime MomentoDeAcceso { get; set; }

            [JsonPropertyName("credencialId")]
            public string? CredencialId { get; set; }

            [JsonPropertyName("espacioId")]
            public string? EspacioId { get; set; }

            [JsonPropertyName("resultado")]
            public string? Resultado { get; set; }

            [JsonPropertyName("motivo")]
            public string? Motivo { get; set; }

            [JsonPropertyName("modo")]
            public string? Modo { get; set; }

            [JsonPropertyName("firma")]
            public string? Firma { get; set; }

            // ✅ AGREGAR: Propiedades que pueden venir en la respuesta del API
            [JsonPropertyName("eventoAccesoId")]
            public string? EventoAccesoId { get; set; } // Solo para respuesta

            [JsonPropertyName("id")]
            public string? Id { get; set; } // Alias para respuesta
        }

        public class NewUsuarioDto
        {
            [JsonPropertyName("nombre")]
            public string Nombre { get; set; }

            [JsonPropertyName("apellido")]
            public string Apellido { get; set; }

            [JsonPropertyName("email")]
            public string Email { get; set; }

            [JsonPropertyName("documento")]
            public string Documento { get; set; }

            [JsonPropertyName("password")]
            public string Password { get; set; }
            [JsonPropertyName("rolesIDs")]
            public string[]? RolesIDs { get; set; }
        }

        
    }
}