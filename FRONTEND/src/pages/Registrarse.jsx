import React, { useState } from "react";
import Navbar from "../components/Navbar";
import "../styles/Registrarse.css";

export default function Registrarse({ isLoggedIn, onToggle }) {
  const [form, setForm] = useState({
    nombre: "",
    email: "",
    password: "",
    confirm: "",
  });

  const handleChange = (e) => {
    setForm({ ...form, [e.target.name]: e.target.value });
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    alert("Registro enviado (placeholder)");
  };

  return (
    <>
      <Navbar isLoggedIn={isLoggedIn} onToggle={onToggle} />

      <main className="page">
        <section className="register-card">
          <h1 className="register-title">Registrarse</h1>

          <form onSubmit={handleSubmit} className="register-form">
            <label className="field">
              <span className="label">Nombre</span>
              <input
                type="text"
                name="nombre"
                value={form.nombre}
                onChange={handleChange}
                className="input"
                placeholder="Tu nombre"
                required
              />
            </label>

            <label className="field">
              <span className="label">Email</span>
              <input
                type="email"
                name="email"
                value={form.email}
                onChange={handleChange}
                className="input"
                placeholder="you@example.com"
                required
              />
            </label>

            <label className="field">
              <span className="label">Contraseña</span>
              <input
                type="password"
                name="password"
                value={form.password}
                onChange={handleChange}
                className="input"
                placeholder="••••••••"
                required
              />
            </label>

            <label className="field">
              <span className="label">Confirmar contraseña</span>
              <input
                type="password"
                name="confirm"
                value={form.confirm}
                onChange={handleChange}
                className="input"
                placeholder="••••••••"
                required
              />
            </label>

            <button type="submit" className="register-btn">
              Crear cuenta
            </button>
          </form>

          <p className="signin">
            ¿Ya tienes cuenta? <a href="/login">Inicia sesión.</a>
          </p>
        </section>
      </main>
    </>
  );
}
