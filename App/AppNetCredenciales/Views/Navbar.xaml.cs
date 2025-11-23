using AppNetCredenciales.Data;
using AppNetCredenciales.models;
using AppNetCredenciales.services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AppNetCredenciales.Views
{
    public partial class Navbar : ContentView
    {
        public ICommand NavigateCommand { get; private set; }
        public ICommand LogoutCommand { get; private set; }

        public ICommand SyncCommand { get; private set; }

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

            NavigateCommand = new Command<string>(async destino =>
            {
                if (string.IsNullOrEmpty(destino)) return;
                try
                {
                    Debug.WriteLine($"[Navbar] Navigating to: {destino}");
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
                    Debug.WriteLine("[Navbar] Logout command executed");
                    SessionManager.Logout();
                    await Shell.Current.GoToAsync("//login");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error en logout: {ex}");
                }
            });

            SyncCommand = new Command(async () =>
            {
                bool confirm = await Application.Current.MainPage.DisplayAlert(
                    "Sincronizar",
                    "¿Deseas sincronizar datos?",
                    "Sí",
                    "No"
                );

                if (!confirm)
                    return;

                try
                {
                    
                    await _db.SincronizarRolesFromBack();
                    await _db.SincronizarUsuariosFromBack();
                    await _db.SincronizarEspaciosFromBack();
                    await _db.SincronizarEventosFromBack();
                    await _db.SincronizarBeneficiosFromBack();
                    await _db.SincronizarCredencialesFromBack();
                    await _db.SincronizarReglasAccesoFromBack();

                    await Application.Current.MainPage.DisplayAlert("Éxito", "Sincronización completada", "OK");
                    var current = Shell.Current.CurrentState.Location.ToString();
                    await Shell.Current.GoToAsync(current, true);
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert("Sync error", ex.Message, "OK");
                }
            });



            BindingContext = this;


            _ = CheckIfFuncionarioAsync();
        }

        private async Task CheckIfFuncionarioAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[Navbar] === INICIANDO VERIFICACIÓN DE ROL FUNCIONARIO ===");

                var usuario = await _db.GetLoggedUserAsync();
                if (usuario == null)
                {
                    System.Diagnostics.Debug.WriteLine("[Navbar] ❌ No hay usuario logueado");
                    IsFuncionario = false;
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"[Navbar] === DATOS DEL USUARIO ===");
                System.Diagnostics.Debug.WriteLine($"[Navbar] Email: {usuario.Email}");
                System.Diagnostics.Debug.WriteLine($"[Navbar] idApi: {usuario.idApi}");
                System.Diagnostics.Debug.WriteLine($"[Navbar] RolId principal: {usuario.RolId}");
                System.Diagnostics.Debug.WriteLine($"[Navbar] RolesIDsJson: '{usuario.RolesIDsJson}'");
                System.Diagnostics.Debug.WriteLine($"[Navbar] RolesIDs array length: {usuario.RolesIDs?.Length ?? 0}");

                if (usuario.RolesIDs != null && usuario.RolesIDs.Length > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"[Navbar] RolesIDs contenido:");
                    for (int i = 0; i < usuario.RolesIDs.Length; i++)
                    {
                        System.Diagnostics.Debug.WriteLine($"[Navbar]   [{i}]: '{usuario.RolesIDs[i]}'");
                    }
                }

                // ✅ OBTENER TODOS LOS ROLES DE LA BASE DE DATOS
                var todosLosRoles = await _db.GetRolesAsync();
                System.Diagnostics.Debug.WriteLine($"[Navbar] === ROLES EN BASE DE DATOS ===");
                System.Diagnostics.Debug.WriteLine($"[Navbar] Total roles en DB: {todosLosRoles.Count}");

                foreach (var rol in todosLosRoles)
                {
                    System.Diagnostics.Debug.WriteLine($"[Navbar] Rol DB: ID={rol.RolId}, Tipo='{rol.Tipo}', idApi='{rol.idApi}'");
                }

                // ✅ VERIFICACIÓN SIMPLIFICADA
                bool esFuncionario = await VerificarSiEsFuncionario(usuario, todosLosRoles);

                System.Diagnostics.Debug.WriteLine($"[Navbar] === RESULTADO FINAL ===");
                System.Diagnostics.Debug.WriteLine($"[Navbar] Es Funcionario: {esFuncionario}");

                IsFuncionario = esFuncionario;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    OnPropertyChanged(nameof(IsFuncionario));
                    System.Diagnostics.Debug.WriteLine($"[Navbar] ✅ UI Actualizada - IsFuncionario: {IsFuncionario}");
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Navbar] ❌ Error en CheckIfFuncionarioAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[Navbar] StackTrace: {ex.StackTrace}");
                IsFuncionario = false;
            }
        }

        private async Task<bool> VerificarSiEsFuncionario(Usuario usuario, List<Rol> todosLosRoles)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[Navbar] === VERIFICANDO SI ES FUNCIONARIO ===");

                // Método 1: Verificar RolId principal
                bool esFuncionarioPorRolPrincipal = false;
                if (usuario.RolId.HasValue)
                {
                    var rolPrincipal = todosLosRoles.FirstOrDefault(r => r.RolId == usuario.RolId.Value);
                    if (rolPrincipal != null)
                    {
                        esFuncionarioPorRolPrincipal = string.Equals(rolPrincipal.Tipo, "Funcionario", StringComparison.OrdinalIgnoreCase);
                        System.Diagnostics.Debug.WriteLine($"[Navbar] Rol principal: {rolPrincipal.Tipo} -> Es Funcionario: {esFuncionarioPorRolPrincipal}");
                    }
                }

                // Método 2: Verificar RolesIDs array
                bool esFuncionarioPorRolesArray = false;
                if (usuario.RolesIDs != null && usuario.RolesIDs.Length > 0)
                {
                    System.Diagnostics.Debug.WriteLine("[Navbar] Verificando roles en array...");

                    foreach (var roleIdApi in usuario.RolesIDs)
                    {
                        if (string.IsNullOrWhiteSpace(roleIdApi)) continue;

                        System.Diagnostics.Debug.WriteLine($"[Navbar] Buscando rol con idApi: '{roleIdApi}'");

                        var rolEncontrado = todosLosRoles.FirstOrDefault(r =>
                            string.Equals(r.idApi, roleIdApi, StringComparison.OrdinalIgnoreCase));

                        if (rolEncontrado != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"[Navbar] Rol encontrado: Tipo='{rolEncontrado.Tipo}', idApi='{rolEncontrado.idApi}'");

                            if (string.Equals(rolEncontrado.Tipo, "Funcionario", StringComparison.OrdinalIgnoreCase))
                            {
                                esFuncionarioPorRolesArray = true;
                                System.Diagnostics.Debug.WriteLine($"[Navbar] ✅ ES FUNCIONARIO por array de roles");
                                break;
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"[Navbar] ❌ No se encontró rol con idApi: '{roleIdApi}'");
                        }
                    }
                }

                // Método 3: Verificar usando GetRolsByUserAsync
                bool esFuncionarioPorMetodo = false;
                try
                {
                    var rolesDelUsuario = await _db.GetRolsByUserAsync(usuario.UsuarioId);
                    System.Diagnostics.Debug.WriteLine($"[Navbar] Roles obtenidos por método GetRolsByUserAsync: {rolesDelUsuario.Count}");

                    foreach (var rol in rolesDelUsuario)
                    {
                        System.Diagnostics.Debug.WriteLine($"[Navbar] Rol del usuario: {rol.Tipo}");
                        if (string.Equals(rol.Tipo, "Funcionario", StringComparison.OrdinalIgnoreCase))
                        {
                            esFuncionarioPorMetodo = true;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Navbar] Error en GetRolsByUserAsync: {ex.Message}");
                }

                System.Diagnostics.Debug.WriteLine($"[Navbar] === RESUMEN DE VERIFICACIONES ===");
                System.Diagnostics.Debug.WriteLine($"[Navbar] Por Rol Principal: {esFuncionarioPorRolPrincipal}");
                System.Diagnostics.Debug.WriteLine($"[Navbar] Por Array de Roles: {esFuncionarioPorRolesArray}");
                System.Diagnostics.Debug.WriteLine($"[Navbar] Por Método GetRolsByUser: {esFuncionarioPorMetodo}");

                // Resultado final: ES funcionario si CUALQUIERA de los métodos lo confirma
                bool resultadoFinal = esFuncionarioPorRolPrincipal || esFuncionarioPorRolesArray || esFuncionarioPorMetodo;

                System.Diagnostics.Debug.WriteLine($"[Navbar] ===== RESULTADO FINAL: {resultadoFinal} =====");

                return resultadoFinal;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Navbar] Error en VerificarSiEsFuncionario: {ex.Message}");
                return false;
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