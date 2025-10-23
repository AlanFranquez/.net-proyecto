using AppNetCredenciales.services;
using AppNetCredenciales.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppNetCredenciales.ViewModel
{
    public class EventoViewModel
    {

        private readonly AuthService _authService;
        private readonly EventoView _eventoView;



        public EventoViewModel(EventoView ev, AuthService auth)
        {
            this._authService = auth;
            this._eventoView = ev;
        }


     
    }
}
