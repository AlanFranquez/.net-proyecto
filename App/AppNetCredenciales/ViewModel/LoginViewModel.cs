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
                System.Diagnostics.Debug.WriteLine($"DATOS DE USURIO {u.UsuarioId} - {u.Email} - {u.idApi}");
                await SessionManager.SaveUserAsync(u.UsuarioId, Email, u.idApi);

                int idRol = u.RolId ?? 0;

                Rol r = await dbService.GetRolByIdAsync(idRol);

                if(r != null && r.Tipo == "Funcionario")
                {

                await Shell.Current.GoToAsync("scan");
                } else
                {
                    await Shell.Current.GoToAsync("espacio");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return true;
        }
    }
}