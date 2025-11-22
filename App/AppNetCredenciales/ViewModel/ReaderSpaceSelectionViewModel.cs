using AppNetCredenciales.Data;
using AppNetCredenciales.models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace AppNetCredenciales.ViewModel
{
    public class ReaderSpaceSelectionViewModel : INotifyPropertyChanged
    {
        private readonly LocalDBService _db;
        private bool _isLoading;
        private string _funcionarioNombre;
        private ObservableCollection<Espacio> _espacios;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public string FuncionarioNombre
        {
            get => _funcionarioNombre;
            set
            {
                _funcionarioNombre = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Espacio> Espacios
        {
            get => _espacios;
            set
            {
                _espacios = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasEspacios));
                OnPropertyChanged(nameof(HasNoEspacios));
            }
        }

        public bool HasEspacios => Espacios?.Count > 0;
        public bool HasNoEspacios => !HasEspacios && !IsLoading;

        public ICommand ActivateReaderCommand { get; }
        public ICommand BackCommand { get; }

        public ReaderSpaceSelectionViewModel(LocalDBService db)
        {
            _db = db;
            _espacios = new ObservableCollection<Espacio>();
            
            ActivateReaderCommand = new Command<Espacio>(async (espacio) => await ActivateReaderAsync(espacio));
            BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));

            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;

                // Obtener usuario actual
                var usuario = await _db.GetLoggedUserAsync();
                if (usuario != null)
                {
                    FuncionarioNombre = usuario.Nombre ?? usuario.Email;
                }

                // Obtener espacios
                var espacios = await _db.GetEspaciosAsync();
                Espacios = new ObservableCollection<Espacio>(espacios ?? new List<Espacio>());

                System.Diagnostics.Debug.WriteLine($"[ReaderSpaceSelection] Cargados {Espacios.Count} espacios");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ReaderSpaceSelection] Error cargando datos: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "No se pudieron cargar los espacios", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ActivateReaderAsync(Espacio? espacio)
        {
            if (espacio == null) return;

            try
            {
                System.Diagnostics.Debug.WriteLine($"[ReaderSpaceSelection] Activando lector para espacio: {espacio.Nombre}");

                // Navegar a la vista del lector activo
                await Shell.Current.GoToAsync($"nfcReaderActive?espacioId={espacio.idApi}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ReaderSpaceSelection] Error activando lector: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "No se pudo activar el lector", "OK");
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
