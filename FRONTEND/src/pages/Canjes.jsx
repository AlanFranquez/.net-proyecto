import React, { useMemo, useState } from "react";
import Navbar from "../components/Navbar";
import "../styles/Canjes.css";

const RAW_ITEMS = [
  { id: "12345", fecha: "2025-10-31", estado: "pendiente",  monto: 0,  metodo: "NFC",     corr: "abc123", beneficio: "Comedor Gratuito" },
  { id: "12344", fecha: "2025-10-30", estado: "completado", monto: 50, metodo: "Efectivo", corr: "def456", beneficio: "Completado" },
  { id: "12343", fecha: "2025-10-29", estado: "cancelado",  monto: 0,  metodo: "NFC",     corr: "ghi789", beneficio: "Cancelado" },
  { id: "12342", fecha: "2025-10-28", estado: "completado", monto: 30, metodo: "Efectivo", corr: "jkl012", beneficio: "Completado" },
  { id: "12341", fecha: "2025-10-27", estado: "pendiente",  monto: 0,  metodo: "NFC",     corr: "mno345", beneficio: "—" },
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

      // rango inclusivo
      if (desde && r.fecha < desde) return false;
      if (hasta && r.fecha > hasta) return false;

      return true;
    });
  }, [search, desde, hasta, estado]);

  return (
    <>
      <Navbar isLoggedIn={isLoggedIn} onToggle={onToggle} />

      <main className="canjes-wrap">
        <section className="canjes-card" aria-labelledby="cj-title">
          <h1 id="cj-title" className="canjes-title">Historial de Canjes</h1>

          {/* Filtros */}
          <div className="filters" role="region" aria-label="Filtros">
            <div className="search">
              <span className="search-icon" aria-hidden>🔎</span>
              <input
                placeholder="Buscar por ID, beneficio, método o corr."
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                aria-label="Buscar"
              />
            </div>

            <div className="date-range">
              <label className="sr-only" htmlFor="desde">Desde</label>
              <input id="desde" type="date" value={desde} onChange={(e) => setDesde(e.target.value)} />
              <span className="dash">—</span>
              <label className="sr-only" htmlFor="hasta">Hasta</label>
              <input id="hasta" type="date" value={hasta} onChange={(e) => setHasta(e.target.value)} />
            </div>

            <div className="status" role="group" aria-label="Estado">
              <button
                className={`chip ${estado === "pendiente" ? "on pending" : ""}`}
                onClick={() => setEstado(estado === "pendiente" ? "all" : "pendiente")}
              >
                Pendiente
              </button>
              <button
                className={`chip ${estado === "completado" ? "on done" : ""}`}
                onClick={() => setEstado(estado === "completado" ? "all" : "completado")}
              >
                Completado
              </button>
              <button
                className={`chip ${estado === "cancelado" ? "on cancel" : ""}`}
                onClick={() => setEstado(estado === "cancelado" ? "all" : "cancelado")}
              >
                Cancelado
              </button>
            </div>
          </div>

          {/* Tabla / Cards responsive */}
          <div className="table" role="table" aria-label="Resultados">
            <div className="thead" role="row">
              <div role="columnheader">Canje</div>
              <div role="columnheader">Fecha</div>
              <div role="columnheader">Estado</div>
              <div role="columnheader">Monto</div>
              <div role="columnheader">Método</div>
              <div role="columnheader">Correlation ID</div>
              <div role="columnheader"></div>
            </div>

            <div className="tbody">
              {items.map((r) => (
                <div key={r.id} className="row" role="row">
                  <div className="col-main" role="cell">
                    <div className="bold">Canje #{r.id}</div>
                    <div className="sub">Beneficio: {r.beneficio}</div>
                  </div>

                  <div className="col" data-label="Fecha" role="cell">
                    {formatDate(r.fecha)}
                  </div>

                  <div className="col" data-label="Estado" role="cell">
                    <span className={`badge ${r.estado}`}>{title(r.estado)}</span>
                  </div>

                  <div className="col" data-label="Monto" role="cell">
                    {formatMoney(r.monto)}
                  </div>

                  <div className="col" data-label="Método" role="cell">
                    {r.metodo}
                  </div>

                  <div className="col" data-label="Correlation ID" role="cell">
                    {r.corr}
                  </div>

                  <div className="col action" role="cell">
                    <button className="link" onClick={() => setDetalle(r)}>
                      Ver Detalle
                    </button>
                  </div>
                </div>
              ))}

              {items.length === 0 && (
                <div className="empty" role="note">Sin resultados con los filtros actuales.</div>
              )}
            </div>

            {/* Paginación fake */}
            <div className="pager" aria-live="polite">Página 1 de 5 ▸</div>
          </div>
        </section>
      </main>

      {/* Modal de detalle */}
      {detalle && (
        <div className="cj-modal-overlay" onClick={() => setDetalle(null)} role="dialog" aria-modal="true" aria-labelledby="cj-modal-title">
          <div className="cj-modal" onClick={(e) => e.stopPropagation()}>
            <h3 id="cj-modal-title">Detalle de Canje</h3>
            <div className="cj-grid">
              <span className="k">ID</span><span>{detalle.id}</span>
              <span className="k">Fecha</span><span>{formatDate(detalle.fecha)}</span>
              <span className="k">Estado</span><span>{title(detalle.estado)}</span>
              <span className="k">Beneficio</span><span>{detalle.beneficio}</span>
              <span className="k">Método</span><span>{detalle.metodo}</span>
              <span className="k">Correlation ID</span><span>{detalle.corr}</span>
              <span className="k">Monto</span><span>{formatMoney(detalle.monto)}</span>
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
  const d = new Date(iso + "T00:00:00");
  const dd = String(d.getDate()).padStart(2, "0");
  const mm = String(d.getMonth() + 1).padStart(2, "0");
  const yyyy = d.getFullYear();
  return `${dd}/${mm}/${yyyy}`;
}

function formatMoney(n) {
  // ajusta la moneda si necesitas otra
  return new Intl.NumberFormat("es-UY", { style: "currency", currency: "UYU", maximumFractionDigits: 0 }).format(n);
}
