import React, { useState } from "react";
import { useAuth } from "../services/AuthService.jsx";
import Navbar from "../components/Navbar";
import { Link, useNavigate } from 'react-router-dom';
import "../styles/Login.css";

export default function Login({ isLoggedIn, onToggle }) {
  const [form, setForm] = useState({
    email: "",
    password: ""
  });
  const { login, error } = useAuth()
  const navigate = useNavigate()
  
  const handleChange = (e) => {
    setForm({ ...form, [e.target.name]: e.target.value });
  };
    const handleSubmit = async (e) => {
    e.preventDefault();
    // setLoading(true);
    //setError("");
    // setSuccess("");

    const obj = {
      "Email": form.email,
      "Password": form.password
    }

    console.log("Registrando usuario:", obj);

    try {
      const ok = await login("http://localhost:8080/api/usuarios/login", obj)
      if(!ok){
        throw new Error(ok || "Error en el login");
      }
      // setSuccess("Usuario registrado con éxito ✅");
      setForm({ 
        email: "",
        password: ""
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
        <section className="login-card">
          <h1 className="login-title">Login</h1>

          <form
            onSubmit={handleSubmit}
            className="login-form"
          >
            <label className="field">
              <span className="label">Email</span>
              <input
                value={form.email}
                onChange={handleChange}
                name="email"
                type="email"
                className="input"
                placeholder="you@example.com"
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
