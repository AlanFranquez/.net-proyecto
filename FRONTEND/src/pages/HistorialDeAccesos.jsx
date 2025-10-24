import React, { useMemo, useState } from "react";
import Navbar from "../components/Navbar";
import "../styles/HistorialDeAccesos.css";

const RAW = [
  {
    id: "a1",
    fecha: "2025-10-10",
    hora: "08:15",
    espacio: "Biblioteca",
    estado: "permitido",
    detalle: {
      costo: 0,
      beneficio: "Sí",
      motivo: "-",
      punto: "Entrada B",
      metodo: "NFC",
    },
  },
  {
    id: "a2",
    fecha: "2025-10-09",
    hora: "17:30",
    espacio: "Lab. Informát.",
    estado: "denegado",
    detalle: {
      costo: 200,
      beneficio: "No",
      motivo: "Fuera de horario",
      punto: "Entrada A",
      metodo: "QR",
    },
  },
  {
    id: "a3",
    fecha: "2025-10-09",
    hora: "12:10",
    espacio: "Comedor",
    estado: "permitido",
    detalle: {
      costo: 0,
      beneficio: "Sí",
      motivo: "-",
      punto: "Puerta 1",
      metodo: "NFC",
    },
  },
  {
    id: "a4",
    fecha: "2025-10-08",
    hora: "19:45",
    espacio: "Biblioteca",
    estado: "denegado",
    detalle: {
      costo: 0,
      beneficio: "No",
      motivo: "Saldo insuficiente",
      punto: "Entrada C",
      metodo: "QR",
    },
  },
];

export default function HistorialDeAccesos({ isLoggedIn = true, onToggle }) {
  const [q, setQ] = useState("");
  const [fecha, setFecha] = useState("");
  const [espacio, setEspacio] = useState("");
  const [info, setInfo] = useState(null); // registro seleccionado

  const items = useMemo(() => {
    return RAW.filter((r) => {
      if (q.trim()) {
        const s = q.trim().toLowerCase();
        const hay =
          r.espacio.toLowerCase().includes(s) ||
          r.estado.toLowerCase().includes(s) ||
          (r.fecha + " " + r.hora).includes(s);
        if (!hay) return false;
      }
      if (fecha && r.fecha !== fecha) return false;
      if (espacio && r.espacio !== espacio) return false;
      return true;
    });
  }, [q, fecha, espacio]);

  // espacios únicos para el filtro
  const espacios = useMemo(
    () => Array.from(new Set(RAW.map((r) => r.espacio))),
    []
  );

  return (
    <>
      <Navbar isLoggedIn={isLoggedIn} onToggle={onToggle} />

      <main className="ha-wrap">
        <section className="ha-card">
          <h1 className="ha-title">Historial de Accesos</h1>

          <div className="ha-filters">
            <div className="f-col">
              <div className="f-icon">🔎</div>
              <label>Buscar</label>
              <input
                type="text"
                value={q}
                onChange={(e) => setQ(e.target.value)}
                placeholder="Texto libre…"
              />
            </div>

            <div className="f-col">
              <div className="f-icon">📅</div>
              <label>Filtro por fecha</label>
              <input
                type="date"
                value={fecha}
                onChange={(e) => setFecha(e.target.value)}
              />
            </div>

            <div className="f-col">
              <div className="f-icon">⤵️</div>
              <label>Espacio</label>
              <select
                value={espacio}
                onChange={(e) => setEspacio(e.target.value)}
              >
                <option value="">Todos</option>
                {espacios.map((e) => (
                  <option key={e} value={e}>
                    {e}
                  </option>
                ))}
              </select>
            </div>
          </div>

          <ul className="ha-list">
            {items.map((r) => (
              <li key={r.id} className="ha-item" onClick={() => setInfo(r)}>
                <div className="ha-main">
                  <div className="ha-when">
                    {formatDMY(r.fecha)} {r.hora}
                  </div>
                  <div className="ha-space">{r.espacio}</div>
                </div>
                <span className={`ha-chip ${r.estado}`}>
                  {r.estado === "permitido" ? "✔ Permitado" : "✖ Denegado"}
                </span>
              </li>
            ))}
            {items.length === 0 && (
              <li className="ha-empty">Sin resultados con los filtros.</li>
            )}
          </ul>
        </section>
      </main>

      {info && (
        <div className="ha-modal-overlay" onClick={() => setInfo(null)}>
          <div className="ha-modal" onClick={(e) => e.stopPropagation()}>
            <div className="ha-modal-head">
              <h3>Información</h3>
              <button className="close" onClick={() => setInfo(null)}>
                ✕
              </button>
            </div>

            <div className="ha-grid">
              <span className="k">Costo</span>
              <span>${info.detalle.costo}</span>

              <span className="k">Beneficio</span>
              <span>{info.detalle.beneficio}</span>

              <span className="k">Estado</span>
              <span>{info.estado === "permitido" ? "Permitido" : "Denegado"}</span>

              <span className="k">Motivo</span>
              <span>{info.detalle.motivo}</span>

              <span className="k">Espacio</span>
              <span>{info.espacio}</span>

              <span className="k">Punto de control</span>
              <span>{info.detalle.punto}</span>

              <span className="k">Método</span>
              <span>{info.detalle.metodo}</span>
            </div>

            <button className="ha-btn" onClick={() => setInfo(null)}>
              Cerrar
            </button>
          </div>
        </div>
      )}
    </>
  );
}

function formatDMY(iso) {
  const d = new Date(iso + "T00:00:00");
  const dd = String(d.getDate()).padStart(2, "0");
  const mm = String(d.getMonth() + 1).padStart(2, "0");
  const yyyy = d.getFullYear();
  return `${dd}/${mm}/${yyyy}`;
}
