import React, { useMemo, useState } from "react";
import Navbar from "../components/Navbar";
import "../styles/Canjes.css";

const RAW_ITEMS = [
  {
    id: "12345",
    fecha: "2025-10-31",
    estado: "pendiente",
    monto: 0,
    metodo: "NFC",
    corr: "abc123",
    beneficio: "Comedor Gratuito",
  },
  {
    id: "12344",
    fecha: "2025-10-30",
    estado: "completado",
    monto: 50,
    metodo: "Efectivo",
    corr: "def456",
    beneficio: "Completado",
  },
  {
    id: "12343",
    fecha: "2025-10-29",
    estado: "cancelado",
    monto: 0,
    metodo: "NFC",
    corr: "ghi789",
    beneficio: "Concelddo",
  },
  {
    id: "12342",
    fecha: "2025-10-28",
    estado: "completado",
    monto: 30,
    metodo: "Efectivo",
    corr: "jkl012",
    beneficio: "Completado",
  },
  {
    id: "12341",
    fecha: "2025-10-27",
    estado: "pendiente",
    monto: 0,
    metodo: "NFC",
    corr: "mno345",
    beneficio: "—",
  },
];

export default function Canjes({ isLoggedIn = true, onToggle }) {
  const [search, setSearch] = useState("");
  const [desde, setDesde] = useState("");
  const [hasta, setHasta] = useState("");
  const [estado, setEstado] = useState("all"); // all | pendiente | completado | cancelado
  const [detalle, setDetalle] = useState(null);

  const items = useMemo(() => {
    return RAW_ITEMS.filter((r) => {
      if (estado !== "all" && r.estado !== estado) return false;

      if (search.trim()) {
        const q = search.trim().toLowerCase();
        const hay =
          r.id.includes(q) ||
          r.beneficio.toLowerCase().includes(q) ||
          r.metodo.toLowerCase().includes(q) ||
          r.corr.toLowerCase().includes(q);
        if (!hay) return false;
      }

      // date range filter (inclusive)
      if (desde && r.fecha < desde) return false;
      if (hasta && r.fecha > hasta) return false;

      return true;
    });
  }, [search, desde, hasta, estado]);

  return (
    <>
      <Navbar isLoggedIn={isLoggedIn} onToggle={onToggle} />

      <main className="canjes-wrap">
        <section className="canjes-card">
          <h1 className="canjes-title">Historial de Canjes</h1>

          {/* Filtros */}
          <div className="filters">
            <div className="search">
              <span className="search-icon" aria-hidden>🔎</span>
              <input
                placeholder="Buscar"
                value={search}
                onChange={(e) => setSearch(e.target.value)}
              />
            </div>

            <div className="date-range">
              <label>Rango de fechas</label>
              <input
                type="date"
                value={desde}
                onChange={(e) => setDesde(e.target.value)}
              />
              <span className="dash">—</span>
              <input
                type="date"
                value={hasta}
                onChange={(e) => setHasta(e.target.value)}
              />
            </div>

            <div className="status">
              <button
                className={`chip ${estado === "pendiente" ? "on pending" : ""}`}
                onClick={() =>
                  setEstado(estado === "pendiente" ? "all" : "pendiente")
                }
              >
                Pendiente
              </button>
              <button
                className={`chip ${estado === "completado" ? "on done" : ""}`}
                onClick={() =>
                  setEstado(estado === "completado" ? "all" : "completado")
                }
              >
                Completado
              </button>
            </div>
          </div>

          {/* Tabla */}
          <div className="table">
            <div className="thead">
              <div>Fecha</div>
              <div>Fecha</div>
              <div>Estado</div>
              <div>Monto</div>
              <div>Método</div>
              <div>Correlation ID</div>
              <div></div>
            </div>

            <div className="tbody">
              {items.map((r) => (
                <div key={r.id} className="row">
                  <div className="col-main">
                    <div className="bold">Canje #{r.id}</div>
                    <div className="sub">Beneficio: {r.beneficio}</div>
                  </div>

                  <div className="col">{formatDate(r.fecha)}</div>

                  <div className="col">
                    <span className={`badge ${r.estado}`}>{title(r.estado)}</span>
                  </div>

                  <div className="col">${r.monto}</div>
                  <div className="col">{r.metodo}</div>
                  <div className="col">{r.corr}</div>

                  <div className="col action">
                    <button className="link" onClick={() => setDetalle(r)}>
                      Ver Detalle
                    </button>
                  </div>
                </div>
              ))}

              {items.length === 0 && (
                <div className="empty">Sin resultados con los filtros actuales.</div>
              )}
            </div>

            {/* Paginación fake */}
            <div className="pager">Página 1 de 5 ▸</div>
          </div>
        </section>
      </main>

      {/* Modal de detalle (placeholder) */}
      {detalle && (
        <div className="cj-modal-overlay" onClick={() => setDetalle(null)}>
          <div className="cj-modal" onClick={(e) => e.stopPropagation()}>
            <h3>Detalle de Canje</h3>
            <div className="cj-grid">
              <span className="k">ID</span>
              <span>{detalle.id}</span>
              <span className="k">Fecha</span>
              <span>{formatDate(detalle.fecha)}</span>
              <span className="k">Estado</span>
              <span>{title(detalle.estado)}</span>
              <span className="k">Beneficio</span>
              <span>{detalle.beneficio}</span>
              <span className="k">Método</span>
              <span>{detalle.metodo}</span>
              <span className="k">Correlation ID</span>
              <span>{detalle.corr}</span>
              <span className="k">Monto</span>
              <span>${detalle.monto}</span>
            </div>

            <div className="cj-actions">
              <button className="btn-close" onClick={() => setDetalle(null)}>
                Cerrar
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}

function title(s) {
  if (s === "pendiente") return "Pendiente";
  if (s === "completado") return "Completado";
  if (s === "cancelado") return "Cancelado";
  return s;
}

function formatDate(iso) {
  // muestra DD/MM/YYYY
  const d = new Date(iso + "T00:00:00");
  const dd = String(d.getDate()).padStart(2, "0");
  const mm = String(d.getMonth() + 1).padStart(2, "0");
  const yyyy = d.getFullYear();
  return `${dd}/${mm}/${yyyy}`;
}
