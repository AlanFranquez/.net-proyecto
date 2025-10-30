using System;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using AppNetCredenciales.services;
using AppNetCredenciales.models;
using AppNetCredenciales.Data;

using System.Diagnostics;

namespace AppNetCredenciales.Views
{
    public partial class Navbar : ContentView
    {
        public ICommand NavigateCommand { get; private set; }
        public ICommand CambiarRolCommand { get; private set; }
        public ObservableCollection<Rol> Roles { get; } = new ObservableCollection<Rol>();

        private Rol _rolSeleccionado;
        public Rol RolSeleccionado
        {
            get => _rolSeleccionado;
            set
            {
                if (!ReferenceEquals(_rolSeleccionado, value))
                {
                    _rolSeleccionado = value;
                    OnPropertyChanged(nameof(RolSeleccionado));

                    if (_rolSeleccionado != null)
                        CambiarRolCommand?.Execute(_rolSeleccionado);
                }
            }
        }
        public ICommand LogoutCommand { get; private set; }

        private readonly AuthService _authService;
        private readonly LocalDBService _dbService;

        public Navbar()
        {
            InitializeComponent();

            _authService = MauiProgram.ServiceProvider?.GetService<AuthService>();
            _dbService = MauiProgram.ServiceProvider?.GetService<LocalDBService>() ?? new LocalDBService();

            LogoutCommand = new Command(async () =>
            {
                try
                {
                    SessionManager.Logout();
                    await Shell.Current.GoToAsync("//login");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Logout error: {ex}");
                }
            });

            // Comandos
            NavigateCommand = new Command<string>(async destino =>
            {
                if (string.IsNullOrEmpty(destino)) return;
                try
                {
                    await Shell.Current.GoToAsync(destino);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Navbar] navigation error: {ex}");
                    if (Application.Current?.MainPage != null)
                        await Application.Current.MainPage.DisplayAlert("Navigation error", ex.Message, "OK");
                }
            });

            CambiarRolCommand = new Command<Rol>(async nuevoRol =>
            {
                if (nuevoRol == null) return;

                try
                {
                    if (_authService != null)
                        await _authService.ChangeRoleForLoggedUser(nuevoRol.RolId);
                    else
                        await _dbService.ChangeUserSelectedRole(await SessionManager.GetUserEmailAsync(), nuevoRol.RolId);

                    await SessionManager.SaveUserRoleAsync(nuevoRol.RolId);

                    ActualizarPermisosSegunRol(nuevoRol.Tipo);

                    if (Application.Current?.MainPage != null)
                        await Application.Current.MainPage.DisplayAlert("Rol Cambiado", $"Ahora eres: {nuevoRol.Tipo}", "OK");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error cambiando rol: {ex}");
                }
            });

            BindingContext = this;

            _ = LoadRolesAsync();
        }

        private async Task LoadRolesAsync()
        {
            try
            {
                List<Rol> roles = null;

                if (_authService != null)
                {
                    roles = await _authService.GetRolesForLoggedUser();
                    Debug.WriteLine($"[Navbar] Roles from AuthService: {(roles?.Count ?? 0)}");
                }

                if ((roles == null || roles.Count == 0) && _dbService != null)
                {
                    var userId = await SessionManager.GetUserIdAsync();
                    if (userId > 0)
                        roles = await _dbService.GetRolsByUserAsync(userId);

                    if (roles == null || roles.Count == 0)
                        roles = await _dbService.GetRolesAsync();
                }

                var raw = roles ?? new List<Rol>();
                var distinctRoles = raw.GroupBy(r => r.RolId).Select(g => g.First()).ToList();

                Roles.Clear();
                foreach (var r in distinctRoles)
                    Roles.Add(r);

                if (Roles.Count > 0)
                {
                    var loggedRole = await _dbService.GetLoggedUserRoleAsync();
                    RolSeleccionado = loggedRole != null
                        ? Roles.FirstOrDefault(x => x.RolId == loggedRole.RolId) ?? Roles.First()
                        : Roles.First();

                    ActualizarPermisosSegunRol(RolSeleccionado.Tipo);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading roles: {ex}");
            }
        }

        private void ActualizarPermisosSegunRol(string rol)
        {
            var defaultNav = this.FindByName<StackLayout>("DefaultNav");
            var funcionarioNav = this.FindByName<StackLayout>("FuncionarioNav");

            bool isFuncionario = string.Equals(rol, "Funcionario", StringComparison.OrdinalIgnoreCase);

            if (defaultNav != null)
                defaultNav.IsVisible = !isFuncionario;

            if (funcionarioNav != null)
                funcionarioNav.IsVisible = isFuncionario;

            if (isFuncionario)
                _ = Shell.Current.GoToAsync("scan");
            else
                _ = Shell.Current.GoToAsync("espacio");
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            
                SessionManager.Logout();
                await Shell.Current.GoToAsync("login");
           
        }

        private void OnRoleImageClicked(object sender, EventArgs e)
        {
            try
            {
                RolePicker?.Focus();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error opening role picker: {ex}");
            }
        }
    }
}