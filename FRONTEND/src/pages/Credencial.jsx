import React, { useEffect, useState } from "react";
import Navbar from "../components/Navbar";
import { useAuth, toApi } from "../services/AuthService.jsx";
import "../styles/Credencial.css";

export default function Credencial({ isLoggedIn, onToggle }) {
  const { user, loading: authLoading } = useAuth();

  const [qrSeed, setQrSeed] = useState(Date.now());

  const [credencial, setCredencial] = useState(null);
  const [loadingCred, setLoadingCred] = useState(true);
  const [errorCred, setErrorCred] = useState(null);

  const [accesos, setAccesos] = useState([]);
  const [loadingAccesos, setLoadingAccesos] = useState(false);
  const [errorAccesos, setErrorAccesos] = useState(null);

  const [beneficios, setBeneficios] = useState([]);
  const [loadingBeneficios, setLoadingBeneficios] = useState(false);
  const [errorBeneficios, setErrorBeneficios] = useState(null);

  const loggedIn = !!user;

  // ------------------ Cargar credencial del usuario actual ------------------
  useEffect(() => {
    // Cada vez que cambia el usuario, limpiamos todo lo asociado
    setCredencial(null);
    setErrorCred(null);
    setAccesos([]);
    setBeneficios([]);
    setErrorAccesos(null);
    setErrorBeneficios(null);

    if (authLoading) {
      setLoadingCred(true);
      return;
    }

    // Si no hay usuario logueado, no hay credencial
    if (!user) {
      setLoadingCred(false);
      return;
    }

    const fetchCredencial = async () => {
      setLoadingCred(true);
      try {
        const res = await fetch(toApi("/credenciales"), {
          method: "GET",
          credentials: "include",
          headers: { Accept: "application/json" },
        });

        if (!res.ok) {
          const txt = await res.text();
          throw new Error(txt || `Error al cargar credenciales: ${res.status}`);
        }

        const data = await res.json();

        if (!Array.isArray(data)) {
          setCredencial(null);
          return;
        }

        const uId = user.usuarioId ?? user.id;

        const mine =
          data.find((c) => {
            // 👇 Ajusta estos campos según tu DTO real de credencial
            const cUserId =
              c.usuarioId ?? c.userId ?? c.usuario?.usuarioId ?? c.usuario?.id;

            const cEmail = c.usuarioEmail ?? c.email ?? c.usuario?.email;

            const matchesId = uId && cUserId && String(cUserId) === String(uId);

            const matchesEmail =
              user.email &&
              cEmail &&
              cEmail.toLowerCase() === user.email.toLowerCase();

            return matchesId || matchesEmail;
          }) ?? null;

        setCredencial(mine);
      } catch (err) {
        console.error("Error cargando credencial:", err);
        setErrorCred(err.message || "Error al cargar la credencial");
        setCredencial(null);
      } finally {
        setLoadingCred(false);
      }
    };

    fetchCredencial();
  }, [authLoading, user?.usuarioId, user?.email]);

  // ------------------ Helpers de UI ------------------
  const estado =
    credencial?.estado ?? (credencial ? "Activa" : "Sin credencial");

  const fechaEmision = (() => {
    if (!credencial) return null;
    // TODO: ajusta nombres de campos según tu DTO
    const raw =
      credencial.fechaEmision ?? credencial.emitidaEl ?? credencial.createdAt;
    if (!raw) return null;
    const d = new Date(raw);
    return isNaN(d) ? null : d.toLocaleDateString("es-UY");
  })();

  const fechaExpiracion = (() => {
    if (!credencial) return null;
    // TODO: ajusta nombres de campos según tu DTO
    const raw =
      credencial.fechaExpiracion ??
      credencial.expiraEl ??
      credencial.vigenteHasta;
    if (!raw) return null;
    const d = new Date(raw);
    return isNaN(d) ? null : d.toLocaleDateString("es-UY");
  })();

  const rolesTexto = (() => {
    const roles =
      user?.roles?.map((r) => r.nombre ?? r.name ?? r.descripcion) ??
      user?.usuarioRoles?.map((ur) => ur.nombre ?? ur.rolNombre) ??
      [];
    return roles.length ? roles.join(", ") : "Sin roles asignados";
  })();

  const nfcTexto = (() => {
    if (!credencial) return "No disponible";
    if (typeof credencial.tieneNfc === "boolean") {
      return credencial.tieneNfc ? "Disponible" : "No disponible";
    }
    return "Disponible";
  })();

  const credIdTexto =
    credencial?.id ??
    credencial?.credencialId ??
    credencial?.codigo ??
    "sin-id";

  const codigoVisible = credencial?.codigoVisible ?? `ID-${credIdTexto}`;

  // ------------------ Fetch Últimos accesos (eventos por CREDENCIAL) ------------------
  useEffect(() => {
    if (!credencial) {
      setAccesos([]);
      setErrorAccesos(null);
      setLoadingAccesos(false);
      return;
    }

    const fetchAccesos = async () => {
      setLoadingAccesos(true);
      setErrorAccesos(null);

      try {
        const res = await fetch(toApi("/eventos"), {
          method: "GET",
          credentials: "include",
          headers: { Accept: "application/json" },
        });

        if (!res.ok) {
          const txt = await res.text();
          throw new Error(txt || `Error al cargar eventos: ${res.status}`);
        }

        const data = await res.json();
        if (!Array.isArray(data)) {
          setAccesos([]);
          return;
        }

        // Id de la credencial actual (ajusta si tu DTO cambia)
        const credId =
          credencial.CredencialId ??
          credencial.credencialId ??
          credencial.id ??
          credencial.codigo;

        const filtrados = data.filter((e) => {
          // nombres tal cual salen del backend
          const eCredId =
            e.CredencialId ??
            e.credencialId ??
            e.Credencial?.CredencialId ??
            e.Credencial?.Id;

          return (
            credId &&
            eCredId &&
            String(eCredId).toLowerCase() === String(credId).toLowerCase()
          );
        });

        // Ordenar por MomentoDeAcceso descendente y quedarse con los 5 últimos
        const sorted = filtrados
          .slice()
          .sort((a, b) => {
            const da = new Date(
              a.MomentoDeAcceso ?? a.momentoDeAcceso ?? 0
            ).getTime();
            const db = new Date(
              b.MomentoDeAcceso ?? b.momentoDeAcceso ?? 0
            ).getTime();
            return db - da;
          })
          .slice(0, 5);

        setAccesos(sorted);
      } catch (err) {
        console.error("Error cargando accesos:", err);
        setErrorAccesos(err.message || "Error al cargar últimos accesos");
        setAccesos([]);
      } finally {
        setLoadingAccesos(false);
      }
    };

    fetchAccesos();
  }, [
    credencial?.id,
    credencial?.CredencialId,
    credencial?.credencialId,
    credencial?.codigo,
  ]);

  // ------------------ Fetch Beneficios activos ------------------
  useEffect(() => {
    if (!credencial) {
      setBeneficios([]);
      setErrorBeneficios(null);
      setLoadingBeneficios(false);
      return;
    }

    const fetchBeneficios = async () => {
      setLoadingBeneficios(true);
      setErrorBeneficios(null);

      try {
        const res = await fetch(toApi("/beneficios"), {
          method: "GET",
          credentials: "include",
          headers: { Accept: "application/json" },
        });

        if (!res.ok) {
          const txt = await res.text();
          throw new Error(txt || `Error al cargar beneficios: ${res.status}`);
        }

        const data = await res.json();
        if (!Array.isArray(data)) {
          setBeneficios([]);
          return;
        }

        const now = new Date();

        const vigentes = data.filter((b) => {
          // 👇 Ajusta estos campos según tu DTO de Beneficio
          const ini =
            b.vigenciaInicio ?? b.desde ?? b.fechaInicio ?? b.inicio ?? null;
          const fin = b.vigenciaFin ?? b.hasta ?? b.fechaFin ?? b.fin ?? null;

          const dIni = ini ? new Date(ini) : null;
          const dFin = fin ? new Date(fin) : null;

          const okIni = !dIni || dIni <= now;
          const okFin = !dFin || dFin >= now;

          return okIni && okFin;
        });

        setBeneficios(vigentes.slice(0, 5));
      } catch (err) {
        console.error("Error cargando beneficios:", err);
        setErrorBeneficios(err.message || "Error al cargar beneficios");
        setBeneficios([]);
      } finally {
        setLoadingBeneficios(false);
      }
    };

    fetchBeneficios();
  }, [credencial?.id, credencial?.credencialId, credencial?.codigo]);

  // ------------------ Acciones ------------------
  const recargarQR = () => {
    setQrSeed(Date.now());
  };

  const recargarDesdeServidor = () => {
    if (!user) return;

    setLoadingCred(true);
    setErrorCred(null);

    const refetch = async () => {
      try {
        const res = await fetch(toApi("/credenciales"), {
          method: "GET",
          credentials: "include",
          headers: { Accept: "application/json" },
        });

        if (!res.ok) {
          const txt = await res.text();
          throw new Error(
            txt || `Error al recargar credenciales: ${res.status}`
          );
        }

        const data = await res.json();

        if (!Array.isArray(data)) {
          setCredencial(null);
          return;
        }

        const uId = user.usuarioId ?? user.id;

        const mine =
          data.find((c) => {
            const cUserId =
              c.usuarioId ?? c.userId ?? c.usuario?.usuarioId ?? c.usuario?.id;

            const cEmail = c.usuarioEmail ?? c.email ?? c.usuario?.email;

            const matchesId = uId && cUserId && String(cUserId) === String(uId);

            const matchesEmail =
              user.email &&
              cEmail &&
              cEmail.toLowerCase() === user.email.toLowerCase();

            return matchesId || matchesEmail;
          }) ?? null;

        setCredencial(mine);
      } catch (err) {
        console.error("Error recargando credencial:", err);
        setErrorCred(err.message || "Error al recargar la credencial");
        setCredencial(null);
      } finally {
        setLoadingCred(false);
        setQrSeed(Date.now());
      }
    };

    refetch();
  };

  const validarBiometria = () => {
    alert("Validación biométrica no implementada aún (placeholder).");
  };

  const renovar = () => {
    alert("Renovación de credencial no implementada aún (placeholder).");
  };

  // ------------------ VISTAS ------------------

  if (authLoading || loadingCred) {
    return (
      <>
        <Navbar isLoggedIn={loggedIn ?? isLoggedIn} onToggle={onToggle} />
        <main className="cred-wrap">
          <section className="cred-card">
            <p className="loading">Cargando credencial...</p>
          </section>
        </main>
      </>
    );
  }

  if (!loggedIn) {
    return (
      <>
        <Navbar isLoggedIn={false} onToggle={onToggle} />
        <main className="cred-wrap">
          <section className="cred-card cred-card--center">
            <h1 className="cred-title">Credencial</h1>
            <p className="cred-message">
              Debes iniciar sesión para ver tu credencial.
            </p>
          </section>
        </main>
      </>
    );
  }

  if (!credencial) {
    return (
      <>
        <Navbar isLoggedIn={true} onToggle={onToggle} />
        <main className="cred-wrap">
          <section className="cred-card cred-card--center">
            <h1 className="cred-title">Credencial</h1>
            <p className="cred-message">No hay credencial registrada.</p>

            <div className="cred-actions" role="toolbar">
              <button
                type="button"
                className="btn ghost"
                onClick={recargarDesdeServidor}
              >
                Reintentar carga
              </button>
            </div>

            {errorCred && (
              <div className="cred-error" role="alert">
                <strong>Ocurrió un error:</strong> {errorCred}
              </div>
            )}
          </section>
        </main>
      </>
    );
  }

  return (
    <>
      <Navbar isLoggedIn={true} onToggle={onToggle} />

      <main className="cred-wrap">
        <section className="cred-card" aria-labelledby="cred-title">
          <h1 id="cred-title" className="sr-only">
            Credencial
          </h1>

          {/* panel principal */}
          <div className="cred-top">
            <div
              className="cred-info"
              role="table"
              aria-label="Información de la credencial"
            >
              <div className="cred-row" role="row">
                <span className="k" role="rowheader">
                  Credencial
                </span>
                <span className="v" role="cell">
                  {codigoVisible}
                </span>
              </div>
              <div className="cred-row" role="row">
                <span className="k" role="rowheader">
                  Usuario
                </span>
                <span className="v" role="cell">
                  {user?.nombre} {user?.apellido}
                </span>
              </div>
              <div className="cred-row" role="row">
                <span className="k" role="rowheader">
                  Roles
                </span>
                <span className="v" role="cell">
                  {rolesTexto}
                </span>
              </div>
              <div className="cred-row" role="row">
                <span className="k" role="rowheader">
                  Tipo
                </span>
                <span className="v" role="cell">
                  {credencial.tipo ?? "Digital"}
                </span>
              </div>
              <div className="cred-row" role="row">
                <span className="k" role="rowheader">
                  Estado
                </span>
                <span
                  className={`badge ${
                    estado === "Activa" || estado === "Renovada" ? "on" : ""
                  }`}
                  role="cell"
                >
                  {estado}
                </span>
              </div>
              <div className="cred-row" role="row">
                <span className="k" role="rowheader">
                  Fecha emisión
                </span>
                <span className="v" role="cell">
                  {fechaEmision ?? "—"}
                </span>
              </div>
              <div className="cred-row" role="row">
                <span className="k" role="rowheader">
                  Fecha expiración
                </span>
                <span className="v" role="cell">
                  {fechaExpiracion ?? "—"}
                </span>
              </div>
              <div className="cred-row" role="row">
                <span className="k" role="rowheader">
                  NFC
                </span>
                <span className="v" role="cell">
                  {nfcTexto}
                </span>
              </div>
              <div className="cred-row" role="row">
                <span className="k" role="rowheader">
                  ID
                </span>
                <span className="v mono" role="cell">
                  {credIdTexto}
                </span>
              </div>
            </div>

            {/* QR */}
            <div className="qr-box" aria-label="Código QR">
              <div className="qr" key={qrSeed} aria-hidden>
                {[...Array(25)].map((_, i) => (
                  <span key={i} className={Math.random() > 0.5 ? "b" : ""} />
                ))}
              </div>
              <div className="qr-caption">Código QR</div>
              <button
                type="button"
                className="btn ghost btn-small"
                onClick={recargarQR}
              >
                Recargar QR
              </button>
            </div>
          </div>

          {/* secciones inferiores */}
          <div className="cred-grid">
            <section className="panel" aria-labelledby="ultimos-accesos">
              <div className="panel-header">
                <h3 id="ultimos-accesos">Últimos accesos</h3>
                {loadingAccesos && (
                  <span className="panel-tag">Cargando...</span>
                )}
              </div>

              {errorAccesos && (
                <p className="panel-error">
                  Error al cargar accesos: {errorAccesos}
                </p>
              )}

              {!loadingAccesos && !errorAccesos && (
                <ul className="list">
                  {accesos.length === 0 && <li>No hay accesos recientes.</li>}
                  {accesos.map((a) => {
                    // Fecha: MomentoDeAcceso
                    const fechaRaw = a.MomentoDeAcceso ?? a.momentoDeAcceso;
                    const d = fechaRaw ? new Date(fechaRaw) : null;
                    const fechaTxt = d
                      ? d.toLocaleString("es-UY")
                      : "Fecha desconocida";

                    // Lugar: nombre del espacio
                    const lugar =
                      a.Espacio?.Nombre ??
                      a.Espacio?.nombre ??
                      a.espacioNombre ??
                      "Acceso";

                    // Modo / Resultado
                    const modo = a.Modo ?? a.modo; // Online / NFC / QR / etc.
                    const resultado = a.Resultado ?? a.resultado; // Permitir / Denegar...

                    const metodoTexto =
                      [modo, resultado].filter(Boolean).join(" · ") ||
                      "QR/NFC/Online";

                    return (
                      <li key={a.EventoId ?? a.eventoId ?? fechaRaw}>
                        {fechaTxt} – {lugar} ({metodoTexto})
                      </li>
                    );
                  })}
                </ul>
              )}
            </section>

            {/* Beneficios activos */}
            <section className="panel" aria-labelledby="beneficios-activos">
              <div className="panel-header">
                <h3 id="beneficios-activos">Beneficios Activos</h3>
                {loadingBeneficios && (
                  <span className="panel-tag">Cargando...</span>
                )}
              </div>

              {errorBeneficios && (
                <p className="panel-error">
                  Error al cargar beneficios: {errorBeneficios}
                </p>
              )}

              {!loadingBeneficios && !errorBeneficios && (
                <ul className="list">
                  {beneficios.length === 0 && (
                    <li>No hay beneficios activos.</li>
                  )}
                  {beneficios.map((b) => (
                    <li key={b.id}>
                      <strong>{b.nombre}</strong>
                      {b.descripcion ? ` – ${b.descripcion}` : ""}
                    </li>
                  ))}
                </ul>
              )}
            </section>
          </div>

          {/* acciones */}
          <div
            className="cred-actions"
            role="toolbar"
            aria-label="Acciones de credencial"
          >
            <button
              type="button"
              className="btn ghost"
              onClick={recargarDesdeServidor}
            >
              Recargar desde servidor
            </button>
            <button type="button" className="btn" onClick={renovar}>
              Renovar
            </button>
            <button
              type="button"
              className="btn ghost"
              onClick={validarBiometria}
            >
              Validar Biometría
            </button>
          </div>

          {errorCred && (
            <div className="cred-error" role="alert">
              <strong>Ocurrió un error:</strong> {errorCred}
            </div>
          )}
        </section>
      </main>
    </>
  );
}
