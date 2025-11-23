using AppNetCredenciales.models;
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
                BaseAddress = new Uri("https://0b14a40e43cb.ngrok-free.app/api/")
            };
        }

        public async Task<List<ReglaAccesoDto>> GetReglasAccesoAsync()
        {
            try
            {
                var list = await _httpClient.GetFromJsonAsync<List<ReglaAccesoDto>>("reglas", _jsonOptions) ?? new List<ReglaAccesoDto>();

                

                return list;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ApiService] Error fetching eventosAcceso: {ex}");
                return new List<ReglaAccesoDto>();
            }
        }

        public async Task<BeneficioDto?> CreateBeneficioAsync(BeneficioDto beneficio)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[ApiService] CreateBeneficioAsync -> beneficioId: {beneficio.Id}");

                var response = await _httpClient.PostAsJsonAsync("beneficios", beneficio, _jsonOptions);
                var content = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"[ApiService] CreateBeneficioAsync response status: {(int)response.StatusCode} {response.ReasonPhrase}");
                System.Diagnostics.Debug.WriteLine($"[ApiService] CreateBeneficioAsync response body: {content}");

                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"[ApiService] CreateBeneficioAsync failed: {response.StatusCode} content={content}");
                    return null;
                }

                if (!string.IsNullOrWhiteSpace(content))
                {
                    try
                    {
                        var nuevoBeneficio = JsonSerializer.Deserialize<BeneficioDto>(content, _jsonOptions);
                        if (nuevoBeneficio != null)
                        {
                            return nuevoBeneficio;
                        }
                    }
                    catch (JsonException ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[ApiService] CreateBeneficioAsync deserialization failed: {ex.Message}");
                    }
                }


                return beneficio;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ApiService] CreateBeneficioAsync error: {ex.Message}");
                return null;
            }
        }

        public async Task<BeneficioDto?> CanjearBeneficio(CanjeDto canje)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[ApiService] CanjearBeneficio -> beneficioId: {canje.beneficioId}, usuarioId: {canje.usuarioId}");

                var response = await _httpClient.PostAsJsonAsync($"beneficios/{canje.beneficioId}/canjear", canje, _jsonOptions);
                var content = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"[ApiService] CanjearBeneficio response status: {(int)response.StatusCode} {response.ReasonPhrase}");
                System.Diagnostics.Debug.WriteLine($"[ApiService] CanjearBeneficio response body: {content}");

                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"[ApiService] CanjearBeneficio failed: {response.StatusCode} content={content}");
                    return null;
                }


                if (!string.IsNullOrWhiteSpace(content))
                {
                    try
                    {
                        var updatedBeneficio = JsonSerializer.Deserialize<BeneficioDto>(content, _jsonOptions);
                        if (updatedBeneficio != null)
                        {
                            return updatedBeneficio;
                        }
                    }
                    catch (JsonException ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[ApiService] CanjearBeneficio deserialization failed: {ex.Message}");
                    }
                }

                // Fallback: fetch the updated benefit data
                var getResponse = await _httpClient.GetFromJsonAsync<BeneficioDto>($"beneficios/{canje.beneficioId}", _jsonOptions);
                return getResponse;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ApiService] CanjearBeneficio error: {ex.Message}");
                return null;
            }
        }

        public async Task<BeneficioDto?> UpdateBeneficioAsync(BeneficioDto beneficio)
        {
            try
            {

                foreach (var u in beneficio.UsuariosIDs)
                {
                    Debug.WriteLine($"USUARIO ID EN BENEFICIO: {u}");
                }
                var requestJson = JsonSerializer.Serialize(beneficio, _jsonOptions);

                var response = await _httpClient.PutAsJsonAsync($"beneficios/{beneficio.Id}", beneficio, _jsonOptions);
                var content = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"[ApiService] UpdateBeneficioAsync response status: {(int)response.StatusCode} {response.ReasonPhrase}");
                System.Diagnostics.Debug.WriteLine($"[ApiService] UpdateBeneficioAsync response body: {content}");

                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"[ApiService] UpdateBeneficioAsync failed: {response.StatusCode} content={content}");
                    return null;
                }

                if (string.IsNullOrWhiteSpace(content))
                    return beneficio;


                try
                {
                    var updatedBeneficio = JsonSerializer.Deserialize<BeneficioDto>(content, _jsonOptions);
                    return updatedBeneficio ?? beneficio;
                }
                catch (JsonException ex)
                {
                    Debug.WriteLine($"ERROR - {ex.Message}");
                    return beneficio;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ApiService] UpdateBeneficioAsync error: {ex}");
                return null;
            }
        }

        public async Task<List<BeneficioDto>> GetBeneficiosAsync()
        {
            try
            {
                var list = await _httpClient.GetFromJsonAsync<List<BeneficioDto>>("beneficios", _jsonOptions) ?? new List<BeneficioDto>();

                foreach (var item in list)
                {
                    Debug.WriteLine($"BENEFICIO => {item.Id} - {item.Tipo}");
                }

                return list;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ApiService] Error fetching eventosAcceso: {ex}");
                return new List<BeneficioDto>();
            }
        }

        public async Task<List<EventoAccesoDto>> GetEventosAccesoAsync()
        {
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
                if (nuevo.MomentoDeAcceso.Kind != DateTimeKind.Utc)
                {
                    nuevo.MomentoDeAcceso = nuevo.MomentoDeAcceso.ToUniversalTime();
                    System.Diagnostics.Debug.WriteLine($"[ApiService] Converted MomentoDeAcceso to UTC: {nuevo.MomentoDeAcceso:yyyy-MM-dd HH:mm:ss} UTC");
                }

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

        public async Task<List<RolDto>> GetRolesAsync()
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
                System.Diagnostics.Debug.WriteLine($"[ApiService] === ENVIANDO PETICIÓN ===");
                System.Diagnostics.Debug.WriteLine($"[ApiService] URL: {_httpClient.BaseAddress}usuarios/registro");
                System.Diagnostics.Debug.WriteLine($"[ApiService] CreateUsuarioAsync -> request JSON: {requestJson}");

                using var req = new HttpRequestMessage(HttpMethod.Post, "usuarios/registro")
                {
                    Content = JsonContent.Create(nuevo, options: _jsonOptions)
                };

                // Agregar headers de debug
                System.Diagnostics.Debug.WriteLine("[ApiService] === HEADERS DE LA PETICIÓN ===");
                foreach (var header in req.Headers)
                {
                    System.Diagnostics.Debug.WriteLine($"[ApiService] {header.Key}: {string.Join(", ", header.Value)}");
                }
                if (req.Content?.Headers != null)
                {
                    foreach (var header in req.Content.Headers)
                    {
                        System.Diagnostics.Debug.WriteLine($"[ApiService] Content-{header.Key}: {string.Join(", ", header.Value)}");
                    }
                }

                var resp = await _httpClient.SendAsync(req);
                var content = await resp.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"[ApiService] === RESPUESTA DEL SERVIDOR ===");
                System.Diagnostics.Debug.WriteLine($"[ApiService] Status Code: {(int)resp.StatusCode} {resp.ReasonPhrase}");
                System.Diagnostics.Debug.WriteLine($"[ApiService] Response Headers:");
                foreach (var header in resp.Headers)
                {
                    System.Diagnostics.Debug.WriteLine($"[ApiService]   {header.Key}: {string.Join(", ", header.Value)}");
                }
                System.Diagnostics.Debug.WriteLine($"[ApiService] Response Body: '{content}'");
                System.Diagnostics.Debug.WriteLine($"[ApiService] Response Body Length: {content?.Length ?? 0} caracteres");

                if (!resp.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"[ApiService] ❌ ERROR: {resp.StatusCode}");

                    // Intentar parsear el error del servidor
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        try
                        {
                            using var errorDoc = JsonDocument.Parse(content);
                            System.Diagnostics.Debug.WriteLine($"[ApiService] Error JSON: {errorDoc.RootElement}");
                        }
                        catch
                        {
                            System.Diagnostics.Debug.WriteLine($"[ApiService] Error como texto: {content}");
                        }
                    }

                    return null;
                }

                if (string.IsNullOrWhiteSpace(content))
                {
                    System.Diagnostics.Debug.WriteLine("[ApiService] ⚠️ Respuesta vacía del servidor");
                    return null;
                }

                // Intentar deserializar como UsuarioDto completo
                try
                {
                    System.Diagnostics.Debug.WriteLine("[ApiService] Intentando deserializar como UsuarioDto...");
                    var dto = JsonSerializer.Deserialize<UsuarioDto>(content, _jsonOptions);
                    if (dto != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"[ApiService] ✅ Deserializado exitosamente: UsuarioId={dto.UsuarioId}");
                        return dto;
                    }
                }
                catch (JsonException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ApiService] ❌ Fallo deserialización UsuarioDto: {ex.Message}");
                }

                // Intentar parsing flexible
                try
                {
                    System.Diagnostics.Debug.WriteLine("[ApiService] Intentando parsing flexible...");
                    using var doc = JsonDocument.Parse(content);
                    var root = doc.RootElement;

                    System.Diagnostics.Debug.WriteLine($"[ApiService] JSON Root ValueKind: {root.ValueKind}");

                    if (root.ValueKind == JsonValueKind.Object)
                    {
                        System.Diagnostics.Debug.WriteLine("[ApiService] Es un objeto JSON, buscando propiedades...");

                        // Listar todas las propiedades
                        foreach (var property in root.EnumerateObject())
                        {
                            System.Diagnostics.Debug.WriteLine($"[ApiService] Propiedad: {property.Name} = {property.Value}");
                        }

                        if (root.TryGetProperty("usuarioId", out var p) && p.ValueKind != JsonValueKind.Null)
                        {
                            var id = p.ValueKind == JsonValueKind.String ? p.GetString() : p.GetRawText();
                            System.Diagnostics.Debug.WriteLine($"[ApiService] ✅ Encontrado usuarioId: {id}");
                            return new UsuarioDto { UsuarioId = id };
                        }

                        if (root.TryGetProperty("id", out var p2) && p2.ValueKind != JsonValueKind.Null)
                        {
                            var id = p2.ValueKind == JsonValueKind.String ? p2.GetString() : p2.GetRawText();
                            System.Diagnostics.Debug.WriteLine($"[ApiService] ✅ Encontrado id: {id}");
                            return new UsuarioDto { UsuarioId = id };
                        }
                    }
                    else if (root.ValueKind == JsonValueKind.String)
                    {
                        var id = root.GetString();
                        System.Diagnostics.Debug.WriteLine($"[ApiService] ✅ String directo: {id}");
                        return new UsuarioDto { UsuarioId = id };
                    }
                    else if (root.ValueKind == JsonValueKind.Number)
                    {
                        var id = root.GetRawText();
                        System.Diagnostics.Debug.WriteLine($"[ApiService] ✅ Número: {id}");
                        return new UsuarioDto { UsuarioId = id };
                    }
                }
                catch (JsonException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ApiService] ❌ Fallo parsing flexible: {ex.Message}");
                }

                // Último intento: texto crudo
                var trimmed = content.Trim();
                if (trimmed.StartsWith("\"") && trimmed.EndsWith("\"") && trimmed.Length >= 2)
                    trimmed = trimmed.Substring(1, trimmed.Length - 2);

                System.Diagnostics.Debug.WriteLine($"[ApiService] ⚠️ Fallback: usando texto crudo '{trimmed}'");
                return new UsuarioDto { UsuarioId = trimmed };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ApiService] ❌ EXCEPCIÓN GENERAL:");
                System.Diagnostics.Debug.WriteLine($"[ApiService] Mensaje: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[ApiService] StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<RolDto?> UpdateRolAsync(UpdateRolDto rol)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[ApiService] UpdateRolAsync -> rolId: {rol.RolId}");

                var response = await _httpClient.PutAsJsonAsync($"roles/{rol.RolId}", rol, _jsonOptions);
                var content = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"[ApiService] UpdateRolAsync response status: {(int)response.StatusCode} {response.ReasonPhrase}");
                System.Diagnostics.Debug.WriteLine($"[ApiService] UpdateRolAsync response body: {content}");

                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"[ApiService] UpdateRolAsync failed: {response.StatusCode} content={content}");
                    return null;
                }

                if (!string.IsNullOrWhiteSpace(content))
                {
                    try
                    {
                        var rolActualizado = JsonSerializer.Deserialize<RolDto>(content, _jsonOptions);
                        return rolActualizado;
                    }
                    catch (JsonException ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[ApiService] UpdateRolAsync deserialization failed: {ex.Message}");
                    }
                }

                return new RolDto { RolId = rol.RolId }; // Fallback
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ApiService] UpdateRolAsync error: {ex.Message}");
                return null;
            }
        }

        public class UpdateRolDto
        {
            [JsonPropertyName("rolId")]
            public string RolId { get; set; }

            [JsonPropertyName("tipo")]
            public string Tipo { get; set; }

            [JsonPropertyName("prioridad")]
            public int Prioridad { get; set; }

            [JsonPropertyName("fechaAsignado")]
            public DateTime FechaAsignado { get; set; }

            [JsonPropertyName("usuariosIDs")]
            public string[] UsuariosIDs { get; set; }
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

            [JsonPropertyName("reglaIds")]
            public string[]? ReglasIds { get; set; }


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

            [JsonPropertyName("usuariosIDs")]
            public string[]? usuariosIDs { get; set; }

        }

        public class UpdateBeneficioDto
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }

            [JsonPropertyName("tipo")]
            public string Tipo { get; set; }

            [JsonPropertyName("nombre")]
            public string Nombre { get; set; }

            [JsonPropertyName("descripcion")]
            public string Descripcion { get; set; }

            [JsonPropertyName("vigenciaInicio")]
            public DateTime VigenciaInicio { get; set; }

            [JsonPropertyName("vigenciaFin")]
            public DateTime VigenciaFin { get; set; }

            [JsonPropertyName("cupoTotal")]
            public int CupoTotal { get; set; }

            [JsonPropertyName("cupoPorUsuario")]
            public int CupoPorUsuario { get; set; }

            [JsonPropertyName("requiereBiometria")]
            public bool RequiereBiometria { get; set; }

            [JsonPropertyName("criterioElegibilidad")]
            public string CriterioElegibilidad { get; set; }

            [JsonPropertyName("espaciosIDs")]
            public string[] EspaciosIDs { get; set; }

            [JsonPropertyName("usuariosIDs")]
            public string[] UsuariosIDs { get; set; }
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

        public class CanjeDto
        {
            [JsonPropertyName("beneficioId")]
            public string beneficioId { get; set; }

            [JsonPropertyName("usuarioId")]
            public string usuarioId { get; set; }
        }

        public class EventoAccesoDto
        {

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

            [JsonPropertyName("eventoAccesoId")]
            public string? EventoAccesoId { get; set; }

            [JsonPropertyName("id")]
            public string? Id { get; set; }
        }

        public class BeneficioDto
        {

            [JsonPropertyName("id")]
            public string Id { get; set; }

            [JsonPropertyName("descripcion")]
            public string Descripcion { get; set; }


            [JsonPropertyName("tipo")]
            public string? Tipo { get; set; }

            [JsonPropertyName("nombre")]
            public string? Nombre { get; set; }

            [JsonPropertyName("vigenciaInicio")]
            public DateTime? VigenciaInicio { get; set; }

            [JsonPropertyName("VigenciaFin")]
            public DateTime? VigenciaFin { get; set; }

            [JsonPropertyName("cupoTotal")]
            public int? CupoTotal { get; set; }

            [JsonPropertyName("cupoPorUsuario")]
            public int? CupoPorUsuario { get; set; }


            [JsonPropertyName("requiereBiometria")]
            public bool? RequiereBiometria { get; set; }

            [JsonPropertyName("criterioElegibilidad")]
            public string? CriterioElegibilidad { get; set; }

            [JsonPropertyName("espaciosIDs")]
            public string[]? EspaciosIDs { get; set; }

            [JsonPropertyName("usuariosIDs")]
            public string[]? UsuariosIDs { get; set; }

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

        public class ReglaAccesoDto
        {
            [JsonPropertyName("id")]
            public string? ReglaId { get; set; }
            [JsonPropertyName("ventanaHoraria")]
            public string? VentanaHoraria { get; set; }
            [JsonPropertyName("vigenciaInicio")]
            public DateTime? VigenciaInicio { get; set; }
            [JsonPropertyName("vigenciaFin")]
            public DateTime? VigenciaFin { get; set; }
            [JsonPropertyName("prioridad")]
            public int Prioridad { get; set; }
            [JsonPropertyName("politica")]
            public string? Politica { get; set; }
            [JsonPropertyName("requiereBiometriaConfirmacion")]
            public bool RequiereBiometriaConfirmacion { get; set; }
            [JsonPropertyName("rol")]
            public string? Rol { get; set; }
            [JsonPropertyName("espaciosIDs")]
            public string[]? EspaciosIDs { get; set; }


        }
    }
}