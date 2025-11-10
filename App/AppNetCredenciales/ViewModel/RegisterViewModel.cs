using AppNetCredenciales.Data;
using AppNetCredenciales.models;
using AppNetCredenciales.services;
using AppNetCredenciales.Services;
using AppNetCredenciales.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
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

            var usuario = new Usuario
            {
                Nombre = Nombre,
                Apellido = Apellido,
                Documento = Documento,
                Email = Email,
                Password = Password
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
                    var seleccionadasRoles = Roles.Where(r => r.IsSelected).ToList();
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

                    var apiResult = await _api_service_create_usuario_safe(nuevoDto);
                    System.Diagnostics.Debug.WriteLine($"[Register] API create usuario result: {apiResult?.UsuarioId}");

                    if (apiResult != null && !string.IsNullOrWhiteSpace(apiResult.UsuarioId))
                    {
                        usuario.idApi = apiResult.UsuarioId;
                        await _db.SaveUsuarioAsync(usuario);
                    }
                }
                catch (Exception ex)
                {
                    // network error or server error — continue but mark for sync
                    System.Diagnostics.Debug.WriteLine($"[Register] Error creating user on API: {ex.Message}");
                }
            }
            else
            {
                usuario.FaltaCargar = true;
                await _db.SaveUsuarioAsync(usuario);
            }

            // Create credential: only attempt remote creation if we have a backend usuario id (GUID)
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

            if (_connectivity_service_is_valid_guid(usuario.idApi))
            {
                // set FaltaCarga false only when remote creation succeeds
                var credId = await _db.SaveCredencialAsync(new Credencial
                {
                    Tipo = credencial.Tipo,
                    Estado = credencial.Estado,
                    IdCriptografico = credencial.IdCriptografico,
                    FechaEmision = credencial.FechaEmision,
                    FechaExpiracion = credencial.FechaExpiracion,
                    FaltaCarga = false,
                    usuarioIdApi = usuario.idApi
                });

                if (credId <= 0)
                {
                    // couldn't create remotely or save; fall back to local-only cred
                    credencial.FaltaCarga = true;
                    credencial.usuarioIdApi = null;
                    await _db.SaveCredencialAsync(credencial);
                }
                else
                {
                    // ensure local user points to credencial if needed (optional)
                }
            }
            else
            {
                // offline or no backend user id: save credencial locally for later sync
                credencial.FaltaCarga = true;
                credencial.usuarioIdApi = null;
                await _db.SaveCredencialAsync(credencial);
            }

            // Assign selected roles to the (local) user
            var usuarioId = usuario.UsuarioId;
            var seleccionadas = Roles.Where(r => r.IsSelected).ToList();
            foreach (var s in seleccionadas)
            {
                var ur = new UsuarioRol
                {
                    UsuarioId = usuarioId,
                    RolId = s.Role.RolId,
                    FechaAsignado = DateTime.UtcNow
                };
                await _db.SaveUsuarioRolAsync(ur);
            }

            await view.DisplayAlert("Éxito", "Usuario registrado correctamente", "OK");
            await view.Navigation.PopAsync();

            Trabajando = false;
            return true;
        }

        // small helpers extracted to keep RegisterAsync readable
        private async Task<ApiService.UsuarioDto?> _api_service_create_usuario_safe(ApiService.NewUsuarioDto dto)
        {
            try
            {
                var apiResult = await _apiService.CreateUsuarioAsync(dto);
                if (apiResult != null && !string.IsNullOrWhiteSpace(apiResult.UsuarioId))
                    return apiResult;

                // fallback: try to find created user by email
                var usuarios = await _apiService.GetUsuariosAsync();
                var matched = usuarios.FirstOrDefault(u => string.Equals(u.Email?.Trim(), dto.Email?.Trim(), StringComparison.OrdinalIgnoreCase));
                return matched;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Register helper] API create/find error: {ex.Message}");
                return null;
            }
        }

        private static bool _connectivity_service_is_valid_guid(string? id)
        {
            return !string.IsNullOrWhiteSpace(id) && Guid.TryParse(id, out _);
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
