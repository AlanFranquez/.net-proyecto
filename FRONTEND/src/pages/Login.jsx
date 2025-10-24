import React from "react";
import Navbar from "../components/Navbar";
import "../styles/Login.css";

export default function Login({ isLoggedIn, onToggle }) {
  return (
    <>
      <Navbar isLoggedIn={isLoggedIn} onToggle={onToggle} />

      <main className="page">
        <section className="login-card">
          <h1 className="login-title">Login</h1>

          <form
            onSubmit={(e) => {
              e.preventDefault();
              alert("Placeholder login — no endpoints wired yet.");
            }}
            className="login-form"
          >
            <label className="field">
              <span className="label">Email</span>
              <input
                type="email"
                defaultValue="john@doe.com"
                className="input"
                placeholder="you@example.com"
              />
            </label>

            <label className="field">
              <span className="label">Contraseña</span>
              <input
                type="password"
                defaultValue="password"
                className="input"
                placeholder="••••••••"
              />
            </label>

            <button type="submit" className="login-btn">Ingresar</button>
          </form>

          <p className="signup">
            ¿No tienes cuenta? <a href="/registrarse">Crea una.</a>
          </p>
        </section>
      </main>
    </>
  );
}
