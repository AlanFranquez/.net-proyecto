using AppNetCredenciales.Data;
using AppNetCredenciales.models;
using AppNetCredenciales.services;
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

            var credencial = new Credencial
            {
                Tipo = CredencialTipo.Campus,
                Estado = CredencialEstado.Emitida,
                IdCriptografico = Guid.NewGuid().ToString("N"),
                FechaEmision = DateTime.UtcNow
            };

            // Save and retrieve the actual PK from the object (SaveCredencialAsync now returns the PK)
            var credId = await _db.SaveCredencialAsync(credencial);
            if (credId <= 0)
            {
                Trabajando = false;
                await view.DisplayAlert("Error", "No se pudo generar la credencial", "OK");
                return false;
            }

            usuario.CredencialId = credId;
            usuario.Credencial = credencial;

            var saved = await _db.SaveUsuarioAsync(usuario);
            if (saved <= 0)
            {
                Trabajando = false;
                await view.DisplayAlert("Error", "No se pudo guardar el usuario", "OK");
                return false;
            }
            var usuarioId = usuario.UsuarioId;
            if (usuarioId == 0) usuarioId = saved;

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
