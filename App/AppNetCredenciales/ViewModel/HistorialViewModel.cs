using AppNetCredenciales.Data;
using AppNetCredenciales.models;
using AppNetCredenciales.services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AppNetCredenciales.ViewModel
{
    public class HistorialViewModel : INotifyPropertyChanged
    {
        private readonly AuthService _authService;
        private readonly LocalDBService _db;

        public ObservableCollection<EventoAcceso> Accesos { get; } = new();

        public HistorialViewModel(AuthService auth, LocalDBService db)
        {
            _authService = auth;
            _db = db;
        }

        

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}