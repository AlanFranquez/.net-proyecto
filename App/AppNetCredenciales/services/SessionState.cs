using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AppNetCredenciales.models;

namespace AppNetCredenciales.Services
{
    // Shared application session state — single source of truth for current role
    public class SessionState : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private Rol? _currentRole;
        public Rol? CurrentRole
        {
            get => _currentRole;
            set
            {
                if (ReferenceEquals(_currentRole, value)) return;
                _currentRole = value;
                OnPropertyChanged();
            }
        }

        // Keep a shared list of roles (populated at startup or when needed)
        public ObservableCollection<Rol> Roles { get; } = new ObservableCollection<Rol>();

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}