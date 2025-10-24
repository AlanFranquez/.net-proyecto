import React from "react";
import { Link } from "react-router-dom";
import Navbar from "../components/Navbar";
import "../styles/Home.css";

export default function Home({ isLoggedIn, onToggle }) {
  return (
    <>
      <Navbar isLoggedIn={isLoggedIn} onToggle={onToggle} />

      <main className="home">
        <div className="backdrop" />

        <section className="hero-card">
          <div className="hand" aria-hidden>👋</div>
          <h1 className="title">
            Bienvenido a tu<br />Credencial Digital
          </h1>

          <p className="subtitle">
            Accede a todos tus servicios institucionales de forma segura
            y sencilla desde tu celular.
          </p>

          <div className="cta">
            <Link to="/login" className="btn primary">Iniciar sesión</Link>
            <a href="/registrarse" className="btn">Registrarse</a>
          </div>

          <ul className="features">
            <li>✅ Seguridad con biometría y NFC</li>
            <li>🌐 Disponible offline y online</li>
            <li>🏛️ Acceso a biblioteca, comedor, etc.</li>
          </ul>
        </section>
      </main>
    </>
  );
}
