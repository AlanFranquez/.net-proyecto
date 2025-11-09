using System;
using Microsoft.Maui.Networking;

namespace AppNetCredenciales.Services
{
    public class ConnectivityService : IDisposable
    {
        private bool _lastConnected;

        public event EventHandler<bool>? ConnectivityChanged;

        public ConnectivityService()
        {
            _lastConnected = IsConnected;
            Connectivity.Current.ConnectivityChanged += OnConnectivityChanged;
        }

        public bool IsConnected => Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

        private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
        {
            var connected = e.NetworkAccess == NetworkAccess.Internet;
            if (connected != _lastConnected)
            {
                _lastConnected = connected;
                ConnectivityChanged?.Invoke(this, connected);
            }
        }

        public void Dispose()
        {
            Connectivity.Current.ConnectivityChanged -= OnConnectivityChanged;
        }
    }
}   