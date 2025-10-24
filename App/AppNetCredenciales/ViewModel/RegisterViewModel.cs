using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AppNetCredenciales.models;
using AppNetCredenciales.services;
using AppNetCredenciales.Views;

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

        public RegisterViewModel(RegisterView view, AuthService authService)
        {
            this.view = view;
            this.authService = authService;
            RegisterCommand = new Command(async () => await RegisterAsync(), () => !trabajando);

            NavigateToLogin = new Command(async () =>
            {
                if (Application.Current?.MainPage is not null)
                {
                    await Shell.Current.GoToAsync("login");
                }
            });
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

            var registrado = await authService.registrarUsuario(usuario);

            if (!registrado)
            {
                Trabajando = false;
                await view.DisplayAlert("Error", "No se pudo registrar el usuario. Verifique los datos.", "OK");
                return false;
            }

            await view.DisplayAlert("Éxito", "Usuario registrado correctamente", "OK");

            // Vuelve al login
            await view.Navigation.PopAsync();

            Trabajando = false;
            return true;
        }
    }
}
