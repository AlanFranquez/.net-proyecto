import React from "react";
import Navbar from "../components/Navbar";
import "../styles/Autenticacion.css";

export default function Autenticacion({ isLoggedIn, onToggle }) {
  const click = (msg) => alert(`${msg} (placeholder)`);

  return (
    <>
      <Navbar isLoggedIn={isLoggedIn} onToggle={onToggle} />

      <main className="auth-wrap">
        <h1 className="auth-title">Autenticación</h1>

        <section className="auth-card">
          <button className="auth-btn" onClick={() => click("Cambiar contraseña")}>
            Cambiar contraseña
          </button>

          <button className="auth-btn" onClick={() => click("Eliminar datos de biometría")}>
            Eliminar datos de biometría
          </button>

          <button className="auth-btn" onClick={() => click("Modificar 2FA")}>
            Modificar 2FA
          </button>

          <button className="auth-btn danger" onClick={() => click("Eliminar cuenta")}>
            Eliminar cuenta
          </button>

          <button className="auth-btn" onClick={() => click("Eliminar credencial")}>
            Eliminar credencial
          </button>

          <button className="auth-btn" onClick={() => click("Administrar roles")}>
            Administrar roles
          </button>
        </section>
      </main>
    </>
  );
}
