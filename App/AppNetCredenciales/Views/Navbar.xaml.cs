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

namespace AppNetCredenciales.Views;

public partial class Navbar : ContentView
{
    public ICommand NavigateCommand { get; private set; }
    public ICommand CambiarRolCommand { get; private set; }
    public ObservableCollection<Rol> Roles { get; } = new ObservableCollection<Rol>();

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
    private Rol _rolSeleccionado;

    private readonly AuthService _authService;
    private readonly LocalDBService _dbService;

    public Navbar()
    {
        InitializeComponent();

        _authService = MauiProgram.ServiceProvider?.GetService<AuthService>();
        _dbService = MauiProgram.ServiceProvider?.GetService<LocalDBService>();

        
        if (_dbService == null)
            _dbService = new LocalDBService();

        // Navigation command
        NavigateCommand = new Command<string>(async destino =>
        {
            if (string.IsNullOrEmpty(destino)) return;
            await Shell.Current.GoToAsync(destino);
        });

        // Change role command
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

        // Load roles asynchronously (fire-and-forget)
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
                try
                {
                    var userId = await SessionManager.GetUserIdAsync();
                    if (userId > 0)
                    {
                        roles = await _dbService.GetRolsByUserAsync(userId);
                        Debug.WriteLine($"[Navbar] Roles from GetRolsByUserAsync: {(roles?.Count ?? 0)}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Navbar] error getting roles by user: {ex}");
                }

                // final fallback: all roles
                if (roles == null || roles.Count == 0)
                {
                    roles = await _dbService.GetRolesAsync();
                    Debug.WriteLine($"[Navbar] Roles from GetRolesAsync (fallback): {(roles?.Count ?? 0)}");
                }
            }

            var raw = roles ?? new List<Rol>();
            Debug.WriteLine("[Navbar] raw roles: " + string.Join(", ", raw.Select(r => $"{r.RolId}:{r.Tipo}")));

            // Defensive deduplication by RolId
            var distinctRoles = raw
                .GroupBy(r => r.RolId)
                .Select(g => g.First())
                .ToList();

            if (distinctRoles.Count != raw.Count)
                Debug.WriteLine($"[Navbar] removed {raw.Count - distinctRoles.Count} duplicate role(s).");

            // Populate observable collection (UI)
            Roles.Clear();
            foreach (var r in distinctRoles)
                Roles.Add(r);

            Debug.WriteLine("[Navbar] final Roles collection count: " + Roles.Count);

            // Set selected role: prefer stored/logged role, otherwise first available
            if (Roles.Count > 0)
            {
                var loggedRole = await _dbService.GetLoggedUserRoleAsync();
                Debug.WriteLine("[Navbar] loggedRole from DB: " + (loggedRole != null ? $"{loggedRole.RolId}:{loggedRole.Tipo}" : "null"));

                if (loggedRole != null)
                    RolSeleccionado = Roles.FirstOrDefault(x => x.RolId == loggedRole.RolId) ?? Roles.First();
                else
                    RolSeleccionado = Roles.First();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading roles: {ex}");
        }
    }

    private void ActualizarPermisosSegunRol(string rol)
    {
        switch (rol)
        {
            case "Administrador":
                // admin features
                break;
            case "Usuario":
                // basic features
                break;
            case "Invitado":
                // limited features
                break;
            case "Supervisor":
                // supervisor features
                break;
        }
    }
}