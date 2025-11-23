using AppNetCredenciales.Data;
using AppNetCredenciales.models;
using AppNetCredenciales.services;
using AppNetCredenciales.Views;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AppNetCredenciales.ViewModel
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private string email;
        private string password;
        private bool trabajando;
        private readonly AuthService authService;
        private readonly LoginView view;
        private readonly LocalDBService dbService;

        public event PropertyChangedEventHandler PropertyChanged;

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

        public ICommand LoginCommand { get; }
        public ICommand NavigateToRegisterCommand { get; }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public LoginViewModel(LoginView view, AuthService AuthService, LocalDBService db)
        {
            this.view = view;
            this.dbService = db;
            this.authService = AuthService;
            LoginCommand = new Command(async () => await LoginAsync(), () => !trabajando);

            NavigateToRegisterCommand = new Command(async () =>
            {
                if (Application.Current?.MainPage is not null)
                {
                    await Shell.Current.GoToAsync("register");
                }
            });
        }

        public LoginViewModel() { }

        public async Task ShowUsuariosAsync()
        {
            var usuarios = await authService.GetUsuarios();

            var eventos = await dbService.GetEspaciosAsync();
            System.Diagnostics.Debug.WriteLine("DESDE LA FUNCION  SHOW USUARIOS ASYNC, PARA MOSTRAR TAMbien eventos");
            foreach (var ev in eventos)
            {
                System.Diagnostics.Debug.WriteLine($"[Evento] {ev.EspacioId} - {ev.Nombre} - {ev.Descripcion}");
            }

            if (usuarios == null || usuarios.Count == 0)
            {
                await App.Current.MainPage.DisplayAlert("Usuarios", "No hay usuarios registrados.", "OK");
                return;
            }

            string lista = string.Join("\n", usuarios.Select(u => $"{u.Nombre} {u.Apellido} - {u.Email} - {u.Password} - {u.idApi}"));
            await App.Current.MainPage.DisplayAlert("Usuarios registrados", lista, "Cerrar");
        }

        public async Task<bool> LoginAsync()
        {
            if (trabajando)
                return false;

            Trabajando = true;

            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                Trabajando = false;
                await App.Current.MainPage.DisplayAlert("Error", "Por favor ingrese email y contraseña.", "OK");
                return false;
            }

            var loggeo = await authService.loginUsuario(Email, Password);

            if (!loggeo)
            {
                Trabajando = false;
                await App.Current.MainPage.DisplayAlert("Error", "Usuario o contraseña incorrectos.", "OK");
                return false;
            }

            var u = await authService.getUsuarioData(Email);
            Trabajando = false;

            try
            {
                System.Diagnostics.Debug.WriteLine($"DATOS DE USUARIO {u.UsuarioId} - {u.Email} - {u.idApi}");
                await SessionManager.SaveUserAsync(u.UsuarioId, Email, u.idApi);

                // ✅ NUEVA LÓGICA: Verificar todos los roles del usuario
                await DeterminarNavegacionPorRoles(u);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en login: {ex.Message}");
                Console.WriteLine(ex.Message);
            }

            return true;
        }

        // ✅ NUEVO MÉTODO: Determinar navegación basada en roles
        private async Task DeterminarNavegacionPorRoles(Usuario usuario)
        {
            try
            {
                List<Rol> rolesDelUsuario = new List<Rol>();

                // Método 1: Verificar RolId principal (si existe)
                if (usuario.RolId.HasValue && usuario.RolId > 0)
                {
                    var rolPrincipal = await dbService.GetRolByIdAsync(usuario.RolId.Value);
                    if (rolPrincipal != null)
                    {
                        rolesDelUsuario.Add(rolPrincipal);
                        System.Diagnostics.Debug.WriteLine($"[LoginViewModel] Rol principal encontrado: {rolPrincipal.Tipo}");
                    }
                }

                // Método 2: Verificar RolesIDs (array de roles)
                if (usuario.RolesIDs != null && usuario.RolesIDs.Length > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"[LoginViewModel] Verificando {usuario.RolesIDs.Length} roles adicionales...");

                    foreach (var rolIdApi in usuario.RolesIDs)
                    {
                        if (string.IsNullOrWhiteSpace(rolIdApi)) continue;

                        // Buscar rol por idApi
                        var rol = await dbService.GetRolByTipoAsync(rolIdApi);
                        if (rol == null)
                        {
                            // Si no se encuentra por tipo, buscar por idApi
                            var todosLosRoles = await dbService.GetRolesAsync();
                            rol = todosLosRoles.FirstOrDefault(r => r.idApi == rolIdApi);
                        }

                        if (rol != null && !rolesDelUsuario.Any(r => r.RolId == rol.RolId))
                        {
                            rolesDelUsuario.Add(rol);
                            System.Diagnostics.Debug.WriteLine($"[LoginViewModel] Rol adicional encontrado: {rol.Tipo}");
                        }
                    }
                }

                // Método 3: Usar el método GetRolsByUserAsync
                try
                {
                    var rolesDelMetodo = await dbService.GetRolsByUserAsync(usuario.UsuarioId);
                    foreach (var rol in rolesDelMetodo)
                    {
                        if (!rolesDelUsuario.Any(r => r.RolId == rol.RolId))
                        {
                            rolesDelUsuario.Add(rol);
                            System.Diagnostics.Debug.WriteLine($"[LoginViewModel] Rol del método encontrado: {rol.Tipo}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[LoginViewModel] Error obteniendo roles por usuario: {ex.Message}");
                }

                System.Diagnostics.Debug.WriteLine($"[LoginViewModel] === RESUMEN DE ROLES ===");
                System.Diagnostics.Debug.WriteLine($"[LoginViewModel] Total roles encontrados: {rolesDelUsuario.Count}");

                var tiposDeRoles = rolesDelUsuario.Select(r => r.Tipo).ToList();
                foreach (var tipo in tiposDeRoles)
                {
                    System.Diagnostics.Debug.WriteLine($"[LoginViewModel] - Rol: {tipo}");
                }

                // ✅ LÓGICA DE NAVEGACIÓN
                await NavegarSegunRoles(tiposDeRoles);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LoginViewModel] Error determinando roles: {ex.Message}");

                // Fallback: usar navegación por defecto
                await Shell.Current.GoToAsync("espacio");
            }
        }

        // ✅ NUEVO MÉTODO: Navegar según los roles
        private async Task NavegarSegunRoles(List<string> tiposDeRoles)
        {
            try
            {
                // Si no tiene roles o la lista está vacía, ir a espacios por defecto
                if (tiposDeRoles == null || tiposDeRoles.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("[LoginViewModel] ⚠️ No hay roles definidos, navegando a espacios");
                    await Shell.Current.GoToAsync("espacio");
                    return;
                }

                // Verificar si SOLO tiene el rol "Funcionario"
                bool soloEsFuncionario = tiposDeRoles.Count == 1 &&
                                       tiposDeRoles.Any(r => r.Equals("Funcionario", StringComparison.OrdinalIgnoreCase));

                bool esFuncionario = tiposDeRoles.Any(r => r.Equals("Funcionario", StringComparison.OrdinalIgnoreCase));

             
                bool esUsuario = tiposDeRoles.Any(r =>
                    r.Equals("Usuario", StringComparison.OrdinalIgnoreCase) ||
                    r.Equals("Cliente", StringComparison.OrdinalIgnoreCase) ||
                    r.Equals("Participante", StringComparison.OrdinalIgnoreCase));

                
                if (soloEsFuncionario)
                {
                    // Si SOLO es funcionario → ir a scan
                    System.Diagnostics.Debug.WriteLine("[LoginViewModel] ✅ Navegando a SCAN (solo funcionario)");
                    await Shell.Current.GoToAsync("scan");
                }
                else if (esUsuario)
                {
                    // Si tiene rol de usuario → ir a espacios
                    System.Diagnostics.Debug.WriteLine("[LoginViewModel] ✅ Navegando a ESPACIOS (usuario)");
                    await Shell.Current.GoToAsync("espacio");
                }
                else if (esFuncionario)
                {
                    // Si es funcionario pero no usuario → ir a scan
                    System.Diagnostics.Debug.WriteLine("[LoginViewModel] ✅ Navegando a SCAN (funcionario sin rol usuario)");
                    await Shell.Current.GoToAsync("scan");
                }
                else
                {
                    // Fallback: ir a espacios por defecto
                    System.Diagnostics.Debug.WriteLine("[LoginViewModel] ⚠️ Navegación por defecto a ESPACIOS");
                    await Shell.Current.GoToAsync("espacio");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LoginViewModel] Error en navegación: {ex.Message}");
                await Shell.Current.GoToAsync("espacio"); // Fallback
            }
        }
    }
}