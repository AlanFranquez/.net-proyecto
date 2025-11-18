import React, { useEffect, useMemo, useState } from "react";
import Navbar from "../components/Navbar";
import { useAuth, toApi } from "../services/AuthService.jsx";
import "../styles/HistorialDeAccesos.css";

export default function HistorialDeAccesos({ isLoggedIn = true, onToggle }) {
  const { user, loading: authLoading } = useAuth();

  const [q, setQ] = useState("");
  const [fecha, setFecha] = useState("");
  const [espacioFiltro, setEspacioFiltro] = useState("");
  const [info, setInfo] = useState(null); // evento seleccionado

  const [eventos, setEventos] = useState([]);
  const [loadingEventos, setLoadingEventos] = useState(true);
  const [errorEventos, setErrorEventos] = useState(null);

  const loggedIn = !!user;

  // --------- Cargar eventos del usuario actual (vía credenciales) ----------
  useEffect(() => {
    setEventos([]);
    setErrorEventos(null);
    setLoadingEventos(true);

    if (authLoading) {
      return;
    }

    if (!user) {
      setLoadingEventos(false);
      return;
    }

    const fetchForUser = async () => {
      try {
        // 1) Obtener credenciales del usuario
        const credRes = await fetch(toApi("/credenciales"), {
          method: "GET",
          credentials: "include",
          headers: { Accept: "application/json" },
        });

        if (!credRes.ok) {
          const txt = await credRes.text();
          throw new Error(
            txt || `Error al cargar credenciales: ${credRes.status}`
          );
        }

        const credData = await credRes.json();
        const uId = user.usuarioId ?? user.id;

        let credencialesUsuario = [];

        if (Array.isArray(credData)) {
          credencialesUsuario = credData.filter((c) => {
            // TODO: ajusta estos nombres según tu DTO de credencial
            const cUserId =
              c.UsuarioId ??
              c.usuarioId ??
              c.userId ??
              c.Usuario?.UsuarioId ??
              c.usuario?.usuarioId ??
              c.Usuario?.Id ??
              c.usuario?.id;

            const cEmail =
              c.UsuarioEmail ??
              c.usuarioEmail ??
              c.Email ??
              c.email ??
              c.Usuario?.Email ??
              c.usuario?.email;

            const matchesId =
              uId && cUserId && String(cUserId) === String(uId);

            const matchesEmail =
              user.email &&
              cEmail &&
              cEmail.toLowerCase() === user.email.toLowerCase();

            return matchesId || matchesEmail;
          });
        }

        const credIds = credencialesUsuario.map(
          (c) =>
            c.CredencialId ??
            c.credencialId ??
            c.Id ??
            c.id ??
            c.Codigo ??
            c.codigo
        );

        // Si no tiene credenciales, no habrá eventos
        if (!credIds.length) {
          setEventos([]);
          setLoadingEventos(false);
          return;
        }

        // 2) Obtener todos los eventos
        const evRes = await fetch(toApi("/eventos"), {
          method: "GET",
          credentials: "include",
          headers: { Accept: "application/json" },
        });

        if (!evRes.ok) {
          const txt = await evRes.text();
          throw new Error(
            txt || `Error al cargar eventos: ${evRes.status}`
          );
        }

        const evData = await evRes.json();
        if (!Array.isArray(evData)) {
          setEventos([]);
          setLoadingEventos(false);
          return;
        }

        // 3) Filtrar eventos cuyas CredencialId estén dentro de las del usuario
        const credIdSet = new Set(
          credIds
            .filter(Boolean)
            .map((x) => String(x).toLowerCase())
        );

        const filtrados = evData.filter((e) => {
          // Nombres según tu entity EventoAcceso
          const eCredId =
            e.CredencialId ??
            e.credencialId ??
            e.Credencial?.CredencialId ??
            e.Credencial?.Id;

          if (!eCredId) return false;

          return credIdSet.has(String(eCredId).toLowerCase());
        });

        // Ordenamos por MomentoDeAcceso descendente
        const ordenados = filtrados
          .slice()
          .sort((a, b) => {
            const da = new Date(
              a.MomentoDeAcceso ?? a.momentoDeAcceso ?? 0
            ).getTime();
            const db = new Date(
              b.MomentoDeAcceso ?? b.momentoDeAcceso ?? 0
            ).getTime();
            return db - da;
          });

        setEventos(ordenados);
      } catch (err) {
        console.error("Error cargando historial de accesos:", err);
        setErrorEventos(
          err.message || "Error al cargar historial de accesos"
        );
        setEventos([]);
      } finally {
        setLoadingEventos(false);
      }
    };

    fetchForUser();
  }, [authLoading, user?.usuarioId, user?.email]);

  // --------- Normalización para la UI ----------
  function mapEventoToUI(e) {
    const dtRaw = e.MomentoDeAcceso ?? e.momentoDeAcceso;
    const d = dtRaw ? new Date(dtRaw) : null;

    // Para filtros y display
    const fechaStr = d ? formatDateInput(d) : "";
    const fechaDisplay = d ? formatDMY(d) : "Fecha desconocida";
    const horaDisplay = d ? formatHM(d) : "--:--";

    const espacioNombre =
      e.Espacio?.Nombre ??
      e.Espacio?.nombre ??
      e.espacioNombre ??
      "Acceso";

    // Resultado enum → estado UI
    const resultadoRaw =
      (e.Resultado ?? e.resultado ?? "").toString().toLowerCase();
    let estado = "otro";
    if (resultadoRaw.includes("permit")) estado = "permitido";
    else if (resultadoRaw.includes("deneg")) estado = "denegado";

    const modo =
      e.Modo ??
      e.modo ??
      "Online";

    const motivo =
      e.Motivo ??
      e.motivo ??
      "-";

    const id =
      e.EventoId ??
      e.eventoId ??
      e.Id ??
      e.id ??
      `${dtRaw}-${espacioNombre}-${modo}`;

    return {
      id,
      fecha: fechaStr, // para filtro YYYY-MM-DD
      fechaDisplay,
      horaDisplay,
      espacio: espacioNombre,
      estado,
      modo,
      motivo,
      raw: e,
    };
  }

  const eventosUI = useMemo(
    () => eventos.map(mapEventoToUI),
    [eventos]
  );

  // --------- Filtros (texto, fecha, espacio) ----------
  const itemsFiltrados = useMemo(() => {
    return eventosUI.filter((r) => {
      if (q.trim()) {
        const s = q.trim().toLowerCase();
        const hay =
          r.espacio.toLowerCase().includes(s) ||
          r.estado.toLowerCase().includes(s) ||
          (r.fechaDisplay + " " + r.horaDisplay)
            .toLowerCase()
            .includes(s) ||
          r.modo?.toLowerCase().includes(s) ||
          r.motivo?.toLowerCase().includes(s);
        if (!hay) return false;
      }
      if (fecha && r.fecha !== fecha) return false;
      if (espacioFiltro && r.espacio !== espacioFiltro) return false;
      return true;
    });
  }, [q, fecha, espacioFiltro, eventosUI]);

  // espacios únicos para el filtro
  const espacios = useMemo(
    () =>
      Array.from(new Set(eventosUI.map((r) => r.espacio))).sort(
        (a, b) => a.localeCompare(b)
      ),
    [eventosUI]
  );

  const handleClickItem = (ui) => {
    setInfo(ui);
  };

  // --------- Vistas globales de estado ----------
  if (authLoading || loadingEventos) {
    return (
      <>
        <Navbar isLoggedIn={loggedIn ?? isLoggedIn} onToggle={onToggle} />
        <main className="ha-wrap">
          <section className="ha-card">
            <h1 className="ha-title">Historial de Accesos</h1>
            <p className="ha-loading">Cargando historial...</p>
          </section>
        </main>
      </>
    );
  }

  if (!loggedIn) {
    return (
      <>
        <Navbar isLoggedIn={false} onToggle={onToggle} />
        <main className="ha-wrap">
          <section className="ha-card ha-card--center">
            <h1 className="ha-title">Historial de Accesos</h1>
            <p className="ha-empty">
              Debes iniciar sesión para ver el historial de accesos.
            </p>
          </section>
        </main>
      </>
    );
  }

  return (
    <>
      <Navbar isLoggedIn={true} onToggle={onToggle} />

      <main className="ha-wrap">
        <section className="ha-card">
          <h1 className="ha-title">Historial de Accesos</h1>

          {errorEventos && (
            <p className="ha-error">
              Error al cargar el historial: {errorEventos}
            </p>
          )}

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
                value={espacioFiltro}
                onChange={(e) => setEspacioFiltro(e.target.value)}
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
            {itemsFiltrados.map((r) => (
              <li
                key={r.id}
                className="ha-item"
                onClick={() => handleClickItem(r)}
              >
                <div className="ha-main">
                  <div className="ha-when">
                    {r.fechaDisplay} {r.horaDisplay}
                  </div>
                  <div className="ha-space">{r.espacio}</div>
                </div>
                <span className={`ha-chip ${r.estado}`}>
                  {r.estado === "permitido"
                    ? "✔ Permitido"
                    : r.estado === "denegado"
                    ? "✖ Denegado"
                    : "• Otro"}
                </span>
              </li>
            ))}
            {itemsFiltrados.length === 0 && (
              <li className="ha-empty">Sin resultados con los filtros.</li>
            )}
          </ul>
        </section>
      </main>

      {/* Modal de detalle */}
      {info && (
        <div
          className="ha-modal-overlay"
          onClick={() => setInfo(null)}
        >
          <div
            className="ha-modal"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="ha-modal-head">
              <h3>Información del acceso</h3>
              <button
                className="close"
                onClick={() => setInfo(null)}
              >
                ✕
              </button>
            </div>

            <div className="ha-grid">
              <span className="k">Fecha</span>
              <span>
                {info.fechaDisplay} {info.horaDisplay}
              </span>

              <span className="k">Estado</span>
              <span>
                {info.estado === "permitido"
                  ? "Permitido"
                  : info.estado === "denegado"
                  ? "Denegado"
                  : info.estado}
              </span>

              <span className="k">Modo</span>
              <span>{info.modo}</span>

              <span className="k">Motivo</span>
              <span>{info.motivo || "-"}</span>

              <span className="k">Espacio</span>
              <span>{info.espacio}</span>

              <span className="k">ID Evento</span>
              <span>{info.id}</span>

              {/* Si quisieras mostrar la credencial, firma, etc.: */}
              <span className="k">Firma</span>
              <span>
                {info.raw?.Firma ??
                  info.raw?.firma ??
                  "—"}
              </span>
            </div>

            <button
              className="ha-btn"
              onClick={() => setInfo(null)}
            >
              Cerrar
            </button>
          </div>
        </div>
      )}
    </>
  );
}

// --------- Helpers de fecha/hora ----------
function formatDateInput(d) {
  const yyyy = d.getFullYear();
  const mm = String(d.getMonth() + 1).padStart(2, "0");
  const dd = String(d.getDate()).padStart(2, "0");
  return `${yyyy}-${mm}-${dd}`;
}

function formatDMY(d) {
  const dd = String(d.getDate()).padStart(2, "0");
  const mm = String(d.getMonth() + 1).padStart(2, "0");
  const yyyy = d.getFullYear();
  return `${dd}/${mm}/${yyyy}`;
}

function formatHM(d) {
  const hh = String(d.getHours()).padStart(2, "0");
  const mm = String(d.getMinutes()).padStart(2, "0");
  return `${hh}:${mm}`;
}
