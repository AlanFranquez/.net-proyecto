import React from "react";
import Navbar from "../components/Navbar";
import "../styles/Perfil.css";

export default function Perfil({ isLoggedIn, onToggle }) {
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
              <h2 className="user-name">UsuarioNombreApellido</h2>
              <p className="cred-state">Estado de credencial</p>
            </div>

            <aside className="roles" aria-label="Roles">
              <span>Rol1</span>
              <span>Rol2</span>
              <span>Rol3</span>
            </aside>
          </div>

          {/* Datos */}
          <div className="data-box" role="region" aria-labelledby="datos-title">
            <h3 id="datos-title" className="data-title">Datos</h3>

            {/* grid semántica simple (labels/values) */}
            <div className="data-grid">
              <div className="k">Correo</div>
              <div className="v">usuario@ejemplo.edu</div>

              <div className="k">Teléfono</div>
              <div className="v">+598 99 999 999</div>

              <div className="k">Fecha de nacimiento</div>
              <div className="v">01/01/2000</div>

              <div className="k">Fecha de alta en el sistema</div>
              <div className="v">15/02/2024</div>

              <div className="k">Último acceso</div>
              <div className="v">31/10/2025 10:15</div>
            </div>
          </div>

          {/* Footer / acciones */}
          <div className="card-footer" role="toolbar" aria-label="Preferencias">
            <div className="theme">Tema claro</div>
            <button className="edit-btn" type="button">EDITAR</button>
          </div>
        </section>
      </main>
    </>
  );
}
