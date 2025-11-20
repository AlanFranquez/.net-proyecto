// src/pages/Notificaciones.jsx
import React, { useEffect, useState } from "react";
import Navbar from "../components/Navbar";
import { useAuth, toApi } from "../services/AuthService.jsx";
import "../styles/Notificaciones.css";

export default function Notificaciones({ isLoggedIn, onToggle }) {
  const { user, loading: authLoading } = useAuth();

  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [selected, setSelected] = useState(null);

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

        // ⬇⬇⬇ Traemos TODAS las notificaciones activas
        const res = await fetch(toApi("/notificaciones?onlyActive=true"), {
          method: "GET",
          credentials: "include",
        });

        if (!res.ok) {
          const txt = await res.text();
          throw new Error(txt || "Error cargando notificaciones");
        }

        const data = await res.json();

        let list = [];
        if (Array.isArray(data)) list = data;
        else if (Array.isArray(data.items)) list = data.items;
        else if (Array.isArray(data.item1)) list = data.item1;

        // id del usuario logueado (/usuarios/me)
        const usuarioId =
          user?.usuarioId ?? user?.UsuarioId ?? user?.id ?? null;
        const usuarioIdStr = usuarioId ? String(usuarioId) : null;

        // ⬇⬇⬇ Filtramos solo las del usuario
        const mine = list.filter((n) => {
          const nUserId = n.usuarioId ?? n.UsuarioId ?? null;
          if (!usuarioIdStr || !nUserId) return false;
          return String(nUserId) === usuarioIdStr;
        });

        setItems(mine);
      } catch (err) {
        console.error(err);
        setError(err.message || "Error al cargar notificaciones");
      } finally {
        setLoading(false);
      }
    };

    load();
  }, [authLoading, loggedIn, user?.usuarioId, user?.id]);

  // ---------- estados generales ----------
  if (authLoading || loading) {
    return (
      <>
        <Navbar isLoggedIn={loggedIn ?? isLoggedIn} onToggle={onToggle} />
        <div className="notifs-container">
          <h2>Notificaciones</h2>
          <p>Cargando notificaciones…</p>
        </div>
      </>
    );
  }

  if (!loggedIn) {
    return (
      <>
        <Navbar isLoggedIn={false} onToggle={onToggle} />
        <div className="notifs-container">
          <h2>Notificaciones</h2>
          <p>Debes iniciar sesión para ver tus notificaciones.</p>
        </div>
      </>
    );
  }

  const isEmpty = !items || items.length === 0;

  return (
    <>
      <Navbar isLoggedIn={true} onToggle={onToggle} />

      <div className="notifs-container">
        <h2>Notificaciones</h2>

        {error && <p className="notifs-error">{error}</p>}

        {isEmpty && !error && (
          <p className="notifs-empty">
            No tienes notificaciones activas en este momento.
          </p>
        )}

        {!isEmpty && (
          <div className="notifs-list">
            {items.map((n) => {
              const estado = mapEstado(n.estado ?? n.Estado);
              const when =
                n.programadaParaUtc ??
                n.ProgramadaParaUtc ??
                n.creadoEnUtc ??
                n.CreadoEnUtc;

              return (
                <article
                  key={n.notificacionId || n.id}
                  className="notif-item"
                  onClick={() => setSelected(n)}
                >
                  <div className="notif-main">
                    <div className="notif-title-row">
                      <span className="notif-dot" />
                      <h3 className="notif-title">{n.titulo}</h3>
                    </div>

                    <p className="notif-meta">
                      Tipo: <strong>{mapTipo(n.tipo)}</strong>{" "}
                      <span className="notif-separator">·</span> Audiencia:{" "}
                      <strong>{mapAudiencia(n.audiencia)}</strong>
                    </p>

                    {n.cuerpo && (
                      <p className="notif-body-preview">
                        {truncate(n.cuerpo, 120)}
                      </p>
                    )}
                  </div>

                  <div className="notif-side">
                    <span className={`notif-badge estado-${estado.code}`}>
                      {estado.label}
                    </span>
                    <span className="notif-date">{formatDMYHM(when)}</span>
                  </div>
                </article>
              );
            })}
          </div>
        )}
      </div>

      {/* Popup detalle */}
      {selected && (
        <div
          className="notifs-modal-overlay"
          onClick={() => setSelected(null)}
        >
          <div
            className="notifs-modal"
            onClick={(e) => e.stopPropagation()}
          >
            <header className="notifs-modal-header">
              <h3>{selected.titulo}</h3>
              <button
                className="notifs-modal-close"
                onClick={() => setSelected(null)}
              >
                ✕
              </button>
            </header>

            <p className="notifs-modal-subtitle">
              Tipo: <strong>{mapTipo(selected.tipo)}</strong> · Audiencia:{" "}
              <strong>{mapAudiencia(selected.audiencia)}</strong>
            </p>

            {selected.cuerpo && (
              <p className="notifs-modal-body">{selected.cuerpo}</p>
            )}

            <div className="notifs-modal-row">
              <span>
                Estado:{" "}
                <strong>
                  {mapEstado(selected.estado ?? selected.Estado).label}
                </strong>
              </span>
            </div>

            <div className="notifs-modal-footer">
              <button
                className="notifs-modal-btn"
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

// helpers

function mapTipo(t) {
  if (!t) return "—";
  const s = String(t).toLowerCase();
  if (s.includes("beneficio")) return "Beneficio";
  if (s.includes("comunicado")) return "Comunicado";
  if (s.includes("alerta")) return "Alerta";
  if (s.includes("recordatorio")) return "Recordatorio";
  if (s.includes("general")) return "General";
  return String(t);
}

function mapAudiencia(a) {
  if (!a) return "—";
  const s = String(a).toLowerCase();
  if (s.includes("usuario")) return "Usuario";
  if (s.includes("todos")) return "Todos";
  if (s.includes("segmento") || s.includes("rol")) return "Segmento";
  return String(a);
}

function mapEstado(e) {
  if (!e) return { code: "otro", label: "—" };
  const s = String(e).toLowerCase();
  if (s.includes("programada"))
    return { code: "programada", label: "Programada" };
  if (s.includes("publicada"))
    return { code: "publicada", label: "Publicada" };
  if (s.includes("cancelada"))
    return { code: "cancelada", label: "Cancelada" };
  if (s.includes("borrador"))
    return { code: "borrador", label: "Borrador" };
  return { code: "otro", label: String(e) };
}

function formatDMYHM(dateStr) {
  if (!dateStr) return "—";
  const d = new Date(dateStr);
  if (isNaN(d)) return dateStr;
  const dd = String(d.getDate()).padStart(2, "0");
  const mm = String(d.getMonth() + 1).padStart(2, "0");
  const yyyy = d.getFullYear();
  const hh = String(d.getHours()).padStart(2, "0");
  const min = String(d.getMinutes()).padStart(2, "0");
  return `${dd}/${mm}/${yyyy} ${hh}:${min}`;
}

function truncate(str, max) {
  if (!str) return "";
  if (str.length <= max) return str;
  return str.slice(0, max - 1) + "…";
}
