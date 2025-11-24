// src/pages/Registrarse.jsx
import React, { useState } from "react";
import { useAuth } from "../services/AuthService.jsx";
import { Link, useNavigate } from "react-router-dom";
import Navbar from "../components/Navbar";
import "../styles/Registrarse.css";
import "../styles/Login.css";


export default function Registrarse({ isLoggedIn, onToggle }) {
  const [form, setForm] = useState({
    nombre: "",
    apellido: "",
    documento: "",
    email: "",
    password: "",
    confirm: "",
  });

  const [localError, setLocalError] = useState(null);

  const { register, error: authError } = useAuth();
  const navigate = useNavigate();

  const handleChange = (e) => {
    setForm({ ...form, [e.target.name]: e.target.value });
    setLocalError(null);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (form.password !== form.confirm) {
      setLocalError("Las contraseñas no coinciden");
      return;
    }

    const obj = {
      nombre: form.nombre,
      apellido: form.apellido,
      documento: form.documento,
      email: form.email,
      password: form.password,
    };

    console.log("Registrando usuario:", obj);

    try {
      const ok = await register(obj);
      if (!ok) {
        setLocalError("Error en el registro");
        return;
      }

      setForm({
        nombre: "",
        apellido: "",
        documento: "",
        email: "",
        password: "",
        confirm: "",
      });

      navigate("/");
    } catch (err) {
      console.log(err);
      setLocalError(err.message || "Error en el registro");
    }
  };

  const combinedError = localError || authError;

  return (
    <>
      <Navbar isLoggedIn={isLoggedIn} onToggle={onToggle} />

      <main className="page login-page">
        <section className="register-card">
          <h1 className="register-title">Registrarse</h1>

          {combinedError && (
            <p className="register-error">{combinedError}</p>
          )}

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
                autoComplete="name"
                required
              />
            </label>

            <label className="field">
              <span className="label">Apellido</span>
              <input
                name="apellido"
                value={form.apellido}
                onChange={handleChange}
                className="input"
                placeholder="Tu apellido"
                autoComplete="family-name"
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
                autoComplete="email"
                required
              />
            </label>

            <label className="field">
              <span className="label">Documento</span>
              <input
                name="documento"
                value={form.documento}
                onChange={handleChange}
                className="input"
                placeholder="Tu CI"
                autoComplete="off"
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
                autoComplete="new-password"
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
                autoComplete="new-password"
                required
              />
            </label>

            <button type="submit" className="register-btn">
              Crear cuenta
            </button>
          </form>

          <p className="signin">
            ¿Ya tienes cuenta? <Link to="/login">Inicia sesión.</Link>
          </p>
        </section>
      </main>
    </>
  );
}
