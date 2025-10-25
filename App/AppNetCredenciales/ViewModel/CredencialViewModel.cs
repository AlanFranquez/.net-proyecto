using AppNetCredenciales.Data;
using AppNetCredenciales.models;
using AppNetCredenciales.services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AppNetCredenciales.ViewModel;

public class CredencialViewModel : INotifyPropertyChanged
{
    private readonly LocalDBService _db;
    private readonly AuthService _auth;

    private Credencial _credencial;
    public Credencial Credencial
    {
        get => _credencial;
        set { _credencial = value; OnPropertyChanged(); }
    }

    public CredencialViewModel(AuthService auth, LocalDBService db)
    {
        _auth = auth;
        _db = db;
    }



    public async Task LoadCredencialAsync()
    {
        // 🔹 Obtener usuario logueado
        var usuario = await _auth.GetUserLogged();

        Credencial = usuario != null ?
            await _db.GetCredencialByIdAsync(usuario.CredencialId)
            : null;

        // 🔹 Si no existe, crear una credencial de ejemplo
        if (Credencial == null)
        {
            Credencial = new Credencial
            {
                Tipo = CredencialTipo.Campus,
                Estado = CredencialEstado.Emitida,
                IdCriptografico = "ABC123XYZ",
                FechaEmision = DateTime.Now,
                FechaExpiracion = DateTime.Now.AddYears(1)
            };
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
