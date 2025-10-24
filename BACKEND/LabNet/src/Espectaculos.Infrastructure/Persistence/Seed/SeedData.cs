using Espectaculos.Domain.Entities;

namespace Espectaculos.Infrastructure.Persistence.Seed;

public static class SeedData
{
    /*
    public static IEnumerable<Evento> GetEventosSeed()
    {
        {
            var list = new List<Evento>();
            var now = DateTime.UtcNow;


            list.AddRange([
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Vive Latino",
                    Descripcion = "Festival iberoamericano con rock, pop y fusiones alternativas.",
                    Fecha = now.AddDays(-10),
                    Lugar = "Foro Sol, Ciudad de México",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 90, StockTotal = 12000,
                            StockDisponible = 0
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 160, StockTotal = 6000,
                            StockDisponible = 0
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 280, StockTotal = 2000, StockDisponible = 0
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Corona Capital",
                    Descripcion = "Headliners internacionales de indie y rock alternativo.",
                    Fecha = DateTime.UtcNow.AddDays(25),
                    Lugar = "Autódromo Hermanos Rodríguez, CDMX",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 95, StockTotal = 14000,
                            StockDisponible = 14000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 180, StockTotal = 7000,
                            StockDisponible = 7000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 320, StockTotal = 2500, StockDisponible = 2500
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "EDC México",
                    Descripcion = "Electrónica y cultura rave con mega-escenarios.",
                    Fecha = DateTime.UtcNow.AddDays(40),
                    Lugar = "Autódromo Hermanos Rodríguez, CDMX",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 85, StockTotal = 20000,
                            StockDisponible = 20000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 150, StockTotal = 10000,
                            StockDisponible = 10000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 260, StockTotal = 4000, StockDisponible = 4000
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Pa'l Norte",
                    Descripcion = "Cartel ecléctico con actos latinos y globales.",
                    Fecha = DateTime.UtcNow.AddDays(55),
                    Lugar = "Parque Fundidora, Monterrey",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 80, StockTotal = 18000,
                            StockDisponible = 18000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 145, StockTotal = 9000,
                            StockDisponible = 9000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 250, StockTotal = 3500, StockDisponible = 3500
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Bahidorá",
                    Descripcion = "Experiencia boutique con música alternativa y arte.",
                    Fecha = DateTime.UtcNow.AddDays(18),
                    Lugar = "Las Estacas, Morelos",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 75, StockTotal = 6000,
                            StockDisponible = 6000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 120, StockTotal = 2500,
                            StockDisponible = 2500
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 210, StockTotal = 800, StockDisponible = 800
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Tecate Emblema",
                    Descripcion = "Pop contemporáneo con producción masiva.",
                    Fecha = DateTime.UtcNow.AddDays(34),
                    Lugar = "Autódromo Hermanos Rodríguez, CDMX",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 88, StockTotal = 15000,
                            StockDisponible = 15000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 155, StockTotal = 7000,
                            StockDisponible = 7000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 270, StockTotal = 2500, StockDisponible = 2500
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Tecate Coordenada",
                    Descripcion = "Rock y alternativo en el occidente del país.",
                    Fecha = DateTime.UtcNow.AddDays(60),
                    Lugar = "Guadalajara",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 70, StockTotal = 10000,
                            StockDisponible = 10000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 130, StockTotal = 5000,
                            StockDisponible = 5000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 230, StockTotal = 1800, StockDisponible = 1800
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Lollapalooza Chile",
                    Descripcion = "Multigénero con headliners globales.",
                    Fecha = DateTime.UtcNow.AddDays(45),
                    Lugar = "Parque Bicentenario de Cerrillos, Santiago",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 110, StockTotal = 22000,
                            StockDisponible = 22000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 190, StockTotal = 11000,
                            StockDisponible = 11000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 340, StockTotal = 4000, StockDisponible = 4000
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Lollapalooza Argentina",
                    Descripcion = "Clásico de Buenos Aires con gran producción.",
                    Fecha = DateTime.UtcNow.AddDays(47),
                    Lugar = "Hipódromo de San Isidro, Buenos Aires",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 115, StockTotal = 24000,
                            StockDisponible = 24000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 195, StockTotal = 12000,
                            StockDisponible = 12000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 360, StockTotal = 4500, StockDisponible = 4500
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Primavera Sound Buenos Aires",
                    Descripcion = "Sonido indie, electrónica y propuestas vanguardistas.",
                    Fecha = DateTime.UtcNow.AddDays(-400),
                    Lugar = "Parque Sarmiento, Buenos Aires",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 98, StockTotal = 15000,
                            StockDisponible = 15000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 170, StockTotal = 8000,
                            StockDisponible = 8000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 300, StockTotal = 2800, StockDisponible = 2800
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Cosquín Rock",
                    Descripcion = "Histórico encuentro del rock argentino.",
                    Fecha = DateTime.UtcNow.AddDays(-890),
                    Lugar = "Santa María de Punilla, Córdoba",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 65, StockTotal = 22000,
                            StockDisponible = 22000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 120, StockTotal = 9000,
                            StockDisponible = 9000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 210, StockTotal = 3000, StockDisponible = 3000
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Asunciónico",
                    Descripcion = "Festival de Paraguay con propuesta internacional.",
                    Fecha = DateTime.UtcNow.AddDays(33),
                    Lugar = "Asunción",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 72, StockTotal = 12000,
                            StockDisponible = 12000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 130, StockTotal = 6000,
                            StockDisponible = 6000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 220, StockTotal = 1800, StockDisponible = 1800
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Festival Estéreo Picnic",
                    Descripcion = "Bogotá recibe a grandes del indie y el pop.",
                    Fecha = DateTime.UtcNow.AddDays(52),
                    Lugar = "Bogotá",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 92, StockTotal = 20000,
                            StockDisponible = 20000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 165, StockTotal = 9000,
                            StockDisponible = 9000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 285, StockTotal = 3000, StockDisponible = 3000
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Rock al Parque",
                    Descripcion = "Evento gratuito/mixto emblemático de Bogotá.",
                    Fecha = DateTime.UtcNow.AddDays(75),
                    Lugar = "Parque Simón Bolívar, Bogotá",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 50, StockTotal = 30000,
                            StockDisponible = 30000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 95, StockTotal = 10000,
                            StockDisponible = 10000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 180, StockTotal = 3500, StockDisponible = 3500
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Festival Viña del Mar",
                    Descripcion = "El ícono latino con competencia y shows masivos.",
                    Fecha = DateTime.UtcNow.AddDays(180),
                    Lugar = "Quinta Vergara, Viña del Mar",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 70, StockTotal = 14000,
                            StockDisponible = 14000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 140, StockTotal = 7000,
                            StockDisponible = 7000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 260, StockTotal = 2200, StockDisponible = 2200
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Festival de la Canción de Punta del Este",
                    Descripcion = "Pop latino y artistas emergentes junto al mar.",
                    Fecha = now.AddDays(-32),
                    Lugar = "Punta del Este, Uruguay",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 60, StockTotal = 8000,
                            StockDisponible = 8000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 115, StockTotal = 4000,
                            StockDisponible = 4000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 210, StockTotal = 1200, StockDisponible = 1200
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Festival de Jazz de Montevideo",
                    Descripcion = "Noche de standards y fusiones contemporáneas.",
                    Fecha = DateTime.UtcNow.AddDays(28),
                    Lugar = "Teatro Solís, Montevideo",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 55, StockTotal = 2500,
                            StockDisponible = 2500
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 95, StockTotal = 1200, StockDisponible = 1200
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 160, StockTotal = 400, StockDisponible = 400
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Festival Marvin",
                    Descripcion = "Circuito urbano con showcases y conferencias.",
                    Fecha = DateTime.UtcNow.AddDays(38),
                    Lugar = "CDMX",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 45, StockTotal = 3000,
                            StockDisponible = 3000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 80, StockTotal = 1500, StockDisponible = 1500
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 140, StockTotal = 500, StockDisponible = 500
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Festival Internacional Cervantino – Música",
                    Descripcion = "Selección musical dentro del encuentro artístico más grande de MX.",
                    Fecha = DateTime.UtcNow.AddDays(110),
                    Lugar = "Guanajuato",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 40, StockTotal = 5000,
                            StockDisponible = 5000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 75, StockTotal = 2500, StockDisponible = 2500
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 130, StockTotal = 800, StockDisponible = 800
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "FIM GDL – Noche de Festivales",
                    Descripcion = "Showcase de talentos y cartel internacional.",
                    Fecha = DateTime.UtcNow.AddDays(22),
                    Lugar = "Guadalajara",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 35, StockTotal = 2000,
                            StockDisponible = 2000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 65, StockTotal = 1000, StockDisponible = 1000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 120, StockTotal = 350, StockDisponible = 350
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Primavera Sound São Paulo",
                    Descripcion = "Edición brasileña con curaduría indie.",
                    Fecha = DateTime.UtcNow.AddDays(58),
                    Lugar = "São Paulo",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 100, StockTotal = 16000,
                            StockDisponible = 16000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 180, StockTotal = 8000,
                            StockDisponible = 8000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 320, StockTotal = 3000, StockDisponible = 3000
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Planeta Atlântida",
                    Descripcion = "Pop y funk brasileño a gran escala.",
                    Fecha = DateTime.UtcNow.AddDays(76),
                    Lugar = "Rio Grande do Sul",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 68, StockTotal = 18000,
                            StockDisponible = 18000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 125, StockTotal = 9000,
                            StockDisponible = 9000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 220, StockTotal = 3200, StockDisponible = 3200
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Rock in Rio – Night",
                    Descripcion = "Noche temática del mega festival carioca.",
                    Fecha = DateTime.UtcNow.AddDays(140),
                    Lugar = "Parque Olímpico, Río de Janeiro",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 120, StockTotal = 35000,
                            StockDisponible = 35000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 210, StockTotal = 18000,
                            StockDisponible = 18000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 380, StockTotal = 6000, StockDisponible = 6000
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Festival de la Leyenda Vallenata",
                    Descripcion = "Competencias y conciertos de vallenato.",
                    Fecha = DateTime.UtcNow.AddDays(170),
                    Lugar = "Valledupar",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 55, StockTotal = 10000,
                            StockDisponible = 10000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 100, StockTotal = 5000,
                            StockDisponible = 5000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 180, StockTotal = 1800, StockDisponible = 1800
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Festival de Tango de Buenos Aires",
                    Descripcion = "Gala y milongas con orquestas en vivo.",
                    Fecha = DateTime.UtcNow.AddDays(85),
                    Lugar = "Buenos Aires",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 40, StockTotal = 5000,
                            StockDisponible = 5000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 75, StockTotal = 2500, StockDisponible = 2500
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 130, StockTotal = 900, StockDisponible = 900
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Festival Internacional de Jazz de Buenos Aires",
                    Descripcion = "Programación moderna y clásica con artistas globales.",
                    Fecha = DateTime.UtcNow.AddDays(112),
                    Lugar = "Buenos Aires",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 48, StockTotal = 4000,
                            StockDisponible = 4000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 90, StockTotal = 2000, StockDisponible = 2000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 150, StockTotal = 700, StockDisponible = 700
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Festival de la Vendimia – Noche de Rock",
                    Descripcion = "Edición especial con bandas nacionales.",
                    Fecha = DateTime.UtcNow.AddDays(125),
                    Lugar = "Mendoza",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 38, StockTotal = 6000,
                            StockDisponible = 6000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 70, StockTotal = 3000, StockDisponible = 3000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 120, StockTotal = 1000, StockDisponible = 1000
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Festival Quilmes Rock",
                    Descripcion = "Regreso del histórico festival con artistas mainstream.",
                    Fecha = DateTime.UtcNow.AddDays(72),
                    Lugar = "Buenos Aires",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 78, StockTotal = 15000,
                            StockDisponible = 15000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 140, StockTotal = 7000,
                            StockDisponible = 7000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 240, StockTotal = 2500, StockDisponible = 2500
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Movistar Fri Music",
                    Descripcion = "Noche urbana con pop y electrónica.",
                    Fecha = DateTime.UtcNow.AddDays(44),
                    Lugar = "Buenos Aires",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 52, StockTotal = 9000,
                            StockDisponible = 9000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 95, StockTotal = 4500, StockDisponible = 4500
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 170, StockTotal = 1500, StockDisponible = 1500
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Festival de Música Andina",
                    Descripcion = "Encuentro de sonoridades andinas y folklore contemporáneo.",
                    Fecha = DateTime.UtcNow.AddDays(50),
                    Lugar = "La Paz",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 35, StockTotal = 4000,
                            StockDisponible = 4000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 65, StockTotal = 2000, StockDisponible = 2000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 120, StockTotal = 700, StockDisponible = 700
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Festival del Pacífico – Noche Afro",
                    Descripcion = "Ritmos del Pacífico y fusión contemporánea.",
                    Fecha = DateTime.UtcNow.AddDays(67),
                    Lugar = "Buenaventura",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 30, StockTotal = 3000,
                            StockDisponible = 3000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 58, StockTotal = 1500, StockDisponible = 1500
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 110, StockTotal = 500, StockDisponible = 500
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Festival del Litoral",
                    Descripcion = "Chamamé y música del litoral argentino.",
                    Fecha = DateTime.UtcNow.AddDays(135),
                    Lugar = "Posadas",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 28, StockTotal = 3500,
                            StockDisponible = 3500
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 55, StockTotal = 1800, StockDisponible = 1800
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 100, StockTotal = 600, StockDisponible = 600
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Festival Internacional del Mate – Noche Urbana",
                    Descripcion = "Edición con artistas urbanos y pop regional.",
                    Fecha = DateTime.UtcNow.AddDays(145),
                    Lugar = "Entre Ríos",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 32, StockTotal = 4200,
                            StockDisponible = 4200
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 62, StockTotal = 2000, StockDisponible = 2000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 115, StockTotal = 700, StockDisponible = 700
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Festival Internacional de Folklore de Cosquín – Gala",
                    Descripcion = "Gala especial con artistas invitados.",
                    Fecha = now.AddDays(-5),
                    Lugar = "Córdoba",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 36, StockTotal = 4800,
                            StockDisponible = 4800
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 70, StockTotal = 2200, StockDisponible = 2200
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 125, StockTotal = 800, StockDisponible = 800
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Festival de la Patria Gaucha – Noche Fusión",
                    Descripcion = "Tradición y nuevas corrientes del folklore.",
                    Fecha = DateTime.UtcNow.AddDays(130),
                    Lugar = "Tacuarembó, Uruguay",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 26, StockTotal = 4200,
                            StockDisponible = 4200
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 50, StockTotal = 2000, StockDisponible = 2000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 95, StockTotal = 650, StockDisponible = 650
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Festival Punta Rock",
                    Descripcion = "Bandas uruguayas e invitados regionales.",
                    Fecha = DateTime.UtcNow.AddDays(70),
                    Lugar = "Punta del Este, Uruguay",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 42, StockTotal = 5000,
                            StockDisponible = 5000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 78, StockTotal = 2500, StockDisponible = 2500
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 135, StockTotal = 900, StockDisponible = 900
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Festival Internacional de Teatro Musical – Concierto",
                    Descripcion = "Selección de números con banda en vivo.",
                    Fecha = now.AddDays(-20),
                    Lugar = "Teatro Colón, Buenos Aires",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 58, StockTotal = 2800,
                            StockDisponible = 2800
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 110, StockTotal = 1400,
                            StockDisponible = 1400
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 190, StockTotal = 500, StockDisponible = 500
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "André Rieu – Live in Concert",
                    Descripcion = "Una noche mágica con André Rieu y su orquesta.",
                    Fecha = DateTime.UtcNow.AddDays(30),
                    Lugar = "Teatro Colón",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 150, StockTotal = 500, StockDisponible = 500
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 300, StockTotal = 200, StockDisponible = 200
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 600, StockTotal = 50, StockDisponible = 50
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Celtic Trad Fest",
                    Descripcion = "Música tradicional celta en vivo.",
                    Fecha = DateTime.UtcNow.AddDays(45),
                    Lugar = "Centro Cultural Recoleta",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 80, StockTotal = 400, StockDisponible = 400
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 160, StockTotal = 150, StockDisponible = 150
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Krishna Das – Kirtan Night",
                    Descripcion = "Mantras y kirtans con Krishna Das.",
                    Fecha = DateTime.UtcNow.AddDays(20),
                    Lugar = "Teatro Gran Rex",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 120, StockTotal = 300, StockDisponible = 300
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 250, StockTotal = 80, StockDisponible = 80
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Symphonic Rock Gala",
                    Descripcion = "Lo mejor del rock sinfónico.",
                    Fecha = DateTime.UtcNow.AddDays(60),
                    Lugar = "Luna Park",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 100, StockTotal = 600, StockDisponible = 600
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 200, StockTotal = 250, StockDisponible = 250
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 400, StockTotal = 70, StockDisponible = 70
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Festival Indie del Río de la Plata",
                    Descripcion = "Bandas emergentes de Uruguay y Argentina.",
                    Fecha = DateTime.UtcNow.AddDays(32),
                    Lugar = "Montevideo",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 34, StockTotal = 3500,
                            StockDisponible = 3500
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 60, StockTotal = 1800, StockDisponible = 1800
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 110, StockTotal = 600, StockDisponible = 600
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Noche Sinfónica Anime",
                    Descripcion = "Selección de bandas sonoras de anime con orquesta.",
                    Fecha = DateTime.UtcNow.AddDays(48),
                    Lugar = "Auditorio Nacional, CDMX",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 58, StockTotal = 5000,
                            StockDisponible = 5000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 110, StockTotal = 2500,
                            StockDisponible = 2500
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 190, StockTotal = 800, StockDisponible = 800
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Noche de Boleros – Big Band",
                    Descripcion = "Repertorio clásico con arreglos contemporáneos.",
                    Fecha = now.AddDays(-36),
                    Lugar = "Teatro Solís, Montevideo",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 44, StockTotal = 2200,
                            StockDisponible = 2200
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 82, StockTotal = 1100, StockDisponible = 1100
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 140, StockTotal = 380, StockDisponible = 380
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Noche Retro 80s",
                    Descripcion = "Síntesis, new wave y baile sin parar.",
                    Fecha = DateTime.UtcNow.AddDays(29),
                    Lugar = "Antel Arena, Montevideo",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 50, StockTotal = 6000,
                            StockDisponible = 0
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 95, StockTotal = 3000, StockDisponible = 0
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 170, StockTotal = 900, StockDisponible = 0
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Festival Urbano del Caribe",
                    Descripcion = "Reggaetón y afrobeat con line-up caribeño.",
                    Fecha = DateTime.UtcNow.AddDays(57),
                    Lugar = "Santo Domingo",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 70, StockTotal = 12000,
                            StockDisponible = 12000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 130, StockTotal = 6000,
                            StockDisponible = 6000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 220, StockTotal = 2200, StockDisponible = 2200
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Festival Andino de Rock",
                    Descripcion = "Rock con paisajes de altura y escenografía natural.",
                    Fecha = DateTime.UtcNow.AddDays(64),
                    Lugar = "Cusco",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 46, StockTotal = 5000,
                            StockDisponible = 5000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 85, StockTotal = 2500, StockDisponible = 2500
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 150, StockTotal = 800, StockDisponible = 800
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Festival Amazonía Viva",
                    Descripcion = "World music con enfoque en sostenibilidad.",
                    Fecha = DateTime.UtcNow.AddDays(90),
                    Lugar = "Manaus",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 42, StockTotal = 4500,
                            StockDisponible = 4500
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 78, StockTotal = 2200, StockDisponible = 2200
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 135, StockTotal = 700, StockDisponible = 700
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Festival Patagonia Sonora",
                    Descripcion = "Indie y folk con paisaje patagónico.",
                    Fecha = DateTime.UtcNow.AddDays(118),
                    Lugar = "Bariloche",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 52, StockTotal = 3000,
                            StockDisponible = 3000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 95, StockTotal = 1600, StockDisponible = 1600
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 170, StockTotal = 500, StockDisponible = 500
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Festival del Desierto – Electrónica",
                    Descripcion = "Experiencia inmersiva en entorno desértico.",
                    Fecha = DateTime.UtcNow.AddDays(126),
                    Lugar = "San Pedro de Atacama",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 88, StockTotal = 5000,
                            StockDisponible = 5000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 150, StockTotal = 2300,
                            StockDisponible = 0
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 260, StockTotal = 800, StockDisponible = 0
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Festival Caribe Jazz",
                    Descripcion = "Latin jazz y fusiones tropicales.",
                    Fecha = DateTime.UtcNow.AddDays(102),
                    Lugar = "Cartagena",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 48, StockTotal = 4200,
                            StockDisponible = 4200
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 90, StockTotal = 2000, StockDisponible = 2000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 160, StockTotal = 650, StockDisponible = 650
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Festival Pacífico Sonoro",
                    Descripcion = "Marimba, currulao y nuevas propuestas afro.",
                    Fecha = DateTime.UtcNow.AddDays(108),
                    Lugar = "Quibdó",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 30, StockTotal = 3000,
                            StockDisponible = 3000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 58, StockTotal = 1500, StockDisponible = 1500
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 110, StockTotal = 500, StockDisponible = 500
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Festival Norteño – Noche de Acordeones",
                    Descripcion = "Conjunto norteño con invitados especiales.",
                    Fecha = DateTime.UtcNow.AddDays(74),
                    Lugar = "Monterrey",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 36, StockTotal = 5000,
                            StockDisponible = 5000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 66, StockTotal = 2500, StockDisponible = 2500
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 120, StockTotal = 800, StockDisponible = 800
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Noche de Ópera – Arias Famosas",
                    Descripcion = "Solistas internacionales y orquesta.",
                    Fecha = DateTime.UtcNow.AddDays(200),
                    Lugar = "Teatro Municipal, Santiago",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 60, StockTotal = 2000,
                            StockDisponible = 100
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 110, StockTotal = 1000,
                            StockDisponible = 376
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 180, StockTotal = 350, StockDisponible = 20
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Sinfonietta – Música de Películas",
                    Descripcion = "Lo mejor del cine en formato sinfónico.",
                    Fecha = DateTime.UtcNow.AddDays(59),
                    Lugar = "Auditorio Nacional, CDMX",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 62, StockTotal = 6000,
                            StockDisponible = 0
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 115, StockTotal = 3000,
                            StockDisponible = 0
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 190, StockTotal = 900, StockDisponible = 0
                        },
                    }
                },
                new Evento
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Cumbia al Parque",
                    Descripcion = "Fiesta popular con clásicos y nuevas bandas.",
                    Fecha = DateTime.UtcNow.AddDays(53),
                    Lugar = "Bogotá",
                    Publicado = true,
                    Entradas = new List<Entrada>
                    {
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "General", Precio = 28, StockTotal = 8000,
                            StockDisponible = 300
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "Platea", Precio = 55, StockTotal = 4000, StockDisponible = 4000
                        },
                        new Entrada
                        {
                            Id = Guid.NewGuid(), Tipo = "VIP", Precio = 95, StockTotal = 1200, StockDisponible = 1200
                        },
                    }
                }
            ]);

            return list;
        }
    }*/
}
