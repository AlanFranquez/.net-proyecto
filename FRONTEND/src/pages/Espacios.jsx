// src/pages/Espacios.jsx
import React, { useEffect, useMemo, useState } from "react";
import Navbar from "../components/Navbar";
import { useAuth } from "../services/AuthService.jsx";
import { fetchEspacios, fetchReglas } from "../services/EspaciosService.jsx";
import "../styles/Espacios.css";

const pick = (obj, keys, fallback = undefined) => {
  for (const k of keys) if (obj && obj[k] != null) return obj[k];
  return fallback;
};

const toBool = (v) => v === true || v === "true" || v === 1 || v === "1";

function formatDate(d) {
  if (!d) return "—";
  const date = new Date(d);
  if (Number.isNaN(date.getTime())) return String(d);
  return date.toLocaleString();
}

export default function Espacios({ isLoggedIn, onToggle }) {
  const { user, loading: authLoading } = useAuth();
  const loggedIn = !!user;

  const [espacios, setEspacios] = useState([]);
  const [reglas, setReglas] = useState([]);
  const [selectedId, setSelectedId] = useState(null);

  const [q, setQ] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    let cancelled = false;

    async function load() {
      setLoading(true);
      setError("");
      try {
        const [esp, reg] = await Promise.all([fetchEspacios(), fetchReglas()]);
        if (cancelled) return;

        setEspacios(esp);
        setReglas(reg);

        const firstId = pick(esp[0], ["id", "Id"], null);
        setSelectedId(firstId);
      } catch (e) {
        if (!cancelled) setError(e.message || "Error cargando datos.");
      } finally {
        if (!cancelled) setLoading(false);
      }
    }

    if (!authLoading) load();

    return () => {
      cancelled = true;
    };
  }, [authLoading]);

  const espaciosFiltrados = useMemo(() => {
    const term = q.trim().toLowerCase();
    if (!term) return espacios;
    return espacios.filter((e) =>
      String(pick(e, ["nombre", "Nombre"], ""))
        .toLowerCase()
        .includes(term)
    );
  }, [espacios, q]);

  const espacioSeleccionado = useMemo(
    () =>
      espacios.find((e) => pick(e, ["id", "Id"]) === selectedId) || null,
    [espacios, selectedId]
  );

  const reglasDelEspacio = useMemo(() => {
    if (!espacioSeleccionado) return [];
    const espId = pick(espacioSeleccionado, ["id", "Id"]);
    if (!espId) return [];

    return reglas.filter((r) => {
      const ids = pick(r, ["espaciosIDs", "EspaciosIDs"], []);
      return Array.isArray(ids) && ids.includes(espId);
    });
  }, [reglas, espacioSeleccionado]);

  // --------- LOADING ----------
  if (authLoading || loading) {
    return (
      <>
        <Navbar isLoggedIn={loggedIn ?? isLoggedIn} onToggle={onToggle} />
        <div className="esp-wrap">
          <h1 className="esp-title">Espacios</h1>
          <div className="esp-card">Cargando espacios y reglas…</div>
        </div>
      </>
    );
  }

  // --------- ERROR ----------
  if (error) {
    return (
      <>
        <Navbar isLoggedIn={loggedIn ?? isLoggedIn} onToggle={onToggle} />
        <div className="esp-wrap">
          <h1 className="esp-title">Espacios</h1>
          <div className="esp-card esp-error">{error}</div>
        </div>
      </>
    );
  }

  // --------- MAIN ----------
  return (
    <>
      <Navbar isLoggedIn={true} onToggle={onToggle} />

      <div className="esp-wrap">
        <h1 className="esp-title">Espacios y reglas de acceso</h1>

        <div className="esp-toolbar esp-card">
          <label className="esp-search">
            Buscar espacio
            <input
              value={q}
              onChange={(e) => setQ(e.target.value)}
            />
          </label>
          <div className="esp-count">
            {espaciosFiltrados.length} / {espacios.length}
          </div>
        </div>

        <div className="esp-grid">
          {/* Lista de espacios */}
          <section className="esp-list">
            {espaciosFiltrados.length === 0 && (
              <div className="esp-card">No hay espacios para mostrar.</div>
            )}

            {espaciosFiltrados.map((e) => {
              const id = pick(e, ["id", "Id"]);
              const nombre = pick(e, ["nombre", "Nombre"], "Sin nombre");
              const tipo = pick(e, ["tipo", "Tipo"], "—");
              const modo = pick(e, ["modo", "Modo"], "—");
              const activo = toBool(pick(e, ["activo", "Activo"], false));
              const reglasCount = pick(e, ["reglasCount", "ReglasCount"], 0);

              const isActive = id === selectedId;

              return (
                <button
                  key={id}
                  className={`esp-item esp-card ${isActive ? "is-active" : ""}`}
                  onClick={() => setSelectedId(id)}
                >
                  <div className="esp-item-head">
                    <div className="esp-item-title">{nombre}</div>
                    <span className={`esp-badge ${activo ? "ok" : "off"}`}>
                      {activo ? "Activo" : "Inactivo"}
                    </span>
                  </div>

                  <div className="esp-item-meta">
                    <span>
                      <b>Tipo:</b> {tipo}
                    </span>
                    <span>
                      <b>Modo:</b> {modo}
                    </span>
                  </div>

                  <div className="esp-item-foot">
                    <span className="esp-mini">Reglas: {reglasCount}</span>
                  </div>
                </button>
              );
            })}
          </section>

          {/* Detalle + reglas */}
          <section className="esp-detail">
            {!espacioSeleccionado ? (
              <div className="esp-card">Seleccioná un espacio.</div>
            ) : (
              <div className="esp-card">
                <header className="esp-detail-head">
                  <h2 className="esp-detail-title">
                    {pick(espacioSeleccionado, ["nombre", "Nombre"], "Espacio")}
                  </h2>

                  <div className="esp-detail-badges">
                    <span className="esp-badge">
                      {pick(espacioSeleccionado, ["tipo", "Tipo"], "—")}
                    </span>
                    <span className="esp-badge">
                      Modo: {pick(espacioSeleccionado, ["modo", "Modo"], "—")}
                    </span>
                  </div>
                </header>

                <div className="esp-detail-meta">
                  <div>
                  </div>
                  <div>
                    <b>Estado:</b>{" "}
                    {toBool(
                      pick(espacioSeleccionado, ["activo", "Activo"], false)
                    )
                      ? "Activo"
                      : "Inactivo"}
                  </div>
                </div>

                <hr className="esp-sep" />

                <h3 className="esp-subtitle">Reglas de acceso</h3>

                {reglasDelEspacio.length === 0 ? (
                  <div className="esp-empty">
                    Este espacio no tiene reglas asociadas.
                  </div>
                ) : (
                  <ul className="esp-rules">
                    {reglasDelEspacio
                      .sort(
                        (a, b) =>
                          (pick(a, ["prioridad", "Prioridad"], 0) || 0) -
                          (pick(b, ["prioridad", "Prioridad"], 0) || 0)
                      )
                      .map((r) => {
                        const reglaId = pick(r, ["reglaId", "ReglaId"]);
                        const ventana = pick(
                          r,
                          ["ventanaHoraria", "VentanaHoraria"],
                          "—"
                        );
                        const inicio = pick(
                          r,
                          ["vigenciaInicio", "VigenciaInicio"],
                          null
                        );
                        const fin = pick(
                          r,
                          ["vigenciaFin", "VigenciaFin"],
                          null
                        );
                        const prioridad = pick(
                          r,
                          ["prioridad", "Prioridad"],
                          "—"
                        );
                        const politica = pick(
                          r,
                          ["politica", "Politica"],
                          "—"
                        );
                        const rol = pick(r, ["rol", "Rol"], "—");
                        const bio = toBool(
                          pick(
                            r,
                            [
                              "requiereBiometriaConfirmacion",
                              "RequiereBiometriaConfirmacion",
                            ],
                            false
                          )
                        );

                        return (
                          <li key={reglaId} className="esp-rule esp-card">
                            <div className="esp-rule-head">
                              <span className="esp-mini">
                                Prioridad: {prioridad}
                              </span>
                            </div>

                            <div className="esp-rule-body">
                              <div>
                                <b>Ventana horaria:</b> {ventana}
                              </div>
                              <div>
                                <b>Vigencia:</b> {formatDate(inicio)} →{" "}
                                {formatDate(fin)}
                              </div>
                              <div>
                                <b>Política:</b> {politica}
                              </div>
                              <div>
                                <b>Rol requerido:</b> {rol}
                              </div>
                              <div>
                                <b>Biometría:</b>{" "}
                                {bio
                                  ? "Requiere confirmación"
                                  : "No requiere"}
                              </div>
                            </div>
                          </li>
                        );
                      })}
                  </ul>
                )}
              </div>
            )}
          </section>
        </div>
      </div>
    </>
  );
}
