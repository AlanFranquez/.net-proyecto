using AppNetCredenciales.Data;
using AppNetCredenciales.models;
using AppNetCredenciales.services;
using AppNetCredenciales.Services;
using AppNetCredenciales.Views;
using CommunityToolkit.Maui.Converters;
using Microsoft.Maui.ApplicationModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace AppNetCredenciales.ViewModel
{
    public class RegisterViewModel : INotifyPropertyChanged
    {
        private string email;
        private string password;
        private string nombre;
        private string documento;
        private string apellido;
        private bool trabajando;
        private readonly AuthService authService;
        private readonly RegisterView view;
        private readonly LocalDBService _db;
        private readonly ConnectivityService _connectivityService = new ConnectivityService();
        private readonly ApiService _apiService = new ApiService();
        public ObservableCollection<SelectableRole> Roles { get; } = new();


        public event PropertyChangedEventHandler PropertyChanged;

        public string Nombre
        {
            get => nombre;
            set { if (nombre == value) return; nombre = value; OnPropertyChanged(); }
        }

        public string Apellido
        {
            get => apellido;
            set { if (apellido == value) return; apellido = value; OnPropertyChanged(); }
        }

        public string Documento
        {
            get => documento;
            set { if (documento == value) return; documento = value; OnPropertyChanged(); }
        }

        public string Email
        {
            get => email;
            set { if (email == value) return; email = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get => password;
            set { if (password == value) return; password = value; OnPropertyChanged(); }
        }

        public bool Trabajando
        {
            get => trabajando;
            set { if (trabajando == value) return; trabajando = value; OnPropertyChanged(); }
        }

        public ICommand RegisterCommand { get; }
        public ICommand NavigateToLogin { get; }

        public RegisterViewModel(RegisterView view, AuthService authService, LocalDBService db)
        {
            this.view = view;
            this.authService = authService;
            this._db = db;
            RegisterCommand = new Command(async () => await RegisterAsync(), () => !trabajando);

            NavigateToLogin = new Command(async () =>
            {
                if (Application.Current?.MainPage is not null)
                {
                    await Shell.Current.GoToAsync("//login");
                }
            });

            _ = LoadRolesAsync();
        }

        private async Task LoadRolesAsync()
        {
            try
            {
                var roles = await _db.GetRolesAsync();
                Roles.Clear();
                foreach (var r in roles)
                {
                    Roles.Add(new SelectableRole(r));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando roles: {ex}");
            }
        }

        private async Task<bool> RegisterAsync()
        {
            if (Trabajando)
                return false;

            Trabajando = true;

            if (string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(Nombre) ||
                string.IsNullOrWhiteSpace(Apellido) ||
                string.IsNullOrWhiteSpace(Documento))
            {
                Trabajando = false;
                await view.DisplayAlert("Error", "Falta ingresar más información", "OK");
                return false;
            }

            var seleccionadasRoles = Roles.Where(r => r.IsSelected).ToList();
            var usuario = new Usuario
            {
                Nombre = Nombre,
                Apellido = Apellido,
                Documento = Documento,
                Email = Email,
                Password = Password,
                RolesIDs = seleccionadasRoles
                                .Select(r => r.Role.idApi)
                                .Where(id => !string.IsNullOrWhiteSpace(id))
                                .ToArray()
            };

            var saved = await _db.SaveUsuarioAsync(usuario);
            if (saved <= 0)
            {
                Trabajando = false;
                await view.DisplayAlert("Error", "No se pudo guardar el usuario localmente", "OK");
                return false;
            }
            if (usuario.UsuarioId == 0) usuario.UsuarioId = saved;

            
            
            if (_connectivityService.IsConnected)
            {
                try
                {

                    foreach(var r in seleccionadasRoles)
                    {
                        Debug.WriteLine($"[Register] Rol seleccionado: {r.Role.Tipo} (ID API: {r.Role.idApi}) ");
                    }

                    if (seleccionadasRoles.Count > 0)
                    {
                        usuario.RolesIDs = seleccionadasRoles
                            .Select(r => r.Role.idApi)
                            .Where(id => !string.IsNullOrWhiteSpace(id))
                            .ToArray();

                        
                        await _db.SaveUsuarioAsync(usuario);
                    }

                    var nuevoDto = new ApiService.NewUsuarioDto
                    {
                        Nombre = usuario.Nombre,
                        Apellido = usuario.Apellido,
                        Email = usuario.Email,
                        Password = usuario.Password,
                        Documento = usuario.Documento,
                        RolesIDs = seleccionadasRoles
                                    .Select(r => r.Role.idApi)
                                    .Where(id => !string.IsNullOrWhiteSpace(id))
                                    .ToArray()
                    };

                    System.Diagnostics.Debug.WriteLine($"[Register] Creando usuario en API...");

                    var apiUsuarioId = await _api_service_create_usuario_safe(nuevoDto);
                    if (!string.IsNullOrWhiteSpace(apiUsuarioId))
                    {
                        // ✅ Solo actualizar el idApi del usuario existente
                        usuario.idApi = apiUsuarioId;
                        await _db.SaveUsuarioAsync(usuario);

                        System.Diagnostics.Debug.WriteLine($"[Register] ✅ Usuario creado en API con ID: {usuario.idApi}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("[Register] ⚠️ No se pudo crear usuario en API, generando GUID local");
                        usuario.idApi = Guid.NewGuid().ToString();
                        await _db.SaveUsuarioAsync(usuario);
                    }
                }
                catch (Exception ex)
                {
                    
                    usuario.idApi = Guid.NewGuid().ToString();
                    await _db.SaveUsuarioAsync(usuario);
                }
            }
            else
            {
                usuario.FaltaCargar = true;
                usuario.idApi = Guid.NewGuid().ToString();
                await _db.SaveUsuarioAsync(usuario);
            }

            
            if (string.IsNullOrWhiteSpace(usuario.idApi))
            {
                usuario.idApi = Guid.NewGuid().ToString();
                await _db.SaveUsuarioAsync(usuario);
            }

            
            var credencial = new Credencial
            {
                Tipo = CredencialTipo.Campus,
                Estado = CredencialEstado.Emitida,
                IdCriptografico = Guid.NewGuid().ToString("N"),
                FechaEmision = DateTime.UtcNow,
                FechaExpiracion = DateTime.UtcNow.AddYears(1),
                FaltaCarga = true,
                usuarioIdApi = usuario.idApi
            };

            var createdCredId = await _db.SaveCredencialAsync(credencial);

            if (createdCredId > 0)
            {
                usuario.CredencialId = createdCredId;
                await _db.SaveUsuarioAsync(usuario);
                System.Diagnostics.Debug.WriteLine($"[Register] ✅ Credencial guardada con ID local: {createdCredId}");
            }

            await view.DisplayAlert("Éxito", "Usuario registrado correctamente", "OK");
            await view.Navigation.PopAsync();

            Trabajando = false;
            return true;
        }

        // Cambiar el método helper para que solo devuelva el usuarioId (string)
        private async Task<string?> _api_service_create_usuario_safe(ApiService.NewUsuarioDto dto)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[Register] Intentando crear usuario en API...");

                var apiResult = await _apiService.CreateUsuarioAsync(dto);
                if (apiResult != null && !string.IsNullOrWhiteSpace(apiResult.UsuarioId))
                {
                    System.Diagnostics.Debug.WriteLine($"[Register] ✅ Usuario creado directamente, ID: {apiResult.UsuarioId}");
                    return apiResult.UsuarioId;
                }

                System.Diagnostics.Debug.WriteLine("[Register] No se pudo crear directamente, buscando por email...");

                // Fallback: buscar usuario creado por email
                var usuarios = await _apiService.GetUsuariosAsync();
                var matched = usuarios.FirstOrDefault(u =>
                    string.Equals(u.Email?.Trim(), dto.Email?.Trim(), StringComparison.OrdinalIgnoreCase));

                if (matched != null && !string.IsNullOrWhiteSpace(matched.UsuarioId))
                {
                    System.Diagnostics.Debug.WriteLine($"[Register] ✅ Usuario encontrado en API, ID: {matched.UsuarioId}");
                    return matched.UsuarioId;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[Register] ❌ Usuario no encontrado en API");
                    return null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Register] ❌ Error en _api_service_create_usuario_safe: {ex.Message}");
                return null;
            }
        }



        public class SelectableRole : INotifyPropertyChanged
        {
            private bool isSelected;
            public Rol Role { get; }

            public bool IsSelected
            {
                get => isSelected;
                set { if (isSelected == value) return; isSelected = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected))); }
            }

            public SelectableRole(Rol role)
            {
                Role = role;
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}
