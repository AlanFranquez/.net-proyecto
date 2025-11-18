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

  // ------------------ LOAD USER BENEFITS ------------------
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

        // 1) obtener beneficios (backend real)
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

        // Se espera estructura:
        // [{
        //    id, nombre, estado, vigenciaInicio, vigenciaFin,
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

      // actualizar pantalla
      setBeneficios((prev) =>
        prev.map((x) =>
          x.id === b.id
            ? { ...x, estado: "obtenido", cuposDisponibles: x.cuposDisponibles - 1 }
            : x
        )
      );

      alert("Beneficio solicitado correctamente");
      setModal(null);
    } catch (err) {
      console.error(err);
      alert(err.message);
    }
  }

  // ------------------ UI ------------------
  if (authLoading || loading) {
    return (
      <>
        <Navbar isLoggedIn={loggedIn ?? isLoggedIn} onToggle={onToggle} />
        <div className="beneficios-container">
          <h2>Mis Beneficios</h2>
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
          <h2>Mis Beneficios</h2>
          <p>Debes iniciar sesión para ver tus beneficios.</p>
        </div>
      </>
    );
  }

  return (
    <>
      <Navbar isLoggedIn={true} onToggle={onToggle} />

      <div className="beneficios-container">
        <h2>Mis Beneficios</h2>

        {error && <p className="beneficios-error">{error}</p>}

        <div className="beneficios-list">
          {beneficios.map((b) => (
            <div key={b.id} className="beneficio-item">
              <span>{b.nombre}</span>

              {b.estado === "obtenido" ? (
                <button className="btn-disabled">Obtenido</button>
              ) : (
                <button className="btn-obtener" onClick={() => setModal(b)}>
                  Obtener
                </button>
              )}
            </div>
          ))}

          {beneficios.length === 0 && (
            <p className="beneficios-empty">
              No tienes beneficios disponibles por ahora.
            </p>
          )}
        </div>
      </div>

      {/* ------------------ MODAL ------------------ */}
      {modal && (
        <div className="modal-overlay" onClick={() => setModal(null)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <h3>Petición de Beneficio</h3>

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
              <strong>Estado:</strong>{" "}
              {modal.estado === "disponible" ? "Disponible" : "Obtenido"}
            </p>

            <div className="modal-actions">
              <button className="btn-cancelar" onClick={() => setModal(null)}>
                Cancelar
              </button>

              <button
                className="btn-solicitar"
                onClick={() => solicitarBeneficio(modal)}
              >
                Solicitar
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
