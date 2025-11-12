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

            LogoutCommand = new Command(async () =>
            {
                try
                {
                    SessionManager.Logout();
                    await Shell.Current.GoToAsync("//login");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error en logout: {ex}");
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


                var userRoleIds = usuario.RolesIDs ?? Array.Empty<string>();
                bool hasFuncionarioFromArray = false;

                if (userRoleIds.Length > 0)
                {
                    var roles = await _db.GetRolesAsync();
                    
                    Debug.WriteLine($"[Navbar] Available roles from DB:");
                    foreach (var role in roles)
                    {
                        Debug.WriteLine($"[Navbar] - Role: {role.Tipo}, idApi: {role.idApi}");
                    }


                    hasFuncionarioFromArray = roles.Any(r =>
                        string.Equals(r.Tipo?.Trim(), "Funcionario", StringComparison.OrdinalIgnoreCase)
                        && !string.IsNullOrWhiteSpace(r.idApi)
                        && userRoleIds.Contains(r.idApi, StringComparer.OrdinalIgnoreCase));

                }

                bool hasFuncionarioFromLocal = false;
                var localRoles = await _db.GetRolesAsync();

                if (userRoleIds?.Length > 0)
                {
                    foreach (var roleId in userRoleIds)
                    {
                        foreach (var localRole in localRoles)
                        {
                            if (string.Equals(localRole.idApi, roleId, StringComparison.OrdinalIgnoreCase) 
                                && string.Equals(localRole.Tipo, "Funcionario", StringComparison.OrdinalIgnoreCase))
                            {
                                hasFuncionarioFromLocal = true;
                                break;
                            }
                        }
                        if (hasFuncionarioFromLocal) break;
                    }
                }

                IsFuncionario = hasFuncionarioFromArray || hasFuncionarioFromLocal;
                
                Debug.WriteLine($"[Navbar] Final IsFuncionario result: {IsFuncionario}");

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    OnPropertyChanged(nameof(IsFuncionario));
                    Debug.WriteLine($"[Navbar] UI Updated - IsFuncionario: {IsFuncionario}");
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CheckIfFuncionarioAsync error: {ex}");
                IsFuncionario = false;
            }
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