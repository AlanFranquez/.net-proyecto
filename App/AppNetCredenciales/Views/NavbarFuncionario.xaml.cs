using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using AppNetCredenciales.models;
using AppNetCredenciales.services;
using AppNetCredenciales.Data;

namespace AppNetCredenciales.Views
{
    public partial class NavbarFuncionario : ContentView
    {
        public event EventHandler? ScanRequested;

        private readonly AuthService? _authService;
        private readonly LocalDBService _dbService;

        public NavbarFuncionario()
        {
            InitializeComponent();

            _authService = MauiProgram.ServiceProvider?.GetService<AuthService>();
            _dbService = MauiProgram.ServiceProvider?.GetService<LocalDBService>() ?? new LocalDBService();

            _ = ConfigureForFuncionarioAsync();
        }

        private async Task ConfigureForFuncionarioAsync()
        {
            try
            {
                var usuario = await _dbService.GetLoggedUserAsync();
                if (usuario == null)
                {
                    System.Diagnostics.Debug.WriteLine("NavbarFuncionario: no logged user found.");
                    return;
                }

                
                var funcionarioRol = await _dbService.GetRolByTipoAsync("Funcionario");
                if (funcionarioRol != null)
                {
                    try
                    {
                        await _dbService.ChangeUserSelectedRole(usuario.Email, funcionarioRol.RolId);
                        System.Diagnostics.Debug.WriteLine($"NavbarFuncionario: set user {usuario.Email} role to Funcionario ({funcionarioRol.RolId}).");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"NavbarFuncionario: error setting funcionario role: {ex}");
                    }
                }

                bool hasUsuarioRole = false;
                try
                {
                    var userId = usuario.UsuarioId;
                    var userRoles = await _dbService.GetRolsByUserAsync(userId);
                    hasUsuarioRole = userRoles?.Any(r => string.Equals(r.Tipo, "Usuario", StringComparison.OrdinalIgnoreCase)) == true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"NavbarFuncionario: error checking user roles: {ex}");
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    RoleLabel.Text = "Rol: Funcionario";
                    EspacioButton.IsVisible = hasUsuarioRole;
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"NavbarFuncionario: configuration error: {ex}");
            }
        }

        private void ScanButton_Clicked(object sender, EventArgs e)
        {
            if (Shell.Current != null)
            {
                _ = Shell.Current.GoToAsync("scan");
            }

            ScanRequested?.Invoke(this, EventArgs.Empty);
        }

        private void EspacioButton_Clicked(object sender, EventArgs e)
        {
            if (Shell.Current != null)
            {
                _ = Shell.Current.GoToAsync("espacio");
            }
        }
    }
}