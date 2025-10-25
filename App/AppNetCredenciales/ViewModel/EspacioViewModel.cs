using AppNetCredenciales.Data;
using AppNetCredenciales.models;
using AppNetCredenciales.services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace AppNetCredenciales.ViewModel
{
    public class EspacioViewModel
    {
        private readonly AuthService _authService;
        private readonly LocalDBService _db;

        public ObservableCollection<Espacio> Espacios { get; } = new();

        public EspacioViewModel(AuthService auth, LocalDBService db)
        {
            this._authService = auth;
            this._db = db;
        }

        public async Task LoadEspaciosAsync()
        {
            var lista = await _db.GetEspaciosAsync();

            if (lista == null || lista.Count == 0)
            {
                var ejemplos = new List<Espacio>
                {
                    new Espacio
                    {
                        Titulo = "Charla: Innovación en Tecnología Médica",
                        Descripcion = "Evento sobre las últimas tendencias en equipamiento médico e IA aplicada a la salud.",
                        Fecha = DateTime.Now.AddDays(2).AddHours(14),
                        Lugar = "Auditorio Principal",
                        Stock = 50,
                        Disponible = true,
                        Publicado = true
                    },
                    new Espacio
                    {
                        Titulo = "Taller de Desarrollo con .NET MAUI",
                        Descripcion = "Aprendé a crear apps multiplataforma con .NET MAUI, desde cero.",
                        Fecha = DateTime.Now.AddDays(5).AddHours(10),
                        Lugar = "Sala de Informática 2",
                        Stock = 25,
                        Disponible = true,
                        Publicado = true
                    },
                    new Espacio
                    {
                        Titulo = "Conferencia de Ciberseguridad 2025",
                        Descripcion = "Expertos locales e internacionales presentan casos y desafíos de seguridad digital.",
                        Fecha = DateTime.Now.AddDays(10).AddHours(9),
                        Lugar = "Centro de Convenciones",
                        Stock = 100,
                        Disponible = false,
                        Publicado = true
                    }
                };

                foreach (var e in ejemplos)
                    await _db.SaveEspacioAsync(e);

                lista = await _db.GetEspaciosAsync();
            }

            Espacios.Clear();
            foreach (var espacio in lista.OrderBy(e => e.Fecha))
                Espacios.Add(espacio);
        }

        // kept existing helpers if needed
        public async Task<List<Espacio>> GetEspaciosAsync() => await _db.GetEspaciosAsync();
        public async Task<Espacio> GetEspacioByIdAsync(int id) => await _db.GetEspacioByIdAsync(id);
        public async Task<List<Espacio>> GetEspaciosFuturos() => await _db.GetEspaciosFuturos();
    }
}