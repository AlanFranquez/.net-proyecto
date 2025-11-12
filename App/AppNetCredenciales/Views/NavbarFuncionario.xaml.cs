using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using AppNetCredenciales.models;
using AppNetCredenciales.services;
using AppNetCredenciales.Data;

namespace AppNetCredenciales.Views
{
    public partial class NavbarFuncionario : ContentView, INotifyPropertyChanged
    {
        public event EventHandler? ScanRequested;

        private readonly AuthService? _authService;
        private readonly LocalDBService _dbService;

        // Bindable property for HasUsuarioRole
        public static readonly BindableProperty HasUsuarioRoleProperty =
            BindableProperty.Create(nameof(HasUsuarioRole), typeof(bool), typeof(NavbarFuncionario), false);

        public bool HasUsuarioRole
        {
            get => (bool)GetValue(HasUsuarioRoleProperty);
            set => SetValue(HasUsuarioRoleProperty, value);
        }

        // Commands
        public ICommand NavigateCommand { get; private set; }
        public ICommand LogoutCommand { get; private set; }
        public ICommand GoToEspaciosCommand { get; private set; }

        public NavbarFuncionario()
        {
            InitializeComponent();

            _authService = MauiProgram.ServiceProvider?.GetService<AuthService>();
            _dbService = MauiProgram.ServiceProvider?.GetService<LocalDBService>() ?? new LocalDBService();

            // Initialize commands
            NavigateCommand = new Command<string>(async destino =>
            {
                if (string.IsNullOrEmpty(destino)) return;
                try
                {
                    await Shell.Current.GoToAsync(destino);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[NavbarFuncionario] navigation error: {ex}");
                }
            });

            LogoutCommand = new Command(async () =>
            {
                try
                {
                    SessionManager.Logout();
                    await Shell.Current.GoToAsync("//login");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[NavbarFuncionario] logout error: {ex}");
                }
            });

            // New command to go to espacios (switches to Usuario role and navigates)
            GoToEspaciosCommand = new Command(async () =>
            {
                try
                {
                    var usuario = await _dbService.GetLoggedUserAsync();
                    if (usuario != null)
                    {
                        // Switch to Usuario role temporarily
                        var usuarioRole = await _dbService.GetRolByTipoAsync("Usuario");
                        if (usuarioRole != null)
                        {
                            await _dbService.ChangeUserSelectedRole(usuario.Email, usuarioRole.RolId);
                            System.Diagnostics.Debug.WriteLine($"[NavbarFuncionario] Switched to Usuario role and navigating to espacio");
                        }

                        // Navigate to espacios
                        await Shell.Current.GoToAsync("espacio");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[NavbarFuncionario] GoToEspaciosCommand error: {ex}");
                }
            });

            BindingContext = this;
            _ = ConfigureForFuncionarioAsync();
        }

        private async Task ConfigureForFuncionarioAsync()
        {
            try
            {
                var usuario = await _dbService.GetLoggedUserAsync();
                if (usuario == null)
                {
                    System.Diagnostics.Debug.WriteLine("[NavbarFuncionario] No logged user found.");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"[NavbarFuncionario] Configuring for user: {usuario.Email}");
                System.Diagnostics.Debug.WriteLine($"[NavbarFuncionario] User RolesIDs: [{string.Join(", ", usuario.RolesIDs ?? Array.Empty<string>())}]");

                bool hasUsuarioRole = false;
                var userRoleIds = usuario.RolesIDs ?? Array.Empty<string>();

                if (userRoleIds.Length > 0)
                {
                    var roles = await _dbService.GetRolesAsync();

                    // Debug: log all available roles
                    System.Diagnostics.Debug.WriteLine($"[NavbarFuncionario] Available roles from DB:");
                    foreach (var role in roles)
                    {
                        System.Diagnostics.Debug.WriteLine($"[NavbarFuncionario] - Role: {role.Tipo}, idApi: {role.idApi}");
                    }

                    hasUsuarioRole = roles.Any(r =>
                        string.Equals(r.Tipo?.Trim(), "Usuario", StringComparison.OrdinalIgnoreCase)
                        && !string.IsNullOrWhiteSpace(r.idApi)
                        && userRoleIds.Contains(r.idApi, StringComparer.OrdinalIgnoreCase));

                    System.Diagnostics.Debug.WriteLine($"[NavbarFuncionario] Usuario role found in RolesIDs: {hasUsuarioRole}");
                }

                // Also check local UsuarioRol relations as fallback
                if (!hasUsuarioRole)
                {
                    var userRoles = await _dbService.GetRolsByUserAsync(usuario.UsuarioId);
                    hasUsuarioRole = userRoles?.Any(r => string.Equals(r.Tipo, "Usuario", StringComparison.OrdinalIgnoreCase)) == true;
                    System.Diagnostics.Debug.WriteLine($"[NavbarFuncionario] Usuario role found in local relations: {hasUsuarioRole}");

                    // Debug: log all user roles
                    if (userRoles != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"[NavbarFuncionario] Local user roles: {string.Join(", ", userRoles.Select(r => r.Tipo))}");
                    }
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    HasUsuarioRole = hasUsuarioRole;
                    RoleLabel.Text = "Rol: Funcionario";
                    System.Diagnostics.Debug.WriteLine($"[NavbarFuncionario] UI Updated - HasUsuarioRole: {hasUsuarioRole}");

                    // Force property change notification
                    OnPropertyChanged(nameof(HasUsuarioRole));
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"NavbarFuncionario: configuration error: {ex}");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void ScanButton_Clicked(object sender, EventArgs e)
        {
            NavigateCommand?.Execute("scan"); // Fixed: was "espacio", should be "scan"
        }

        private void EspacioButton_Clicked(object sender, EventArgs e)
        {
            GoToEspaciosCommand?.Execute(null);
        }
    }
}