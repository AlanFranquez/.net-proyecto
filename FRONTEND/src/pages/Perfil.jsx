// src/pages/Perfil.jsx
import React, { useEffect, useMemo, useState } from "react";
import Navbar from "../components/Navbar";
import { useAuth, toApi } from "../services/AuthService.jsx";
import "../styles/Perfil.css";

export default function Perfil({ isLoggedIn, onToggle }) {
  const { user, loading: authLoading, error: authError, refetchUser } = useAuth();

  const [currentUsuario, setCurrentUsuario] = useState(null);
  const [loadingUsuario, setLoadingUsuario] = useState(false);
  const [errorUsuario, setErrorUsuario] = useState(null);

  const [roles, setRoles] = useState([]);
  const [loadingRoles, setLoadingRoles] = useState(false);
  const [errorRoles, setErrorRoles] = useState(null);

  // Form state for editable fields
  const [formNombre, setFormNombre] = useState("");
  const [formApellido, setFormApellido] = useState("");
  const [formEmail, setFormEmail] = useState("");
  const [formDocumento, setFormDocumento] = useState("");

  const [saving, setSaving] = useState(false);
  const [saveError, setSaveError] = useState(null);
  const [saveSuccess, setSaveSuccess] = useState(null);

  const loggedIn = !!user;

  // Debug: ver qué llega desde /me
  if (user) {
    // eslint-disable-next-line no-console
    console.log("Usuario desde /api/usuarios/me:", user);
  }

  // --------- Inicializar formulario cuando cambia el usuario o currentUsuario ----------
  useEffect(() => {
    const src = currentUsuario ?? user;

    if (!src) {
      setFormNombre("");
      setFormApellido("");
      setFormEmail("");
      setFormDocumento("");
      return;
    }

    setFormNombre(src.nombre ?? src.Nombre ?? "");
    setFormApellido(src.apellido ?? src.Apellido ?? "");
    setFormEmail(src.email ?? src.Email ?? "");
    setFormDocumento(src.documento ?? src.Documento ?? "");
  }, [
    user?.usuarioId,
    user?.email,
    currentUsuario?.usuarioId,
    currentUsuario?.email,
  ]);

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

  // ------------------ Cargar roles del usuario (igual que en Credencial) ------------------
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

  // Roles normalizados para mostrar
  const rolesNormalizados = useMemo(() => {
    if (loadingRoles || loadingUsuario) return [];
    if (!roles || !roles.length) return [];

    return roles.map(
      (r) =>
        r.tipo ??
        r.Tipo ??
        r.nombre ??
        r.Nombre ??
        r.descripcion ??
        r.Descripcion ??
        "Rol"
    );
  }, [roles, loadingRoles, loadingUsuario]);

  // ------------------ Guardar cambios (PUT /usuarios/{id} con solo campos modificados) ------------------
  const handleSave = async (e) => {
    e.preventDefault();
    if (!user) return;

    setSaveError(null);
    setSaveSuccess(null);

    const usuarioId = user.usuarioId ?? user.UsuarioId ?? user.id;
    if (!usuarioId) {
      setSaveError("No se pudo determinar el ID del usuario.");
      return;
    }

    const payload = {};
    const src = currentUsuario ?? user;

    const originalNombre = src.nombre ?? src.Nombre ?? "";
    const originalApellido = src.apellido ?? src.Apellido ?? "";
    const originalEmail = src.email ?? src.Email ?? "";
    const originalDocumento = src.documento ?? src.Documento ?? "";

    if (formNombre.trim() !== originalNombre) {
      payload.nombre = formNombre.trim();
    }
    if (formApellido.trim() !== originalApellido) {
      payload.apellido = formApellido.trim();
    }
    if (formEmail.trim() !== originalEmail) {
      payload.email = formEmail.trim();
    }
    if (formDocumento.trim() !== originalDocumento) {
      payload.documento = formDocumento.trim();
    }

    if (Object.keys(payload).length === 0) {
      setSaveSuccess("No hay cambios para guardar.");
      return;
    }

    try {
      setSaving(true);
      const res = await fetch(toApi(`/usuarios/${usuarioId}`), {
        method: "PUT",
        credentials: "include",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(payload),
      });

      if (!res.ok) {
        const txt = await res.text();
        throw new Error(txt || `Error al actualizar usuario: ${res.status}`);
      }

      setSaveSuccess("Datos actualizados correctamente.");
      await refetchUser?.();
    } catch (err) {
      console.error("Error guardando usuario:", err);
      setSaveError(err.message || "Error al guardar cambios.");
    } finally {
      setSaving(false);
    }
  };

  // ------------------ VISTAS ------------------

  if (authLoading) {
    return (
      <>
        <Navbar isLoggedIn={loggedIn ?? isLoggedIn} onToggle={onToggle} />
        <main className="profile-wrap">
          <p className="loading">Cargando perfil...</p>
        </main>
      </>
    );
  }

  if (!loggedIn) {
    return (
      <>
        <Navbar isLoggedIn={false} onToggle={onToggle} />
        <main className="profile-wrap">
          <section className="profile-card profile-card--center">
            <h1 className="profile-title">Perfil</h1>
            <p className="profile-message">
              Debes iniciar sesión para ver tu perfil.
            </p>
          </section>
        </main>
      </>
    );
  }

  const { nombre, apellido, email, estado } = user;

  return (
    <>
      <Navbar isLoggedIn={true} onToggle={onToggle} />

      <main className="profile-wrap">
        <section className="profile-card" aria-labelledby="profile-title">
          <header className="profile-head">
            <div className="avatar" aria-hidden="true">
              <span role="img" aria-label="user">
                👤
              </span>
            </div>

            <div className="title-block">
              <h1 id="profile-title" className="profile-name">
                {nombre} {apellido}
              </h1>
              <p className="profile-subtitle">
                {email} ·{" "}
                <span className="badge badge--state">{estado}</span>
              </p>
            </div>

            <aside className="roles" aria-label="Roles">
              <h2 className="roles-title">Roles</h2>
              {loadingRoles || loadingUsuario ? (
                <p className="roles-empty">Cargando roles…</p>
              ) : errorRoles || errorUsuario ? (
                <p className="roles-empty">Error al cargar roles.</p>
              ) : rolesNormalizados.length > 0 ? (
                <div className="roles-chips">
                  {rolesNormalizados.map((r) => (
                    <span key={r} className="chip">
                      {r}
                    </span>
                  ))}
                </div>
              ) : (
                <p className="roles-empty">Sin roles asignados</p>
              )}
            </aside>
          </header>

          {/* Datos de la cuenta (EDITABLES) */}
          <section
            className="data-box"
            role="region"
            aria-labelledby="datos-title"
          >
            <div className="data-box-header">
              <h2 id="datos-title" className="data-title">
                Datos de la cuenta
              </h2>
            </div>

            <form onSubmit={handleSave}>
              <dl className="data-grid">
                <div className="data-row">
                  <dt className="k">Nombre</dt>
                  <dd className="v">
                    <input
                      type="text"
                      value={formNombre}
                      onChange={(e) => setFormNombre(e.target.value)}
                    />
                  </dd>
                </div>

                <div className="data-row">
                  <dt className="k">Apellido</dt>
                  <dd className="v">
                    <input
                      type="text"
                      value={formApellido}
                      onChange={(e) => setFormApellido(e.target.value)}
                    />
                  </dd>
                </div>

                <div className="data-row">
                  <dt className="k">Correo</dt>
                  <dd className="v">
                    <input
                      type="email"
                      value={formEmail}
                      onChange={(e) => setFormEmail(e.target.value)}
                    />
                  </dd>
                </div>

                <div className="data-row">
                  <dt className="k">Documento</dt>
                  <dd className="v">
                    <input
                      type="text"
                      value={formDocumento}
                      onChange={(e) => setFormDocumento(e.target.value)}
                    />
                  </dd>
                </div>
              </dl>

              <div className="profile-actions">
                <button
                  type="submit"
                  className="primary-btn"
                  disabled={saving}
                >
                  {saving ? "Guardando..." : "Guardar cambios"}
                </button>
              </div>
            </form>

            {saveSuccess && (
              <p className="profile-success" role="status">
                {saveSuccess}
              </p>
            )}
            {saveError && (
              <p className="profile-error" role="alert">
                {saveError}
              </p>
            )}
          </section>

          {/* Errores globales */}
          {(authError || errorUsuario || errorRoles) && (
            <div className="profile-error" role="alert">
              <strong>Ocurrió un error:</strong>{" "}
              {authError || errorUsuario || errorRoles}
            </div>
          )}
        </section>
      </main>
    </>
  );
}
