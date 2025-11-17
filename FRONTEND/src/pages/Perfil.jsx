// src/pages/Perfil.jsx
import React from "react";
import Navbar from "../components/Navbar";
import { useAuth } from "../services/AuthService.jsx";
import "../styles/Perfil.css";

export default function Perfil({ isLoggedIn, onToggle }) {
  const { user, loading } = useAuth();

  if (loading) {
    return (
      <>
        <Navbar isLoggedIn={isLoggedIn} onToggle={onToggle} />
        <main className="profile-wrap">
          <p className="loading">Cargando perfil...</p>
        </main>
      </>
    );
  }

  if (!user) {
    return (
      <>
        <Navbar isLoggedIn={isLoggedIn} onToggle={onToggle} />
        <main className="profile-wrap">
          <p className="loading">Debes iniciar sesión para ver tu perfil.</p>
        </main>
      </>
    );
  }

  const {
    nombre,
    apellido,
    email,
    telefono,
    fechaNacimiento,
    fechaAlta,
    ultimoAcceso,
    roles = []
  } = user;

  return (
    <>
      <Navbar isLoggedIn={isLoggedIn} onToggle={onToggle} />

      <main className="profile-wrap">
        <section className="profile-card" aria-labelledby="profile-title">
          <h1 id="profile-title" className="sr-only">Perfil</h1>

          {/* Header */}
          <div className="profile-head">
            <div className="avatar" aria-hidden="true">
              <span role="img" aria-label="user">👤</span>
            </div>

            <div className="title-block">
              <h2 className="user-name">
                {nombre} {apellido}
              </h2>
              <p className="cred-state">Activo</p>
            </div>

            <aside className="roles" aria-label="Roles">
              {roles.length > 0 ? (
                roles.map((r) => <span key={r}>{r}</span>)
              ) : (
                <span>Sin roles</span>
              )}
            </aside>
          </div>

          {/* Datos */}
          <div className="data-box" role="region" aria-labelledby="datos-title">
            <h3 id="datos-title" className="data-title">Datos</h3>

            <div className="data-grid">
              <div className="k">Correo</div>
              <div className="v">{email}</div>

              <div className="k">Teléfono</div>
              <div className="v">{telefono || "—"}</div>

              <div className="k">Fecha de nacimiento</div>
              <div className="v">{fechaNacimiento || "—"}</div>

              <div className="k">Fecha de alta en el sistema</div>
              <div className="v">{fechaAlta || "—"}</div>

              <div className="k">Último acceso</div>
              <div className="v">{ultimoAcceso || "—"}</div>
            </div>
          </div>

          {/* Footer / acciones */}
          <div className="card-footer" role="toolbar" aria-label="Preferencias">
            <div className="theme">Tema claro</div>
            <button className="edit-btn" type="button">
              EDITAR
            </button>
          </div>
        </section>
      </main>
    </>
  );
}
