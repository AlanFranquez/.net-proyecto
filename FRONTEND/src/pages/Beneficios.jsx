import React, { useEffect, useState } from "react";
import Navbar from "../components/Navbar";
import { useAuth, toApi } from "../services/AuthService.jsx";
import "../styles/Beneficios.css";

export default function Beneficios({ isLoggedIn, onToggle }) {
  const { user, loading: authLoading } = useAuth();

  const [beneficios, setBeneficios] = useState([]);
  const [loading, setLoading] = useState(true);
  const [modal, setModal] = useState(null);
  const [error, setError] = useState(null);

  const loggedIn = !!user;

  // ------------------ LOAD BENEFICIOS ------------------
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

        const res = await fetch(toApi("/beneficios"), {
          method: "GET",
          credentials: "include",
        });

        if (!res.ok) {
          throw new Error("Error cargando beneficios");
        }

        const data = await res.json();
        if (!Array.isArray(data)) {
          throw new Error("Formato desconocido en beneficios");
        }

        setBeneficios(data);
      } catch (err) {
        console.error(err);
        setError(err.message || "Error al cargar beneficios");
      } finally {
        setLoading(false);
      }
    };

    load();
  }, [authLoading, loggedIn]);

  // ------------------ DERIVED LISTS (replicando lógica de Credencial) ------------------
  // Filtramos vigentes y asignados al usuario, como en Credencial
  const now = new Date();
  const usuarioId = user?.usuarioId ?? user?.id;
  const beneficiosUsuarioIds =
    user?.beneficiosIDs ?? user?.beneficiosIds ?? [];

  // Todos los beneficios vigentes (para cualquiera)
  const beneficiosVigentes = beneficios.filter((b) => {
    const ini =
      b.vigenciaInicio ?? b.desde ?? b.fechaInicio ?? b.inicio ?? null;
    const fin = b.vigenciaFin ?? b.hasta ?? b.fechaFin ?? b.fin ?? null;

    const dIni = ini ? new Date(ini) : null;
    const dFin = fin ? new Date(fin) : null;

    const okIni = !dIni || dIni <= now;
    const okFin = !dFin || dFin >= now;

    return okIni && okFin;
  });

  // Mis beneficios (vigentes y asignados al usuario)
  const misBeneficios = beneficiosVigentes.filter((b) => {
    const usuariosIDs = b.usuariosIDs ?? [];
    const byUserList =
      Array.isArray(usuariosIDs) &&
      usuarioId &&
      usuariosIDs.includes(usuarioId);

    const byIdList =
      Array.isArray(beneficiosUsuarioIds) &&
      beneficiosUsuarioIds.includes(b.id);

    // Además, por si el backend marca explícitamente estado "obtenido"
    const estado = (b.estado || "").toLowerCase();
    const esObtenido =
      estado === "obtenido" || estado === "activo";

    return byUserList || byIdList || esObtenido;
  });

  // Beneficios vigentes que NO son míos
  const disponibles = beneficiosVigentes.filter(
    (b) => !misBeneficios.some((mb) => mb.id === b.id)
  );

  // ------------------ SOLICITAR BENEFICIO ------------------
  async function solicitarBeneficio(b) {
    try {
      const res = await fetch(toApi(`/beneficios/${b.id}/solicitar`), {
        method: "POST",
        credentials: "include",
      });

      if (!res.ok) {
        const txt = await res.text();
        throw new Error(txt || "No se pudo solicitar el beneficio");
      }

      // actualizar pantalla: marcamos el beneficio como obtenido
      setBeneficios((prev) =>
        prev.map((x) =>
          x.id === b.id
            ? {
                ...x,
                estado: "obtenido",
                cuposDisponibles:
                  typeof x.cuposDisponibles === "number"
                    ? x.cuposDisponibles - 1
                    : x.cuposDisponibles,
              }
            : x
        )
      );

      alert("Beneficio obtenido correctamente");
      setModal(null);
    } catch (err) {
      console.error(err);
      alert(err.message);
    }
  }

  // ------------------ UI ESTADOS GENERALES ------------------
  if (authLoading || loading) {
    return (
      <>
        <Navbar isLoggedIn={loggedIn ?? isLoggedIn} onToggle={onToggle} />
        <div className="beneficios-container">
          <h2>Beneficios</h2>
          <p>Cargando beneficios…</p>
        </div>
      </>
    );
  }

  if (!loggedIn) {
    return (
      <>
        <Navbar isLoggedIn={false} onToggle={onToggle} />
        <div className="beneficios-container">
          <h2>Beneficios</h2>
          <p>Debes iniciar sesión para ver tus beneficios.</p>
        </div>
      </>
    );
  }

  const noHayNinguno = beneficiosVigentes.length === 0;

  return (
    <>
      <Navbar isLoggedIn={true} onToggle={onToggle} />

      <div className="beneficios-container">
        <h2>Beneficios</h2>

        {error && <p className="beneficios-error">{error}</p>}

        {noHayNinguno && !error && (
          <p className="beneficios-empty">
            No hay beneficios configurados por ahora.
          </p>
        )}

        {/* ------------------ MIS BENEFICIOS ------------------ */}
        {!noHayNinguno && (
          <>
            <h3 className="beneficios-subtitle">Mis beneficios</h3>
            <div className="beneficios-list">
              {misBeneficios.map((b) => (
                <div key={b.id} className="beneficio-item">
                  <span>{b.nombre}</span>
                  <button className="btn-disabled" disabled>
                    Obtenido
                  </button>
                </div>
              ))}

              {misBeneficios.length === 0 && (
                <p className="beneficios-empty">
                  Aún no has obtenido ningún beneficio.
                </p>
              )}
            </div>

            {/* ------------------ BENEFICIOS DISPONIBLES (todos los demás vigentes) ------------------ */}
            <h3 className="beneficios-subtitle">Beneficios disponibles</h3>
            <div className="beneficios-list">
              {disponibles.map((b) => (
                <div key={b.id} className="beneficio-item">
                  <span>{b.nombre}</span>
                  {/* Aquí podrías abrir el modal para ver detalle y confirmar */}
                  {/* <button onClick={() => setModal(b)}>Ver / Solicitar</button> */}
                </div>
              ))}

              {disponibles.length === 0 && (
                <p className="beneficios-empty">
                  No hay beneficios nuevos disponibles en este momento.
                </p>
              )}
            </div>
          </>
        )}
      </div>

      {/* ------------------ MODAL ------------------ */}
      {modal && (
        <div className="modal-overlay" onClick={() => setModal(null)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <h3>Obtener Beneficio</h3>

            <p>
              <strong>Beneficio:</strong> {modal.nombre}
            </p>

            <p>
              <strong>Vigencia:</strong>{" "}
              {formatDMY(modal.vigenciaInicio)} – {formatDMY(modal.vigenciaFin)}
            </p>

            <p>
              <strong>Cupos:</strong>{" "}
              {modal.cuposDisponibles}/{modal.cuposTotales} disponibles
            </p>

            <p>
              <strong>Costo:</strong> ${modal.costo ?? 0}
            </p>

            <p>
              <strong>Estado actual:</strong>{" "}
              {(modal.estado || "disponible").toString()}
            </p>

            <div className="modal-actions">
              <button className="btn-cancelar" onClick={() => setModal(null)}>
                Cancelar
              </button>

              <button
                className="btn-solicitar"
                onClick={() => solicitarBeneficio(modal)}
              >
                Confirmar
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}

function formatDMY(dateStr) {
  if (!dateStr) return "—";
  const d = new Date(dateStr);
  if (isNaN(d)) return dateStr;
  const dd = String(d.getDate()).padStart(2, "0");
  const mm = String(d.getMonth() + 1).padStart(2, "0");
  const yyyy = d.getFullYear();
  return `${dd}/${mm}/${yyyy}`;
}
