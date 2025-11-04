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

namespace AppNetCredenciales.Views
{
    public partial class Navbar : ContentView
    {
        public ICommand NavigateCommand { get; private set; }
        public ICommand LogoutCommand { get; private set; }

        // Bindable property so XAML can react to role changes
        public static readonly BindableProperty IsFuncionarioProperty =
            BindableProperty.Create(nameof(IsFuncionario), typeof(bool), typeof(Navbar), false);

        public bool IsFuncionario
        {
            get => (bool)GetValue(IsFuncionarioProperty);
            set => SetValue(IsFuncionarioProperty, value);
        }

        private readonly LocalDBService _db;

        public Navbar()
        {
            InitializeComponent();
            _db = App.Services?.GetRequiredService<LocalDBService>()
                  ?? throw new InvalidOperationException("LocalDBService not registered in DI.");

            // check user roles (fire-and-forget)
            _ = CheckIfFuncionarioAsync();

            _ = cambiarRolAUsuario();

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

            // Simple logout command
            LogoutCommand = new Command(async () =>
            {
                try
                {
                    SessionManager.Logout();
                    await Shell.Current.GoToAsync("//login");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error cambiando rol: {ex}");
                }
            });

            BindingContext = this;
        }

        private async Task CheckIfFuncionarioAsync()
        {
            try
            {
                var usuario = await _db.GetLoggedUserAsync();
                if (usuario == null)
                {
                    IsFuncionario = false;
                    return;
                }

                var roles = await _db.GetRolsByUserAsync(usuario.UsuarioId);
                IsFuncionario = roles != null && roles.Any(r => string.Equals(r.Tipo?.Trim(), "Funcionario", StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CheckIfFuncionarioAsync error: {ex}");
                IsFuncionario = false;
            }
        }

        private async Task cambiarRolAUsuario()
        {
            var usuario = await _db.GetLoggedUserAsync();

            var rol = await _db.GetRolByTipoAsync("Usuario");
            await _db.ChangeUserSelectedRole(usuario.Email, rol.RolId);
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            try
            {
                SessionManager.Logout();
                await Shell.Current.GoToAsync("login");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Logout click error: {ex}");
            }
        }

        private async Task OnRoleImageClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("roleselection");
        }
    }
}