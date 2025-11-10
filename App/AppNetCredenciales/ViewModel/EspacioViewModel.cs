using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using AppNetCredenciales.Data;
using AppNetCredenciales.models;
using AppNetCredenciales.services;
using Microsoft.Maui.Controls;

namespace AppNetCredenciales.ViewModel;

public partial class EspacioViewModel : INotifyPropertyChanged
{
    private readonly AuthService _authService;
    private readonly LocalDBService _db;

    public ObservableCollection<Espacio> Espacios { get; } = new();

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); }
    }

    public Espacio SelectedEspacio { get; set; }

    public ICommand RefreshCommand { get; }

    public EspacioViewModel(AuthService auth, LocalDBService db)
    {
        _authService = auth;
        _db = db;
        RefreshCommand = new Command(async () => await LoadEspaciosAsync());
    }

    public async Task LoadEspaciosAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            var list = await _db.GetEspaciosAsync() ?? new List<Espacio>();
            Espacios.Clear();
            foreach (var e in list)
                Espacios.Add(e);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}