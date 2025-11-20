// src/pages/Novedades.jsx
import React, { useEffect, useState } from "react";
import Navbar from "../components/Navbar";
import { useAuth, toApi } from "../services/AuthService.jsx";
import "../styles/Novedades.css";

export default function Novedades({ isLoggedIn, onToggle }) {
  const { user, loading: authLoading } = useAuth();

  const [novedades, setNovedades] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [selected, setSelected] = useState(null);
  const [tipoFilter, setTipoFilter] = useState("all");

  const loggedIn = !!user;

  useEffect(() => {
    if (authLoading) return;

    if (!loggedIn) {
      setLoading(false);
      return;
    }

    const load = async () => {
      try {
        setLoading(true);
        setError(null);

        const res = await fetch(toApi("/novedades?page=1&pageSize=50"), {
          method: "GET",
          credentials: "include",
        });

        if (!res.ok) {
          const txt = await res.text();
          throw new Error(txt || "Error cargando novedades");
        }

        const data = await res.json();

        let items = [];
        if (Array.isArray(data)) items = data;
        else if (Array.isArray(data.items)) items = data.items;
        else if (Array.isArray(data.item1)) items = data.item1;

        setNovedades(items);
      } catch (err) {
        console.error(err);
        setError(err.message || "Error al cargar novedades");
      } finally {
        setLoading(false);
      }
    };

    load();
  }, [authLoading, loggedIn]);

  // --------- LOADING ----------
  if (authLoading || loading) {
    return (
      <>
        <Navbar isLoggedIn={loggedIn ?? isLoggedIn} onToggle={onToggle} />
        <div className="novedades-container">
          <h2>Novedades</h2>
          <p>Cargando novedades…</p>
        </div>
      </>
    );
  }

  // --------- NO LOGGED IN ----------
  if (!loggedIn) {
    return (
      <>
        <Navbar isLoggedIn={false} onToggle={onToggle} />
        <div className="novedades-container">
          <h2>Novedades</h2>
          <p>Debes iniciar sesión para ver las novedades.</p>
        </div>
      </>
    );
  }

  // --------- DERIVED DATA ----------
  const noHayNovedadesOriginal = !novedades || novedades.length === 0;

  // tipos disponibles para el combo
  const tiposSet = new Set();
  for (const n of novedades) {
    const label = mapTipo(n.tipo);
    if (label && label !== "—") tiposSet.add(label);
  }
  const tiposDisponibles = Array.from(tiposSet);

  // filtro por tipo
  const novedadesFiltradasPorTipo =
    tipoFilter === "all"
      ? novedades
      : novedades.filter((n) => mapTipo(n.tipo) === tipoFilter);

  // REMOVE NO-PUBLICADA ITEMS
  const novedadesPublicadas = novedadesFiltradasPorTipo.filter(
    (n) => getEstadoNovedad(n).code !== "no-publicada"
  );

  // separar vigentes / no vigentes
  const hoy = new Date();
  hoy.setHours(0, 0, 0, 0);

  const vigentes = [];
  const noVigentes = [];

  for (const n of novedadesPublicadas) {
    const hastaStr = getHasta(n);
    const dHasta = hastaStr ? new Date(hastaStr) : null;

    if (dHasta && !isNaN(dHasta.getTime()) && dHasta < hoy) {
      noVigentes.push(n);
    } else {
      vigentes.push(n);
    }
  }

  const noHayConFiltro = vigentes.length === 0 && noVigentes.length === 0;

  const renderNovedadCard = (n) => {
    const estado = getEstadoNovedad(n);
    const desde = getDesde(n);
    const hasta = getHasta(n);

    return (
      <article
        key={n.id || n.novedadId}
        className="novedad-item"
        onClick={() => setSelected(n)}
      >
        <header className="novedad-header">
          <h3 className="novedad-title">{n.titulo}</h3>
          <span className={`novedad-badge estado-${estado.code}`}>
            {estado.label}
          </span>
        </header>

        {n.tipo !== undefined && (
          <p className="novedad-tipo">
            Tipo: <strong>{mapTipo(n.tipo)}</strong>
          </p>
        )}

        {n.contenido && <p className="novedad-contenido">{n.contenido}</p>}

        <footer className="novedad-footer">
          <span>Desde: {formatDMY(desde)}</span>
          <span>Hasta: {formatDMY(hasta)}</span>
        </footer>
      </article>
    );
  };

  // --------- RENDER ----------
  return (
    <>
      <Navbar isLoggedIn={true} onToggle={onToggle} />

      <div className="novedades-container">
        <h2>Novedades</h2>

        {error && <p className="novedades-error">{error}</p>}

        {noHayNovedadesOriginal && !error && (
          <p className="novedades-empty">
            No hay novedades configuradas por ahora.
          </p>
        )}

        {!noHayNovedadesOriginal && (
          <>
            {/* Filtros */}
            <div className="novedades-filtros">
              <label>
                Filtrar por tipo:{" "}
                <select
                  value={tipoFilter}
                  onChange={(e) => setTipoFilter(e.target.value)}
                >
                  <option value="all">Todos</option>
                  {tiposDisponibles.map((t) => (
                    <option key={t} value={t}>
                      {t}
                    </option>
                  ))}
                </select>
              </label>
            </div>

            {noHayConFiltro && !error && (
              <p className="novedades-empty">
                No hay novedades para el filtro seleccionado.
              </p>
            )}

            {!noHayConFiltro && (
              <>
                {/* Novedades vigentes */}
                {vigentes.length > 0 && (
                  <>
                    <h3 className="novedades-subtitle">Novedades vigentes</h3>
                    <div className="novedades-list">
                      {vigentes.map(renderNovedadCard)}
                    </div>
                  </>
                )}

                {/* Novedades no vigentes */}
                {noVigentes.length > 0 && (
                  <>
                    <h3 className="novedades-subtitle">
                      Novedades no vigentes
                    </h3>
                    <div className="novedades-list novedades-list-no-vigentes">
                      {noVigentes.map(renderNovedadCard)}
                    </div>
                  </>
                )}
              </>
            )}
          </>
        )}
      </div>

      {/* MODAL */}
      {selected && (
        <div
          className="novedades-modal-overlay"
          onClick={() => setSelected(null)}
        >
          <div className="novedades-modal" onClick={(e) => e.stopPropagation()}>
            <header className="novedades-modal-header">
              <h3>{selected.titulo}</h3>
              <button
                className="novedades-modal-close"
                onClick={() => setSelected(null)}
              >
                ✕
              </button>
            </header>

            <p className="novedades-modal-subtitle">
              Tipo: <strong>{mapTipo(selected.tipo)}</strong>
            </p>

            {selected.contenido && (
              <p className="novedades-modal-content">{selected.contenido}</p>
            )}

            <div className="novedades-modal-row">
              <span>
                Estado: <strong>{getEstadoNovedad(selected).label}</strong>
              </span>
            </div>

            <div className="novedades-modal-row">
              <span>Desde: {formatDMY(getDesde(selected))}</span>
              <span>Hasta: {formatDMY(getHasta(selected))}</span>
            </div>

            {selected.creadoEnUtc && (
              <div className="novedades-modal-row">
                <span>Creado el: {formatDMY(selected.creadoEnUtc)}</span>
              </div>
            )}

            <div className="novedades-modal-row">
              <span>
                Publicado:{" "}
                <strong>
                  {selected.publicado ?? selected.Publicado ? "Sí" : "No"}
                </strong>
              </span>
            </div>

            <div className="novedades-modal-footer">
              <button
                className="novedades-modal-btn"
                onClick={() => setSelected(null)}
              >
                Cerrar
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}

/* ---------- HELPERS ---------- */

function mapTipo(t) {
  if (t === undefined || t === null) return "—";

  if (typeof t === "string") return t;

  switch (t) {
    case 1:
      return "Beneficio";
    case 2:
      return "Comunicado";
    case 3:
      return "Campaña";
    default:
      return `Tipo ${t}`;
  }
}

function getDesde(n) {
  return (
    n.desdeUtc ??
    n.DesdeUtc ??
    n.publicadoDesdeUtc ??
    n.PublicadoDesdeUtc ??
    n.desde ??
    n.fechaInicio ??
    n.inicio ??
    null
  );
}

function getHasta(n) {
  return (
    n.hastaUtc ??
    n.HastaUtc ??
    n.publicadoHastaUtc ??
    n.PublicadoHastaUtc ??
    n.hasta ??
    n.fechaFin ??
    n.fin ??
    null
  );
}

function getEstadoNovedad(n) {
  const publicado = n.publicado ?? n.Publicado ?? false;

  const desdeStr = getDesde(n);
  const hastaStr = getHasta(n);

  const now = new Date();
  const dDesde = desdeStr ? new Date(desdeStr) : null;
  const dHasta = hastaStr ? new Date(hastaStr) : null;

  if (!publicado) return { code: "no-publicada", label: "No publicada" };
  if (dHasta && now > dHasta) return { code: "finalizada", label: "Finalizada" };
  if (dDesde && now < dDesde) return { code: "programada", label: "Programada" };

  return { code: "activa", label: "Activa" };
}

function formatDMY(dateStr) {
  if (!dateStr) return "—";
  const d = new Date(dateStr);
  if (isNaN(d)) return "—";
  const dd = String(d.getDate()).padStart(2, "0");
  const mm = String(d.getMonth() + 1).padStart(2, "0");
  const yyyy = d.getFullYear();
  return `${dd}/${mm}/${yyyy}`;
}
