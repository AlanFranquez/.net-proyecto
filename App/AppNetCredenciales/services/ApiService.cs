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

        

        public async Task<List<UsuarioDto>> GetUsuariosAsync()
        {
            try
            {
                var list = await _httpClient.GetFromJsonAsync<List<UsuarioDto>>("usuarios", _jsonOptions)
                           ?? new List<UsuarioDto>();

                // debug log to Output window
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

        public class UsuarioDto
        {
            [JsonPropertyName("usuarioId")]
            public string? UsuarioId { get; set; }   // ensure mapping if API uses this name

            [JsonPropertyName("nombre")]
            public string? Nombre { get; set; }

            [JsonPropertyName("apellido")]
            public string? Apellido { get; set; }

            [JsonPropertyName("email")]
            public string? Email { get; set; }
        }
    }
}