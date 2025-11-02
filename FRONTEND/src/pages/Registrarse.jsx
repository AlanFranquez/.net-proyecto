import React, { useState } from "react";
import { useAuth } from "../services/AuthService.jsx";
import { Link, useNavigate } from 'react-router-dom';
import Navbar from "../components/Navbar";
import "../styles/Registrarse.css";

export default function Registrarse({ isLoggedIn, onToggle }) {
  const [form, setForm] = useState({
    nombre: "",
    apellido: "",
    documento: "",
    email: "",
    password: "",
    confirm: ""
  });
  const { register, error } = useAuth()
    const navigate = useNavigate()

  const handleChange = (e) => {
    setForm({ ...form, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    // setLoading(true);
    //setError("");
    // setSuccess("");

    if(form.password != form.confirm){
      throw new Error("las contraseñas no coinciden")
    }

    const obj = {
      "Nombre": form.nombre, 
      "Apellido": form.apellido,
      "Documento": form.documento,
      "Email": form.email,
      "Password": form.password
    }

    console.log("Registrando usuario:", obj);

    try {
      await fetch("http://localhost:8080/api/usuarios")
      const ok = await register("http://localhost:8080/api/usuarios/registro", obj)
      if(!ok){
        throw new Error(ok || "Error en el registro");
      }
      // setSuccess("Usuario registrado con éxito ✅");
      setForm({ 
        nombre: "",
        apellido: "",
        documento: "",
        email: "",
        password: "",
        confirm: ""
      });
      navigate("/");
    } catch (err) {
      console.log(err);
      //setError(err.message);
    } finally {
      // setLoading(false);
    }
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
              <span className="label">Apelldio</span>
              <input
                name="apellido"
                value={form.apellido}
                onChange={handleChange}
                className="input"
                placeholder="Tu Aplellido"
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
              <span className="label">Documento</span>
              <input
                name="documento"
                value={form.documento}
                onChange={handleChange}
                className="input"
                placeholder="Tu CI"
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
