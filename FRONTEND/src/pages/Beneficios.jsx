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

        // Esperamos algo como:
        // [{
        //    id, nombre, estado (disponible/obtenido), vigenciaInicio, vigenciaFin,
        //    cuposTotales, cuposDisponibles, costo
        // }]
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

  // Separamos en 2 listas:
  const misBeneficios = beneficios.filter(
    (b) => (b.estado || "").toLowerCase() === "obtenido"
  );
  const disponibles = beneficios.filter(
    (b) => (b.estado || "").toLowerCase() !== "obtenido"
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

  const noHayNinguno = beneficios.length === 0;

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

            {/* ------------------ BENEFICIOS DISPONIBLES ------------------ */}
            <h3 className="beneficios-subtitle">Beneficios disponibles</h3>
            <div className="beneficios-list">
              {disponibles.map((b) => (
                <div key={b.id} className="beneficio-item">
                  <span>{b.nombre}</span>

                  <button
                    className="btn-obtener"
                    onClick={() => setModal(b)}
                  >
                    Obtener
                  </button>
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
