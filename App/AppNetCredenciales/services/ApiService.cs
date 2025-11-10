using System;
using System.Collections.Generic;
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
                BaseAddress = new Uri("http://localhost:8080/api/")
            };
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

                // Intentar deserializar a DTO completo si el servidor devuelve el objeto
                try
                {
                    var dto = JsonSerializer.Deserialize<CredentialDto>(content, _jsonOptions);
                    if (dto != null && !string.IsNullOrWhiteSpace(dto.CredencialId))
                        return dto.CredencialId;
                }
                catch (JsonException) { /* ignore and try other parsing strategies */ }

                // Intentar parse flexible: { credencialId: "..."} o { id: "..." } o "123" o 123
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

                // Fallback: raw trimmed content (puede venir sin JSON)
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
                // Log request payload
                var requestJson = JsonSerializer.Serialize(nuevo, _jsonOptions);
                System.Diagnostics.Debug.WriteLine($"[ApiService] CreateUsuarioAsync -> request JSON: {requestJson}");

                // Build request explicitly so podemos inspeccionar headers y body
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
                    // return null but leave the details in debug output; caller can inspect logs
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
                catch (JsonException) { /* malformed or not-json -> fallback */ }

                var trimmed = content.Trim();
                if (trimmed.StartsWith("\"") && trimmed.EndsWith("\"") && trimmed.Length >= 2)
                    trimmed = trimmed.Substring(1, trimmed.Length - 2);

                return new UsuarioDto { UsuarioId = trimmed };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ApiService] CreateUsuarioAsync error: {ex}");
                throw; // rethrow so you can see exception in debugger call stack
            }
        }

        public class EspacioDto
        {
            [JsonPropertyName("espacioId")]
            public string? EspacioId { get; set; }
            [JsonPropertyName("nombre")]
            public string? Nombre { get; set; }
            [JsonPropertyName("tipo")]
            public string? Tipo { get; set; }
            [JsonPropertyName("activo")]
            public bool Activo { get; set; }

            


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
        }

        
    }
}