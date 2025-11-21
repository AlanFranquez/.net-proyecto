using AppNetCredenciales.Data;
using AppNetCredenciales.models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AppNetCredenciales.ViewModel
{
    /// <summary>
    /// ViewModel para la pantalla de selección de espacio NFC
    /// </summary>
    public class NFCEspacioSelectionViewModel : INotifyPropertyChanged
    {
        private readonly LocalDBService _db;
        private ObservableCollection<EspacioViewModel> _espacios;
        private bool _noEspaciosDisponibles;
        private bool _isLoading;

        public ObservableCollection<EspacioViewModel> Espacios
        {
            get => _espacios;
            set { _espacios = value; OnPropertyChanged(); }
        }

        public bool NoEspaciosDisponibles
        {
            get => _noEspaciosDisponibles;
            set { _noEspaciosDisponibles = value; OnPropertyChanged(); }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public ICommand SelectEspacioCommand { get; }

        public NFCEspacioSelectionViewModel(LocalDBService db)
        {
            _db = db;
            _espacios = new ObservableCollection<EspacioViewModel>();
            SelectEspacioCommand = new Command<EspacioViewModel>(OnSelectEspacio);
        }

        /// <summary>
        /// Carga la lista de espacios disponibles
        /// </summary>
        public async Task LoadEspaciosAsync()
        {
            try
            {
                IsLoading = true;
                Espacios.Clear();

                Debug.WriteLine("[NFCEspacioSelectionVM] Cargando espacios...");
                var espacios = await _db.GetEspaciosAsync();

                if (espacios == null || espacios.Count == 0)
                {
                    Debug.WriteLine("[NFCEspacioSelectionVM] No hay espacios disponibles");
                    NoEspaciosDisponibles = true;
                    return;
                }

                Debug.WriteLine($"[NFCEspacioSelectionVM] {espacios.Count} espacios encontrados");
                
                foreach (var espacio in espacios)
                {
                    if (espacio.Activo)
                    {
                        Espacios.Add(new EspacioViewModel(espacio));
                    }
                }

                NoEspaciosDisponibles = Espacios.Count == 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[NFCEspacioSelectionVM] Error cargando espacios: {ex.Message}");
                NoEspaciosDisponibles = true;
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Maneja la selección de un espacio
        /// </summary>
        private async void OnSelectEspacio(EspacioViewModel espacioVm)
        {
            if (espacioVm == null) return;

            try
            {
                Debug.WriteLine($"[NFCEspacioSelectionVM] Espacio seleccionado: {espacioVm.Nombre} (ID: {espacioVm.EspacioId})");

                // Navegar a la vista del lector NFC con el ID del espacio
                await Shell.Current.GoToAsync($"nfc-reader?espacioId={espacioVm.EspacioId}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[NFCEspacioSelectionVM] Error navegando: {ex.Message}");
                await App.Current.MainPage.DisplayAlert("Error", "No se pudo abrir el lector NFC", "OK");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    /// <summary>
    /// ViewModel para mostrar un espacio en la lista
    /// </summary>
    public partial class EspacioViewModel
    {
        public int EspacioId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string TipoTexto { get; set; }
        public string TipoIcon { get; set; }

        public EspacioViewModel(Espacio espacio)
        {
            EspacioId = espacio.EspacioId;
            Nombre = espacio.Nombre ?? "Sin nombre";
            Descripcion = espacio.Descripcion ?? "Sin descripción";
            TipoTexto = ObtenerTipoTexto(espacio.Tipo);
            TipoIcon = ObtenerTipoIcono(espacio.Tipo);
        }

        private string ObtenerTipoTexto(EspacioTipo tipo)
        {
            return tipo switch
            {
                EspacioTipo.Aula => "Aula",
                EspacioTipo.Laboratorio => "Laboratorio",
                EspacioTipo.Biblioteca => "Biblioteca",
                EspacioTipo.Gimnasio => "Gimnasio",
                EspacioTipo.Auditorio => "Auditorio",
                _ => "Otro"
            };
        }

        private string ObtenerTipoIcono(EspacioTipo tipo)
        {
            return tipo switch
            {
                EspacioTipo.Aula => "??",
                EspacioTipo.Laboratorio => "??",
                EspacioTipo.Biblioteca => "??",
                EspacioTipo.Gimnasio => "???",
                EspacioTipo.Auditorio => "??",
                _ => "??"
            };
        }
    }
}
