// src/pages/Dispositivos.jsx
import React, { useEffect, useMemo, useState } from "react";
import Navbar from "../components/Navbar";
import {
  useAuth,
  toApi,
  getBrowserId,
  BROWSER_ID_KEY,
} from "../services/AuthService.jsx";
import "../styles/Dispositivos.css";
import { UAParser } from "ua-parser-js";

export default function Dispositivos({ isLoggedIn, onToggle }) {
  const { user, loading: authLoading, logout } = useAuth();

  const [devices, setDevices] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  // confirm modal stores the whole device UI object (so we can show huella)
  const [confirming, setConfirming] = useState(null);
  const [bajandoId, setBajandoId] = useState(null);

  // toggle revoked visibility
  const [showRevoked, setShowRevoked] = useState(false);

  const usuarioIdActual = user?.usuarioId ?? user?.UsuarioId ?? user?.id;
  const loggedIn = !!user;

  // ---------- Load devices ----------
  const fetchDevices = async () => {
    setDevices([]);
    setError(null);
    setLoading(true);

    if (authLoading) return;
    if (!user) {
      setLoading(false);
      return;
    }

    try {
      const res = await fetch(toApi("/dispositivos"), {
        method: "GET",
        credentials: "include",
        headers: { Accept: "application/json" },
      });

      if (!res.ok) {
        const txt = await res.text();
        throw new Error(txt || `Error al cargar dispositivos: ${res.status}`);
      }

      const data = await res.json();
      if (!Array.isArray(data)) {
        throw new Error("Formato inesperado en /dispositivos");
      }

      const uId = user.usuarioId ?? user.id;
      const mine = data.filter((d) => {
        const dUserId =
          d.UsuarioId ??
          d.usuarioId ??
          d.Usuario?.UsuarioId ??
          d.Usuario?.Id ??
          d.usuario?.usuarioId ??
          d.usuario?.id;

        return uId && dUserId && String(dUserId) === String(uId);
      });

      setDevices(mine);
    } catch (err) {
      console.error("Error cargando dispositivos:", err);
      setError(err.message || "Error al cargar dispositivos");
    } finally {
      setLoading(false);
    }
  };

  const getBrowserInfo = () => {
    const parser = new UAParser();
    const r = parser.getResult();
    return {
      browserName: r.browser.name || "Web",
      browserVersion: r.browser.version || "",
      osName: r.os.name || "",
      deviceType: r.device.type || "desktop",
      browserId: getBrowserId(),
    };
  };

  useEffect(() => {
    fetchDevices();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [authLoading, user?.usuarioId, user?.id]);

  // ---------- Normalize for UI ----------
  const devicesUI = useMemo(() => {
    const currentBrowserId = getBrowserId();

    const mapped = devices.map((d) => {
      const id = d.DispositivoId ?? d.dispositivoId ?? d.Id ?? d.id;

      const plataformaRaw = (d.Plataforma ?? d.plataforma ?? "").toString();
      const plataforma = plataformaRaw || "Desconocido";

      const navegador = d.NavegadorNombre ?? d.navegadorNombre ?? "";
      const navegadorVersion = d.NavegadorVersion ?? d.navegadorVersion ?? "";

      const estadoRaw = (d.Estado ?? d.estado ?? "")
        .toString()
        .toLowerCase();

      const activo =
        !estadoRaw.includes("baja") &&
        !estadoRaw.includes("inactivo") &&
        !estadoRaw.includes("revocado");

      const biometria =
        typeof d.BiometriaHabilitada === "boolean"
          ? d.BiometriaHabilitada
          : d.biometriaHabilitada;

      const huella = d.HuellaDispositivo ?? d.huellaDispositivo ?? "—";
      const isCurrent = String(huella) === String(currentBrowserId);

      // helpful short huella to show (still based on huella, not DB id)
      const huellaShort =
        huella && huella !== "—"
          ? `${String(huella).slice(0, 6)}…${String(huella).slice(-4)}`
          : "—";

      return {
        raw: d,
        id, // internal only
        plataforma,
        navegador,
        navegadorVersion,
        estadoRaw,
        activo,
        biometria,
        huella,
        huellaShort,
        isCurrent,
      };
    });

    // sort: current first, then active, then revoked
    mapped.sort((a, b) => {
      if (a.isCurrent && !b.isCurrent) return -1;
      if (!a.isCurrent && b.isCurrent) return 1;
      if (a.activo && !b.activo) return -1;
      if (!a.activo && b.activo) return 1;
      return 0;
    });

    return mapped;
  }, [devices]);

  const iconFor = (plataforma) => {
    const p = (plataforma || "").toLowerCase();
    if (
      p.includes("android") ||
      p.includes("ios") ||
      p.includes("phone") ||
      p.includes("mobile")
    )
      return "📱";
    if (
      p.includes("windows") ||
      p.includes("mac") ||
      p.includes("desktop") ||
      p.includes("pc")
    )
      return "💻";
    return "W";
  };

  // Auto-register web device if missing
  useEffect(() => {
    if (!loggedIn || loading) return;

    const { browserId, browserName, browserVersion } = getBrowserInfo();
    const already = devicesUI.some(
      (d) => String(d.huella) === String(browserId)
    );

    if (already) return;
    if (!usuarioIdActual) return;

    (async () => {
      try {
        const payload = {
          usuarioId: usuarioIdActual,
          numeroTelefono: null,
          plataforma: "Web",
          biometriaHabilitada: false,
          estado: "Enrolado",
          huellaDispositivo: browserId,
          navegadorNombre: browserName,
          navegadorVersion: browserVersion,
        };

        const res = await fetch(toApi("/dispositivos"), {
          method: "POST",
          credentials: "include",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify(payload),
        });

        if (!res.ok) {
          const txt = await res.text();
          throw new Error(
            txt || `Error autocr creando web dispositivo: ${res.status}`
          );
        }

        await fetchDevices();
      } catch (e) {
        console.error("Auto-register web device failed:", e);
      }
    })();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [loggedIn, loading, devicesUI.length, usuarioIdActual]);

  const estadoLabel = (estadoRaw) => {
    const s = (estadoRaw || "").toLowerCase();
    if (s.includes("activo") || s.includes("enrolado")) return "Activo";
    if (s.includes("baja")) return "Dado de baja";
    if (s.includes("inactivo")) return "Inactivo";
    if (s.includes("revocado")) return "Revocado";
    return estadoRaw || "Desconocido";
  };

  // ---------- DELETE / revoke ----------
  const darDeBaja = async (deviceIdInternal) => {
    if (!deviceIdInternal) return;
    setBajandoId(deviceIdInternal);

    const currentBrowserId = getBrowserId();
    const deletingCurrentBrowser = devicesUI.some(
      (d) =>
        String(d.id) === String(deviceIdInternal) &&
        String(d.huella) === String(currentBrowserId)
    );

    try {
      const res = await fetch(toApi("/dispositivos"), {
        method: "DELETE",
        credentials: "include",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ dispositivoId: deviceIdInternal }),
      });

      if (!res.ok && res.status !== 204) {
        const txt = await res.text();
        throw new Error(
          txt || `Error al dar de baja el dispositivo: ${res.status}`
        );
      }

      // If you revoked the current browser -> kill local device id and logout
      if (deletingCurrentBrowser) {
        localStorage.removeItem(BROWSER_ID_KEY);
        await logout();
        return;
      }

      // optimistic UI: mark as revoked locally
      setDevices((prev) =>
        prev.map((d) => {
          const dId = d.DispositivoId ?? d.dispositivoId ?? d.Id ?? d.id;
          if (String(dId) !== String(deviceIdInternal)) return d;
          return { ...d, Estado: "Revocado" };
        })
      );

      setConfirming(null);
    } catch (err) {
      console.error("Error al dar de baja dispositivo:", err);
      alert(err.message || "No se pudo dar de baja el dispositivo");
    } finally {
      setBajandoId(null);
    }
  };

  const activeDevices = devicesUI.filter((d) => d.activo);
  const revokedDevices = devicesUI.filter((d) => !d.activo);

  // ---------- Global views ----------
  if (authLoading || loading) {
    return (
      <>
        <Navbar isLoggedIn={loggedIn ?? isLoggedIn} onToggle={onToggle} />
        <main className="disp-wrap">
          <section className="disp-card">
            <h1 className="disp-title">Dispositivos conectados</h1>
            <p className="disp-loading">Cargando dispositivos…</p>
          </section>
        </main>
      </>
    );
  }

  if (!loggedIn) {
    return (
      <>
        <Navbar isLoggedIn={false} onToggle={onToggle} />
        <main className="disp-wrap">
          <section className="disp-card">
            <h1 className="disp-title">Dispositivos conectados</h1>
            <p className="disp-empty">
              Debes iniciar sesión para ver tus dispositivos conectados.
            </p>
          </section>
        </main>
      </>
    );
  }

  return (
    <>
      <Navbar isLoggedIn={true} onToggle={onToggle} />

      <main className="disp-wrap">
        <section className="disp-card">
          <div className="disp-header-row">
            <h1 className="disp-title">Dispositivos conectados</h1>

            {/* Toggle revoked */}
            {revokedDevices.length > 0 && (
              <button
                className="btn-secondary"
                onClick={() => setShowRevoked((v) => !v)}
                style={{ marginLeft: "auto" }}
              >
                {showRevoked
                  ? "Ocultar sesiones finalizadas"
                  : `Mostrar sesiones finalizadas (${revokedDevices.length})`}
              </button>
            )}
          </div>

          {error && <p className="disp-error">{error}</p>}

          {/* ---------- Active devices ---------- */}
          <h3 style={{ marginTop: 8 }}>Sesiones activas</h3>

          <ul className="disp-list">
            {activeDevices.map((d, idx) => (
              <li key={d.id} className={`disp-row ${idx ? "with-top" : ""}`}>
                <div className="left">
                  <div className="dev-icon" aria-hidden>
                    {iconFor(d.plataforma)}
                  </div>

                  <div className="dev-texts">
                    <div className="dev-name">
                      {d.plataforma === "Web" && d.navegador
                        ? `Web (${d.navegador})`
                        : d.plataforma || "Dispositivo"}

                      {/* Current device tag */}
                      {d.isCurrent && (
                        <span
                          className="dev-badge-current"
                          style={{
                            marginLeft: 8,
                            fontSize: 12,
                            padding: "2px 6px",
                            borderRadius: 999,
                            background: "#e7f5ff",
                          }}
                        >
                          Este dispositivo
                        </span>
                      )}
                    </div>

                    {(d.navegador || d.navegadorVersion) && (
                      <div className="dev-meta">
                        Navegador: {d.navegador || "—"}{" "}
                        {d.navegadorVersion || ""}
                      </div>
                    )}

                    <div className="dev-meta">Plataforma: {d.plataforma}</div>

                    {/* Show ONLY huella as identifier */}
                    <div className="dev-meta" title={d.huella}>
                      Huella: {d.huellaShort}
                    </div>

                    <div className="dev-meta">
                      Biometría: {d.biometria ? "Habilitada" : "No habilitada"}
                    </div>

                    <div className="dev-meta">
                      Estado: {estadoLabel(d.estadoRaw)}
                    </div>
                  </div>
                </div>

                <div className="right disp-actions">
                  <button
                    className="btn-baja"
                    onClick={() => setConfirming(d)}
                    disabled={bajandoId === d.id}
                  >
                    {bajandoId === d.id ? "Procesando..." : "Dar de baja"}
                  </button>
                </div>
              </li>
            ))}

            {activeDevices.length === 0 && !error && (
              <li className="disp-empty">No tienes sesiones activas.</li>
            )}
          </ul>

          {/* ---------- Revoked devices (hidden by default) ---------- */}
          {showRevoked && (
            <>
              <h3 style={{ marginTop: 16 }}>Sesiones finalizadas</h3>

              <ul className="disp-list">
                {revokedDevices.map((d, idx) => (
                  <li
                    key={d.id}
                    className={`disp-row ${idx ? "with-top" : ""}`}
                  >
                    <div className="left">
                      <div className="dev-icon" aria-hidden>
                        {iconFor(d.plataforma)}
                      </div>

                      <div className="dev-texts">
                        <div className="dev-name">
                          {d.plataforma === "Web" && d.navegador
                            ? `Web (${d.navegador})`
                            : d.plataforma || "Dispositivo"}

                          {d.isCurrent && (
                            <span
                              className="dev-badge-current"
                              style={{
                                marginLeft: 8,
                                fontSize: 12,
                                padding: "2px 6px",
                                borderRadius: 999,
                                background: "#e7f5ff",
                              }}
                            >
                              Este dispositivo
                            </span>
                          )}
                        </div>

                        {(d.navegador || d.navegadorVersion) && (
                          <div className="dev-meta">
                            Navegador: {d.navegador || "—"}{" "}
                            {d.navegadorVersion || ""}
                          </div>
                        )}

                        <div className="dev-meta">
                          Plataforma: {d.plataforma}
                        </div>

                        <div className="dev-meta" title={d.huella}>
                          Huella: {d.huellaShort}
                        </div>

                        <div className="dev-meta">
                          Biometría:{" "}
                          {d.biometria ? "Habilitada" : "No habilitada"}
                        </div>

                        <div className="dev-meta">
                          Estado: {estadoLabel(d.estadoRaw)}
                        </div>
                      </div>
                    </div>

                    <div className="right disp-actions">
                      <span className="estado-baja">Finalizada</span>
                    </div>
                  </li>
                ))}

                {revokedDevices.length === 0 && (
                  <li className="disp-empty">
                    No tienes sesiones finalizadas.
                  </li>
                )}
              </ul>
            </>
          )}
        </section>
      </main>

      {/* ---------- DELETE confirm modal ---------- */}
      {confirming && (
        <div className="disp-modal-overlay" onClick={() => setConfirming(null)}>
          <div className="disp-modal" onClick={(e) => e.stopPropagation()}>
            <h3>Confirmar baja</h3>
            <p>
              ¿Seguro que deseas dar de baja este dispositivo?
              <br />
              <strong title={confirming.huella}>
                Huella: {confirming.huellaShort}
              </strong>
            </p>

            <div className="modal-actions">
              <button
                className="btn-cancel"
                onClick={() => setConfirming(null)}
                disabled={bajandoId === confirming.id}
              >
                Cancelar
              </button>
              <button
                className="btn-confirm"
                onClick={() => darDeBaja(confirming.id)}
                disabled={bajandoId === confirming.id}
              >
                {bajandoId === confirming.id ? "Procesando..." : "Confirmar"}
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}
