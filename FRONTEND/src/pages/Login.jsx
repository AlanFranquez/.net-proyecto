// src/pages/Login.jsx
import React, { useState } from "react";
import { useAuth } from "../services/AuthService.jsx";
import Navbar from "../components/Navbar";
import { Link, useNavigate } from "react-router-dom";
import "../styles/Login.css";

export default function Login({ isLoggedIn, onToggle }) {
  const [form, setForm] = useState({ email: "", password: "" });
  const [localError, setLocalError] = useState(null);

  const { login, error: authError } = useAuth();
  const navigate = useNavigate();

  const handleChange = (e) => {
    setForm({ ...form, [e.target.name]: e.target.value });
    setLocalError(null);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    const obj = {
      email: form.email,
      password: form.password,
    };

    try {
      const ok = await login(obj);
      if (!ok) {
        setLocalError("Error en el login");
        return;
      }

      setForm({ email: "", password: "" });
      navigate("/");
    } catch (err) {
      console.log(err);
      setLocalError(err.message || "Error en el login");
    }
  };

  const combinedError = localError || authError;

  return (
    <>
      <Navbar isLoggedIn={isLoggedIn} onToggle={onToggle} />

      <main className="page login-page">
        <div className="backdrop" />

        <section className="login-card">
          <h1 className="login-title">Login</h1>

          {combinedError && <p className="login-error">{combinedError}</p>}

          <form onSubmit={handleSubmit} className="login-form">
            <label className="field">
              <span className="label">Email</span>
              <input
                value={form.email}
                onChange={handleChange}
                name="email"
                type="email"
                className="input"
                placeholder="you@example.com"
                autoComplete="email"
                required
              />
            </label>

            <label className="field">
              <span className="label">Contraseña</span>
              <input
                type="password"
                value={form.password}
                onChange={handleChange}
                name="password"
                className="input"
                placeholder="••••••••"
                autoComplete="current-password"
                required
              />
            </label>

            <button type="submit" className="login-btn">
              Ingresar
            </button>
          </form>

          <p className="signup">
            ¿No tienes cuenta? <Link to="/registrarse">Crea una.</Link>
          </p>
        </section>
      </main>
    </>
  );
}
