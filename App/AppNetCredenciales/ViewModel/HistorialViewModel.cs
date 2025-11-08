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

        public async Task LoadAccesosAsync()
        {
            Accesos.Clear();
            var usuario = await _authService.GetUserLogged();
            if (usuario != null)
            {
                var accesos = await _db.GetEventosAccesoByUsuarioIdAsync(usuario.CredencialId)
                              ?? new List<EventoAcceso>();

                foreach (var a in accesos)
                {
                    // If Espacio is missing but EspacioId exists, load it
                    if ((a.Espacio == null || string.IsNullOrWhiteSpace(a.Espacio.Titulo)) && a.EspacioId != 0)
                    {
                        var espacio = await _db.GetEspacioByIdAsync(a.EspacioId);
                        if (espacio != null)
                            a.Espacio = espacio;
                    }

                    Accesos.Add(a);
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}