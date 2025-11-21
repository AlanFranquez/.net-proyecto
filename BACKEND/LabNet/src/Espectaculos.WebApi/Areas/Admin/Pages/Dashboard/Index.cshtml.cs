using System;
using System.Threading;
using System.Threading.Tasks;
using Espectaculos.Domain.Entities;
using Espectaculos.Domain.Enums;
using Espectaculos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Espectaculos.Backoffice.Areas.Admin.Pages.Dashboard
{
    public class IndexModel : PageModel
    {
        private readonly EspectaculosDbContext _db;

        public IndexModel(EspectaculosDbContext db)
        {
            _db = db;
        }

        public DashboardVm Data { get; private set; } = new();

        public async Task OnGetAsync(CancellationToken ct)
        {
            // Usamos UTC en la DB y mostramos hora local en la vista
            var todayUtc = DateTime.UtcNow.Date;
            var firstDayMonthUtc = new DateTime(todayUtc.Year, todayUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var firstNextMonthUtc = firstDayMonthUtc.AddMonths(1);

            // 1) Usuarios activos
            var usuariosActivos = await _db.Usuario
                .CountAsync(u => u.Estado == UsuarioEstado.Activo, ct);

            // 2) "Reservas hoy": tomamos eventos de acceso ocurridos hoy (por fecha UTC)
            var reservasHoy = await _db.Set<EventoAcceso>()
                .CountAsync(e =>
                    e.MomentoDeAcceso.Date == todayUtc,
                    ct);

            // 3) "Beneficios usados": contamos canjes registrados
            var beneficiosUsados = await _db.Set<Canje>()
                .CountAsync(ct);

            // 4) Eventos del mes: eventos de acceso dentro del mes calendario actual
            var eventosDelMes = await _db.Set<EventoAcceso>()
                .CountAsync(e =>
                    e.MomentoDeAcceso >= firstDayMonthUtc &&
                    e.MomentoDeAcceso < firstNextMonthUtc,
                    ct);

            Data = new DashboardVm
            {
                UsuariosActivos   = usuariosActivos,
                ReservasHoy       = reservasHoy,
                BeneficiosUsados  = beneficiosUsados,
                EventosDelMes     = eventosDelMes,
                GeneratedAtLocal  = DateTime.Now
            };
        }

        public class DashboardVm
        {
            public int UsuariosActivos  { get; set; }
            public int ReservasHoy      { get; set; }
            public int BeneficiosUsados { get; set; }
            public int EventosDelMes    { get; set; }

            public DateTime GeneratedAtLocal { get; set; }
        }
    }
}
