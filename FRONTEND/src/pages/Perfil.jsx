// src/pages/Perfil.jsx
import React from "react";
import Navbar from "../components/Navbar";
import { useAuth } from "../services/AuthService.jsx";
import "../styles/Perfil.css";

export default function Perfil({ isLoggedIn, onToggle }) {
  const { user, loading, error, refetchUser } = useAuth();

  const loggedIn = !!user;

  // Hack sencillo para ver qué está llegando desde /me
  // Abre la consola del navegador para verlo.
  if (user) {
    // eslint-disable-next-line no-console
    console.log("Usuario desde /api/usuarios/me:", user);
  }

  if (loading) {
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

  const {
    usuarioId,
    nombre,
    apellido,
    email,
    documento,
    estado,
    roles,
    usuarioRoles,
  } = user;

  // Normalizar roles:
  // - si el DTO tiene `roles: [{ id, nombre }]`
  // - o si tiene `usuarioRoles: [{ rolId, rolNombre }]`
  const rolesNormalizados =
    roles?.map((r) => r.nombre ?? r.name ?? r.descripcion) ??
    usuarioRoles?.map((ur) => ur.nombre ?? ur.rolNombre) ??
    [];

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
                {email} · <span className="badge badge--state">{estado}</span>
              </p>
            </div>

            <aside className="roles" aria-label="Roles">
              <h2 className="roles-title">Roles</h2>
              {rolesNormalizados.length > 0 ? (
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

          {/* Sección datos de cuenta */}
          <section
            className="data-box"
            role="region"
            aria-labelledby="datos-title"
          >
            <div className="data-box-header">
              <h2 id="datos-title" className="data-title">
                Datos de la cuenta
              </h2>
              <button
                type="button"
                className="ghost-btn"
                onClick={() => refetchUser?.()}
                title="Recargar datos desde el servidor"
              >
                Recargar
              </button>
            </div>

            <dl className="data-grid">
              <div className="data-row">
                <dt className="k">Nombre</dt>
                <dd className="v">
                  {nombre} {apellido}
                </dd>
              </div>

              <div className="data-row">
                <dt className="k">Correo</dt>
                <dd className="v">{email}</dd>
              </div>

              <div className="data-row">
                <dt className="k">Documento</dt>
                <dd className="v">{documento || "No registrado"}</dd>
              </div>

              <div className="data-row">
                <dt className="k">Estado</dt>
                <dd className="v">
                  <span className="badge badge--state">{estado}</span>
                </dd>
              </div>

              <div className="data-row">
                <dt className="k">ID de usuario</dt>
                <dd className="v code">{usuarioId}</dd>
              </div>
            </dl>
          </section>

          {/* Sección futura: preferencias / seguridad */}
          <section
            className="data-box"
            role="region"
            aria-labelledby="prefs-title"
          >
            <div className="data-box-header">
              <h2 id="prefs-title" className="data-title">
                Preferencias &amp; seguridad
              </h2>
            </div>

            <div className="prefs-grid">
              <div className="prefs-item">
                <h3>Tema</h3>
                <p>Usando el tema actual de la aplicación.</p>
              </div>

              <div className="prefs-item">
                <h3>Contraseña</h3>
                <p>
                  La contraseña se gestiona en Cognito. Implementa aquí un
                  enlace a “Cambiar contraseña” si lo necesitas.
                </p>
              </div>
            </div>
          </section>

          {/* Errores */}
          {error && (
            <div className="profile-error" role="alert">
              <strong>Ocurrió un error:</strong> {error}
            </div>
          )}
        </section>
      </main>
    </>
  );
}
