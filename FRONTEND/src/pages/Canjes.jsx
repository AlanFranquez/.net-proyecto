import React, { useEffect, useMemo, useState } from "react";
import Navbar from "../components/Navbar";
import { useAuth, toApi } from "../services/AuthService.jsx";
import "../styles/Canjes.css";

export default function Canjes({ isLoggedIn = true, onToggle }) {
  const { user, loading: authLoading } = useAuth();

  const [search, setSearch] = useState("");
  const [desde, setDesde] = useState("");
  const [hasta, setHasta] = useState("");
  const [estado, setEstado] = useState("all"); // all | pendiente | completado | cancelado
  const [detalle, setDetalle] = useState(null);

  const [canjes, setCanjes] = useState([]);
  const [beneficios, setBeneficios] = useState({});
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const loggedIn = !!user;

  // --------- Cargar canjes del usuario actual ---------
  useEffect(() => {
    setCanjes([]);
    setBeneficios({});
    setError(null);
    setLoading(true);

    if (authLoading) return;

    if (!user) {
      setLoading(false);
      return;
    }

    const fetchData = async () => {
      try {
        const uId = user.usuarioId ?? user.id;
        if (!uId) {
          throw new Error("Usuario sin identificador válido para canjes.");
        }

        // 1) Beneficios (para mostrar nombre)
        const benRes = await fetch(toApi("/beneficios"), {
          method: "GET",
          credentials: "include",
          headers: { Accept: "application/json" },
        });

        if (!benRes.ok) {
          const txt = await benRes.text();
          throw new Error(
            txt || `Error al cargar beneficios: ${benRes.status}`
          );
        }

        const benData = await benRes.json();
        let benMap = {};
        if (Array.isArray(benData)) {
          benData.forEach((b) => {
            const id = b.id ?? b.Id ?? b.BeneficioId ?? b.beneficioId;
            if (!id) return;
            benMap[String(id)] = {
              id,
              nombre: b.nombre ?? b.Nombre ?? `Beneficio ${shortGuid(id)}`,
            };
          });
        }
        setBeneficios(benMap);

        // 2) Canjes del usuario
        const url = toApi(`/canjes?usuarioId=${encodeURIComponent(uId)}`);
        const cjRes = await fetch(url, {
          method: "GET",
          credentials: "include",
          headers: { Accept: "application/json" },
        });

        if (!cjRes.ok) {
          const txt = await cjRes.text();
          throw new Error(txt || `Error al cargar canjes: ${cjRes.status}`);
        }

        const cjData = await cjRes.json();
        if (!Array.isArray(cjData)) {
          throw new Error("Formato inesperado en /canjes");
        }

        setCanjes(cjData);
      } catch (err) {
        console.error("Error cargando canjes:", err);
        setError(err.message || "Error al cargar historial de canjes");
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, [authLoading, user?.usuarioId, user?.id]);

  // --------- Normalización de canjes a modelo de UI ---------
  const canjesUI = useMemo(() => {
    return canjes.map((c) => {
      const id =
        c.CanjeId ?? c.canjeId ?? c.Id ?? c.id;

      const fechaRaw = c.Fecha ?? c.fecha;
      const d = fechaRaw ? new Date(fechaRaw) : null;
      const fechaISO = d ? formatDateInput(d) : "";
      const fechaDisplay = d ? formatDMY(d) : "Fecha desconocida";

      const estadoRaw = (c.Estado ?? c.estado ?? "").toString();
      const estadoNorm = estadoRaw.toLowerCase();

      let estadoKey = "otro"; // para filtros
      if (estadoNorm.includes("pend")) estadoKey = "pendiente";
      else if (estadoNorm.includes("confirm")) estadoKey = "completado";
      else if (estadoNorm.includes("anul") || estadoNorm.includes("cancel"))
        estadoKey = "cancelado";

      const estadoLabel = title(estadoKey, estadoRaw);

      const benId = c.BeneficioId ?? c.beneficioId;
      const benInfo = benId
        ? beneficios[String(benId)]
        : null;
      const beneficioNombre =
        benInfo?.nombre ??
        (benId ? `Beneficio ${shortGuid(benId)}` : "—");

      const biometria =
        typeof c.VerificacionBiometrica === "boolean"
          ? c.VerificacionBiometrica
          : null;

      const firma = c.Firma ?? c.firma ?? "—";

      const usuarioId = c.UsuarioId ?? c.usuarioId ?? user?.usuarioId ?? user?.id;

      return {
        raw: c,
        id,
        fechaISO,
        fechaDisplay,
        estadoKey,
        estadoLabel,
        beneficioNombre,
        biometria,
        firma,
        usuarioId,
      };
    });
  }, [canjes, beneficios, user?.usuarioId, user?.id]);

  // --------- Filtros (estado, texto, rango de fechas) ---------
  const items = useMemo(() => {
    return canjesUI.filter((r) => {
      // filtro de estado
      if (estado !== "all" && r.estadoKey !== estado) return false;

      // filtro de texto libre
      if (search.trim()) {
        const q = search.trim().toLowerCase();
        const hay =
          (r.id && String(r.id).toLowerCase().includes(q)) ||
          r.beneficioNombre.toLowerCase().includes(q) ||
          r.estadoLabel.toLowerCase().includes(q) ||
          (r.firma && r.firma.toLowerCase().includes(q));
        if (!hay) return false;
      }

      // rango de fechas (YYYY-MM-DD inclusivo)
      if (desde && r.fechaISO && r.fechaISO < desde) return false;
      if (hasta && r.fechaISO && r.fechaISO > hasta) return false;

      return true;
    });
  }, [canjesUI, estado, search, desde, hasta]);

  // --------- Vistas globales ---------
  if (authLoading || loading) {
    return (
      <>
        <Navbar isLoggedIn={loggedIn ?? isLoggedIn} onToggle={onToggle} />
        <main className="canjes-wrap">
          <section className="canjes-card">
            <h1 className="canjes-title">Historial de Canjes</h1>
            <p className="canjes-loading">Cargando canjes…</p>
          </section>
        </main>
      </>
    );
  }

  if (!loggedIn) {
    return (
      <>
        <Navbar isLoggedIn={false} onToggle={onToggle} />
        <main className="canjes-wrap">
          <section className="canjes-card">
            <h1 className="canjes-title">Historial de Canjes</h1>
            <p className="canjes-empty">
              Debes iniciar sesión para ver tu historial de canjes.
            </p>
          </section>
        </main>
      </>
    );
  }

  return (
    <>
      <Navbar isLoggedIn={true} onToggle={onToggle} />

      <main className="canjes-wrap">
        <section className="canjes-card" aria-labelledby="cj-title">
          <h1 id="cj-title" className="canjes-title">
            Historial de Canjes
          </h1>

          {error && <p className="canjes-error">{error}</p>}

          {/* Filtros */}
          <div className="filters" role="region" aria-label="Filtros">
            <div className="search">
              <span className="search-icon" aria-hidden>
                
              </span>
              <input
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                aria-label="Buscar"
              />
            </div>

            <div className="date-range">
              <label className="sr-only" htmlFor="desde">
                Desde
              </label>
              <input
                id="desde"
                type="date"
                value={desde}
                onChange={(e) => setDesde(e.target.value)}
              />
              <span className="dash">—</span>
              <label className="sr-only" htmlFor="hasta">
                Hasta
              </label>
              <input
                id="hasta"
                type="date"
                value={hasta}
                onChange={(e) => setHasta(e.target.value)}
              />
            </div>

            <div className="status" role="group" aria-label="Estado">
              <button
                className={`chip ${
                  estado === "pendiente" ? "on pending" : ""
                }`}
                onClick={() =>
                  setEstado(estado === "pendiente" ? "all" : "pendiente")
                }
              >
                Pendiente
              </button>
              <button
                className={`chip ${
                  estado === "completado" ? "on done" : ""
                }`}
                onClick={() =>
                  setEstado(estado === "completado" ? "all" : "completado")
                }
              >
                Completado
              </button>
              <button
                className={`chip ${
                  estado === "cancelado" ? "on cancel" : ""
                }`}
                onClick={() =>
                  setEstado(estado === "cancelado" ? "all" : "cancelado")
                }
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
              <div role="columnheader">Beneficio</div>
              <div role="columnheader">Biometría</div>
              <div role="columnheader">Firma</div>
              <div role="columnheader"></div>
            </div>

            <div className="tbody">
              {items.map((r) => (
                <div key={r.id} className="row" role="row">
                  <div className="col-main" role="cell">
                    <div className="bold">Canje #{r.id}</div>
                  </div>

                  <div className="col" data-label="Fecha" role="cell">
                    {r.fechaDisplay}
                  </div>

                  <div className="col" data-label="Estado" role="cell">
                    <span className={`badge ${r.estadoKey}`}>
                      {r.estadoLabel}
                    </span>
                  </div>

                  <div className="col" data-label="Beneficio" role="cell">
                    {r.beneficioNombre}
                  </div>

                  <div className="col" data-label="Biometría" role="cell">
                    {r.biometria === null
                      ? "—"
                      : r.biometria
                      ? "Sí"
                      : "No"}
                  </div>

                  <div className="col" data-label="Firma" role="cell">
                    {r.firma}
                  </div>

                  <div className="col action" role="cell">
                    <button className="link" onClick={() => setDetalle(r)}>
                      Ver Detalle
                    </button>
                  </div>
                </div>
              ))}

              {items.length === 0 && (
                <div className="empty" role="note">
                  Sin resultados con los filtros actuales.
                </div>
              )}
            </div>

            {/* Paginación fake */}
            <div className="pager" aria-live="polite">
              Página 1 de 1
            </div>
          </div>
        </section>
      </main>

      {/* Modal de detalle */}
      {detalle && (
        <div
          className="cj-modal-overlay"
          onClick={() => setDetalle(null)}
          role="dialog"
          aria-modal="true"
          aria-labelledby="cj-modal-title"
        >
          <div
            className="cj-modal"
            onClick={(e) => e.stopPropagation()}
          >
            <h3 id="cj-modal-title">Detalle de Canje</h3>
            <div className="cj-grid">
              <span className="k">ID</span>
              <span>{detalle.id}</span>

              <span className="k">Fecha</span>
              <span>{detalle.fechaDisplay}</span>

              <span className="k">Estado</span>
              <span>{detalle.estadoLabel}</span>

              <span className="k">Beneficio</span>
              <span>{detalle.beneficioNombre}</span>

              <span className="k">Verificación biométrica</span>
              <span>
                {detalle.biometria === null
                  ? "—"
                  : detalle.biometria
                  ? "Sí"
                  : "No"}
              </span>

              <span className="k">Firma</span>
              <span>{detalle.firma}</span>
            </div>

            <div className="cj-actions">
              <button
                className="btn-close"
                onClick={() => setDetalle(null)}
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

// ---------- helpers ----------

function title(estadoKey, rawFromApi) {
  if (estadoKey === "pendiente") return "Pendiente";
  if (estadoKey === "completado") return "Completado";
  if (estadoKey === "cancelado") return "Cancelado";
  // fallback: mostrar lo que viene del enum (Confirmado / Anulado / etc.)
  return rawFromApi || "Desconocido";
}

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

function shortGuid(g) {
  if (!g) return "";
  const s = String(g);
  // ej: "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" -> tomamos últimos 4
  const parts = s.split("-");
  return parts[parts.length - 1]?.slice(0, 4) ?? s;
}
