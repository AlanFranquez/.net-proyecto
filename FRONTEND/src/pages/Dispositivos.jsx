import React, { useEffect, useMemo, useState } from "react";
import Navbar from "../components/Navbar";
import { useAuth, toApi } from "../services/AuthService.jsx";
import "../styles/Dispositivos.css";

export default function Dispositivos({ isLoggedIn, onToggle }) {
  const { user, loading: authLoading } = useAuth();

  const [devices, setDevices] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const [confirming, setConfirming] = useState(null); // id del dispositivo a dar de baja
  const [bajandoId, setBajandoId] = useState(null);

  const loggedIn = !!user;

  // ---------- Cargar dispositivos del usuario actual ----------
  useEffect(() => {
    setDevices([]);
    setError(null);
    setLoading(true);

    if (authLoading) return;

    if (!user) {
      setLoading(false);
      return;
    }

    const fetchDevices = async () => {
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

          return (
            uId &&
            dUserId &&
            String(dUserId) === String(uId)
          );
        });

        setDevices(mine);
      } catch (err) {
        console.error("Error cargando dispositivos:", err);
        setError(err.message || "Error al cargar dispositivos");
      } finally {
        setLoading(false);
      }
    };

    fetchDevices();
  }, [authLoading, user?.usuarioId, user?.id]);

  // ---------- Normalización para UI ----------
  const devicesUI = useMemo(
    () =>
      devices.map((d) => {
        const id =
          d.DispositivoId ??
          d.dispositivoId ??
          d.Id ??
          d.id;

        const plataformaRaw =
          (d.Plataforma ?? d.plataforma ?? "").toString();

        const plataforma = plataformaRaw || "Desconocido";

        const estadoRaw =
          (d.Estado ?? d.estado ?? "").toString().toLowerCase();

        const activo =
          !estadoRaw.includes("baja") &&
          !estadoRaw.includes("inactivo") &&
          !estadoRaw.includes("revocado");

        const biometria =
          typeof d.BiometriaHabilitada === "boolean"
            ? d.BiometriaHabilitada
            : d.biometriaHabilitada;

        const numero =
          d.NumeroTelefono ??
          d.numeroTelefono ??
          "—";

        const huella =
          d.HuellaDispositivo ??
          d.huellaDispositivo ??
          "—";

        return {
          raw: d,
          id,
          plataforma,
          estadoRaw,
          activo,
          biometria,
          numero,
          huella,
        };
      }),
    [devices]
  );

  const iconFor = (plataforma) => {
    const p = (plataforma || "").toLowerCase();
    if (p.includes("android") || p.includes("ios") || p.includes("phone")) {
      return "📱";
    }
    if (p.includes("windows") || p.includes("mac") || p.includes("desktop")) {
      return "💻";
    }
    return "🔧";
  };

  const estadoLabel = (estadoRaw) => {
    const s = (estadoRaw || "").toLowerCase();
    if (s.includes("activo")) return "Activo";
    if (s.includes("baja")) return "Dado de baja";
    if (s.includes("inactivo")) return "Inactivo";
    if (s.includes("revocado")) return "Revocado";
    return estadoRaw || "Desconocido";
  };

  // ---------- Dar de baja dispositivo (DELETE /api/dispositivos) ----------
  const darDeBaja = async (deviceId) => {
    if (!deviceId) return;
    setBajandoId(deviceId);

    try {
      const res = await fetch(toApi("/dispositivos"), {
        method: "DELETE",
        credentials: "include",
        headers: {
          "Content-Type": "application/json",
        },
        // DeleteDispositivoCommand casi seguro tiene DispositivoId
        body: JSON.stringify({ dispositivoId: deviceId }),
      });

      if (!res.ok && res.status !== 204) {
        const txt = await res.text();
        throw new Error(txt || `Error al dar de baja el dispositivo: ${res.status}`);
      }

      // Actualizamos estado local
      setDevices((prev) =>
        prev.map((d) => {
          const dId =
            d.DispositivoId ??
            d.dispositivoId ??
            d.Id ??
            d.id;
          if (String(dId) !== String(deviceId)) return d;
          return {
            ...d,
            Estado: "DadoDeBaja",
          };
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

  // ---------- Vistas globales ----------
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
          <h1 className="disp-title">Dispositivos conectados</h1>

          {error && <p className="disp-error">{error}</p>}

          <ul className="disp-list">
            {devicesUI.map((d, idx) => (
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
                      {d.plataforma || "Dispositivo"}
                    </div>
                    <div className="dev-meta">
                      ID interno: {d.id}
                    </div>
                    <div className="dev-meta">
                      Número: {d.numero}
                    </div>
                    <div className="dev-meta">
                      Biometría: {d.biometria ? "Habilitada" : "No habilitada"}
                    </div>
                    <div className="dev-meta">
                      Estado: {estadoLabel(d.estadoRaw)}
                    </div>
                  </div>
                </div>

                <div className="right">
                  {d.activo ? (
                    <button
                      className="btn-baja"
                      onClick={() => setConfirming(d.id)}
                      disabled={bajandoId === d.id}
                    >
                      {bajandoId === d.id ? "Procesando..." : "Dar de baja"}
                    </button>
                  ) : (
                    <span className="estado-baja">Dado de baja</span>
                  )}
                </div>
              </li>
            ))}

            {devicesUI.length === 0 && !error && (
              <li className="disp-empty">
                No tienes dispositivos registrados.
              </li>
            )}
          </ul>
        </section>
      </main>

      {confirming && (
        <div
          className="disp-modal-overlay"
          onClick={() => setConfirming(null)}
        >
          <div
            className="disp-modal"
            onClick={(e) => e.stopPropagation()}
          >
            <h3>Confirmar baja</h3>
            <p>
              ¿Seguro que deseas dar de baja el dispositivo con ID{" "}
              <strong>{confirming}</strong>?
            </p>
            <div className="modal-actions">
              <button
                className="btn-cancel"
                onClick={() => setConfirming(null)}
                disabled={bajandoId === confirming}
              >
                Cancelar
              </button>
              <button
                className="btn-confirm"
                onClick={() => darDeBaja(confirming)}
                disabled={bajandoId === confirming}
              >
                {bajandoId === confirming ? "Procesando..." : "Confirmar"}
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}
