import React from "react";
import Navbar from "../components/Navbar";
import "../styles/Perfil.css";

export default function Perfil({ isLoggedIn, onToggle }) {
  return (
    <>
      <Navbar isLoggedIn={isLoggedIn} onToggle={onToggle} />
      <main className="profile-wrap">
        <section className="profile-card">
          <div className="profile-head">
            <div className="avatar">
              <span role="img" aria-label="user">👤</span>
            </div>
            <div className="title-block">
              <h1 className="user-name">UsuarioNombreApellido</h1>
              <p className="cred-state">Estado de credencial</p>
            </div>
            <aside className="roles">
              <span>Rol1</span>
              <span>Rol2</span>
              <span>Rol3</span>
            </aside>
          </div>
          <div className="data-box">
            <h2 className="data-title">Datos</h2>
            <div className="data-list">
              <div>Correo</div>
              <div>Teléfono</div>
              <div>Fecha de nacimiento</div>
              <div>Fecha de alta en el sistema</div>
              <div>Último acceso</div>
            </div>
          </div>
          <div className="card-footer">
            <div className="theme">Tema claro</div>
            <button className="edit-btn">EDITAR</button>
          </div>
        </section>
      </main>
    </>
  );
}
