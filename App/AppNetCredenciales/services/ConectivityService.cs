using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppNetCredenciales.services
{



    public class ConectivityService
    {
        private readonly IConnectivity _connectivity;
        private bool conectado;

        public ConectivityService()
        {
            _connectivity = Connectivity.Current;
            conectado = _connectivity.NetworkAccess == NetworkAccess.Internet;

            _connectivity.ConnectivityChanged += OnConnectivityChanged;
        }


        private void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            var isNowConnected = e.NetworkAccess == NetworkAccess.Internet;

            if (conectado != isNowConnected)
            {
                this.conectado = isNowConnected;
                this.mostrarAlerta(isNowConnected);
            }
        }
        public async Task mostrarAlerta(bool conectado)
        {
            
            if(!conectado)
            {
                await Shell.Current.DisplayAlert("Sin conexión", "No hay conexión a Internet. Algunas funciones pueden no estar disponibles.", "OK");
            }
        }




    }
}
