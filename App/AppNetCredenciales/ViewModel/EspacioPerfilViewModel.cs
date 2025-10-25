using AppNetCredenciales.Data;
using AppNetCredenciales.services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppNetCredenciales.ViewModel
{
    public class EspacioPerfilViewModel
    {
        private readonly AuthService _authService;
        private readonly LocalDBService _db;
        public EspacioPerfilViewModel(AuthService auth, LocalDBService db)
        {
            this._db = db;
            this._authService = auth;
        }


    }
}
