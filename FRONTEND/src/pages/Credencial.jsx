import React, { useEffect, useState } from "react";
import Navbar from "../components/Navbar";
import { useAuth, toApi } from "../services/AuthService.jsx";
import "../styles/Credencial.css";

export default function Credencial({ isLoggedIn, onToggle }) {
  const { user, loading: authLoading } = useAuth();

  const [credencial, setCredencial] = useState(null);
  const [loadingCred, setLoadingCred] = useState(true);
  const [errorCred, setErrorCred] = useState(null);

  const [accesos, setAccesos] = useState([]);
  const [loadingAccesos, setLoadingAccesos] = useState(false);
  const [errorAccesos, setErrorAccesos] = useState(null);

  const [beneficios, setBeneficios] = useState([]);
  const [loadingBeneficios, setLoadingBeneficios] = useState(false);
  const [errorBeneficios, setErrorBeneficios] = useState(null);

  const [roles, setRoles] = useState([]);
  const [loadingRoles, setLoadingRoles] = useState(false);
  const [errorRoles, setErrorRoles] = useState(null);

  const [currentUsuario, setCurrentUsuario] = useState(null);
  const [loadingUsuario, setLoadingUsuario] = useState(false);
  const [errorUsuario, setErrorUsuario] = useState(null);

  const loggedIn = !!user;

  // ---------- Cargar usuario completo desde /usuarios (para tener RolesIDs, BeneficiosIDs, etc) ----------
  useEffect(() => {
    if (authLoading || !user) {
      setCurrentUsuario(null);
      setLoadingUsuario(false);
      setErrorUsuario(null);
      return;
    }

    const fetchUsuarios = async () => {
      setLoadingUsuario(true);
      setErrorUsuario(null);
      try {
        const res = await fetch(toApi("/usuarios"), {
          method: "GET",
          credentials: "include",
          headers: { Accept: "application/json" },
        });

        if (!res.ok) {
          const txt = await res.text();
          throw new Error(txt || `Error al cargar usuarios: ${res.status}`);
        }

        const data = await res.json();
        if (!Array.isArray(data)) {
          setCurrentUsuario(null);
          return;
        }

        const uId = user.usuarioId ?? user.UsuarioId ?? user.id;
        const uIdStr = uId ? String(uId) : null;
        const uEmail = (user.email ?? user.Email ?? "").toLowerCase();

        const mine =
          data.find((u) => {
            const uuId = u.usuarioId ?? u.UsuarioId ?? u.id;
            const uuIdStr = uuId ? String(uuId) : null;
            const uEmail2 = (u.email ?? u.Email ?? "").toLowerCase();

            const idMatch = uIdStr && uuIdStr && uuIdStr === uIdStr;
            const emailMatch = uEmail && uEmail2 && uEmail2 === uEmail;
            return idMatch || emailMatch;
          }) ?? null;

        setCurrentUsuario(mine);
      } catch (err) {
        console.error("Error cargando usuarios:", err);
        setErrorUsuario(err.message || "Error al cargar usuarios");
        setCurrentUsuario(null);
      } finally {
        setLoadingUsuario(false);
      }
    };

    fetchUsuarios();
  }, [authLoading, user?.usuarioId, user?.email, user?.id]);

  // ------------------ Cargar credencial del usuario actual ------------------
  useEffect(() => {
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

        const userCredId =
          currentUsuario?.credencialId ??
          currentUsuario?.CredencialId ??
          user.credencialId ??
          user.CredencialId ??
          null;
        const userCredIdStr = userCredId ? String(userCredId) : null;

        const uId = currentUsuario?.usuarioId ?? user.usuarioId ?? user.id;
        const uIdStr = uId ? String(uId) : null;
        const uEmail = (currentUsuario?.email ?? user.email ?? "").toLowerCase();

        const mine =
          data.find((c) => {
            const cCred = c.credencialId ?? c.CredencialId ?? c.id;
            const cCredStr = cCred ? String(cCred) : null;

            const cUserId =
              c.usuarioId ?? c.UsuarioId ?? c.usuario?.usuarioId ?? c.usuario?.id;
            const cUserIdStr = cUserId ? String(cUserId) : null;

            const cEmail = (
              c.usuarioEmail ?? c.email ?? c.usuario?.email ?? ""
            ).toLowerCase();

            const matchCred =
              userCredIdStr && cCredStr && cCredStr === userCredIdStr;
            const matchId = uIdStr && cUserIdStr && cUserIdStr === uIdStr;
            const matchEmail = uEmail && cEmail && cEmail === uEmail;

            return matchCred || matchId || matchEmail;
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
  }, [
    authLoading,
    user?.usuarioId,
    user?.email,
    user?.id,
    user?.credencialId,
    currentUsuario?.usuarioId,
    currentUsuario?.email,
    currentUsuario?.credencialId,
  ]);

  // ------------------ Cargar roles del usuario ------------------
  useEffect(() => {
    if (authLoading || !user || !currentUsuario) {
      setRoles([]);
      setErrorRoles(null);
      setLoadingRoles(false);
      return;
    }

    const fetchRoles = async () => {
      setLoadingRoles(true);
      setErrorRoles(null);
      try {
        const res = await fetch(toApi("/roles"), {
          method: "GET",
          credentials: "include",
          headers: { Accept: "application/json" },
        });

        if (!res.ok) {
          const txt = await res.text();
          throw new Error(txt || `Error al cargar roles: ${res.status}`);
        }

        const data = await res.json();
        if (!Array.isArray(data)) {
          setRoles([]);
          return;
        }

        const usuarioId =
          currentUsuario.usuarioId ??
          currentUsuario.UsuarioId ??
          user.usuarioId ??
          user.UsuarioId ??
          user.id;
        const usuarioIdStr = usuarioId ? String(usuarioId) : null;

        const userRoleIdsRaw =
          currentUsuario.rolesIDs ??
          currentUsuario.RolesIDs ??
          user.rolesIDs ??
          user.RolesIDs ??
          [];
        const userRoleIds = Array.isArray(userRoleIdsRaw)
          ? userRoleIdsRaw.map((x) => String(x))
          : [];

        const mine = data.filter((r) => {
          const rId = r.rolId ?? r.RolId ?? r.id;
          const rIdStr = rId ? String(rId) : null;

          const usuariosIDs = Array.isArray(r.usuariosIDs ?? r.UsuariosIDs)
            ? (r.usuariosIDs ?? r.UsuariosIDs).map((x) => String(x))
            : [];

          const byRoleIdList =
            rIdStr && userRoleIds.length && userRoleIds.includes(rIdStr);

          const byUserList =
            usuarioIdStr &&
            usuariosIDs.length &&
            usuariosIDs.includes(usuarioIdStr);

          return byRoleIdList || byUserList;
        });

        setRoles(mine);
      } catch (err) {
        console.error("Error cargando roles:", err);
        setErrorRoles(err.message || "Error al cargar roles");
        setRoles([]);
      } finally {
        setLoadingRoles(false);
      }
    };

    fetchRoles();
  }, [
    authLoading,
    user?.usuarioId,
    user?.id,
    user?.rolesIDs,
    currentUsuario?.usuarioId,
    currentUsuario?.rolesIDs,
  ]);

  // ------------------ Helpers de UI ------------------

  // 🔽🔽🔽 SOLO CAMBIA ESTA PARTE (estado) 🔽🔽🔽

  // Normalizar estado venga como string o número (enum)
  const estado = (() => {
    if (!credencial) return "—";

    const raw = credencial.estado ?? credencial.Estado;

    if (raw === null || raw === undefined) return "—";

    // Caso 1: string ("Emitida", "Activada", etc.)
    if (typeof raw === "string") {
      const trimmed = raw.trim();
      return trimmed || "—";
    }

    // Caso 2: número (0,1,2,3 según tu enum)
    if (typeof raw === "number") {
      const map = {
        0: "Emitida",
        1: "Activada",
        2: "Suspendida",
        3: "Expirada",
      };
      return map[raw] ?? String(raw);
    }

    // Cualquier otra cosa rara
    return String(raw);
  })();

  // qué estados se muestran como "encendidos" visualmente
  const estadoBadgeOn = ["Emitida", "Activada"].includes(estado);

  // 🔼🔼🔼 SOLO CAMBIA ESTA PARTE (estado) 🔼🔼🔼

  const fechaEmision = (() => {
    if (!credencial) return null;
    const raw = credencial.fechaEmision ?? credencial.FechaEmision;
    if (!raw) return null;
    const d = new Date(raw);
    return isNaN(d) ? null : d.toLocaleDateString("es-UY");
  })();

  const fechaExpiracion = (() => {
    if (!credencial) return null;
    const raw = credencial.fechaExpiracion ?? credencial.FechaExpiracion;
    if (!raw) return null;
    const d = new Date(raw);
    return isNaN(d) ? null : d.toLocaleDateString("es-UY");
  })();

  const rolesTexto = (() => {
    if (loadingRoles || loadingUsuario) return "Cargando roles...";
    if (errorRoles || errorUsuario) return "Error al cargar roles";
    if (!roles || !roles.length) return "Sin roles asignados";
    return roles
      .map((r) => r.tipo ?? r.Tipo ?? r.nombre ?? r.descripcion)
      .join(", ");
  })();

  // NFC: siempre "Activado"
  const nfcTexto = "Activado";

  // ID mostrado: idCriptografico, si no, credencialId
  const credId =
    credencial?.idCriptografico ??
    credencial?.IdCriptografico ??
    credencial?.credencialId ??
    credencial?.CredencialId ??
    "—";

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

        const credGuid =
          credencial.credencialId ??
          credencial.CredencialId ??
          credencial.id ??
          credencial.codigo;
        const credGuidStr = credGuid ? String(credGuid).toLowerCase() : null;

        const filtrados = data.filter((e) => {
          const eCredId =
            e.credencialId ??
            e.CredencialId ??
            e.Credencial?.CredencialId ??
            e.Credencial?.Id;
          const eCredIdStr = eCredId ? String(eCredId).toLowerCase() : null;

          return credGuidStr && eCredIdStr && eCredIdStr === credGuidStr;
        });

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

  // ------------------ Fetch Beneficios activos (vigentes + asignados al usuario) ------------------
  useEffect(() => {
    if (!credencial || !currentUsuario) {
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
        const usuarioId =
          currentUsuario.usuarioId ??
          currentUsuario.UsuarioId ??
          user?.usuarioId ??
          user?.UsuarioId ??
          user?.id;
        const usuarioIdStr = usuarioId ? String(usuarioId) : null;

        const beneficiosUsuarioIdsRaw =
          currentUsuario.beneficiosIDs ??
          currentUsuario.BeneficiosIDs ??
          user?.beneficiosIDs ??
          user?.BeneficiosIDs ??
          [];
        const beneficiosUsuarioIds = Array.isArray(beneficiosUsuarioIdsRaw)
          ? beneficiosUsuarioIdsRaw.map((x) => String(x))
          : [];

        const vigentesYAsignados = data.filter((b) => {
          const ini = b.vigenciaInicio ?? b.desde ?? b.fechaInicio ?? b.inicio;
          const fin = b.vigenciaFin ?? b.hasta ?? b.fechaFin ?? b.fin;

          const dIni = ini ? new Date(ini) : null;
          const dFin = fin ? new Date(fin) : null;

          const okIni = !dIni || dIni <= now;
          const okFin = !dFin || dFin >= now;
          if (!okIni || !okFin) return false;

          const usuariosIDs = Array.isArray(b.usuariosIDs ?? b.UsuariosIDs)
            ? (b.usuariosIDs ?? b.UsuariosIDs).map((x) => String(x))
            : [];

          const byUserList =
            usuarioIdStr &&
            usuariosIDs.length &&
            usuariosIDs.includes(usuarioIdStr);

          const byIdList =
            beneficiosUsuarioIds.length &&
            beneficiosUsuarioIds.includes(String(b.id));

          return byUserList || byIdList;
        });

        setBeneficios(vigentesYAsignados.slice(0, 5));
      } catch (err) {
        console.error("Error cargando beneficios:", err);
        setErrorBeneficios(err.message || "Error al cargar beneficios");
        setBeneficios([]);
      } finally {
        setLoadingBeneficios(false);
      }
    };

    fetchBeneficios();
  }, [
    credencial?.credencialId,
    currentUsuario?.usuarioId,
    currentUsuario?.beneficiosIDs,
  ]);

  // ------------------ Acciones ------------------
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

        const userCredId =
          currentUsuario?.credencialId ??
          user.credencialId ??
          user.CredencialId ??
          null;
        const userCredIdStr = userCredId ? String(userCredId) : null;

        const uId =
          currentUsuario?.usuarioId ?? user.usuarioId ?? user.UsuarioId ?? user.id;
        const uIdStr = uId ? String(uId) : null;
        const uEmail = (currentUsuario?.email ?? user.email ?? "").toLowerCase();

        const mine =
          data.find((c) => {
            const cCred = c.credencialId ?? c.CredencialId ?? c.id;
            const cCredStr = cCred ? String(cCred) : null;

            const cUserId =
              c.usuarioId ?? c.UsuarioId ?? c.usuario?.usuarioId ?? c.usuario?.id;
            const cUserIdStr = cUserId ? String(cUserId) : null;

            const cEmail = (
              c.usuarioEmail ?? c.email ?? c.usuario?.email ?? ""
            ).toLowerCase();

            const matchCred =
              userCredIdStr && cCredStr && cCredStr === userCredIdStr;
            const matchId = uIdStr && cUserIdStr && cUserIdStr === uIdStr;
            const matchEmail = uEmail && cEmail && cEmail === uEmail;

            return matchCred || matchId || matchEmail;
          }) ?? null;

        setCredencial(mine);
      } catch (err) {
        console.error("Error recargando credencial:", err);
        setErrorCred(err.message || "Error al recargar la credencial");
        setCredencial(null);
      } finally {
        setLoadingCred(false);
      }
    };

    refetch();
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
                <span className="v mono" role="cell">
                  {credId}
                </span>
              </div>
              <div className="cred-row" role="row">
                <span className="k" role="rowheader">
                  Usuario
                </span>
                <span className="v" role="cell">
                  {currentUsuario?.nombre ?? user?.nombre}{" "}
                  {currentUsuario?.apellido ?? user?.apellido}
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
                  {credencial.tipo ?? credencial.Tipo ?? "Digital"}
                </span>
              </div>
              <div className="cred-row" role="row">
                <span className="k" role="rowheader">
                  Estado
                </span>
                <span
                  className={`badge ${estadoBadgeOn ? "on" : ""}`}
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
                    const fechaRaw = a.MomentoDeAcceso ?? a.momentoDeAcceso;
                    const d = fechaRaw ? new Date(fechaRaw) : null;
                    const fechaTxt = d
                      ? d.toLocaleString("es-UY")
                      : "Fecha desconocida";

                    const lugar =
                      a.Espacio?.Nombre ??
                      a.Espacio?.nombre ??
                      a.espacioNombre ??
                      "Acceso";

                    const modo = a.Modo ?? a.modo;
                    const resultado = a.Resultado ?? a.resultado;

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
