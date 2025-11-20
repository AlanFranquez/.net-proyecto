// src/pages/Home.jsx
import React from "react";
import { Link } from "react-router-dom";
import Navbar from "../components/Navbar";
import { useAuth } from "../services/AuthService.jsx"; // <<< igual que en Navbar
import "../styles/Home.css";

export default function Home() {
  const { user } = useAuth();
  const isLoggedIn = !!user;

  // Mejor intento de nombre para mostrar
  const displayName =
    user?.nombre ||
    user?.name ||
    user?.firstName ||
    user?.username ||
    user?.email ||
    "";

  return (
    <>
      <Navbar />

      <main
        className={`home ${isLoggedIn ? "home-logged" : "home-guest"}`}
        style={{
          display: "flex",
          justifyContent: "center",
          alignItems: "center",
          textAlign: "center",
        }}
      >
        <div className="backdrop" />

        <section
          className="hero-card"
          style={{ maxWidth: "600px", width: "100%", margin: "0 auto" }}
        >
          <div className="hand" aria-hidden></div>

          {/* TÍTULO CAMBIA SI ESTÁ LOGUEADA */}
          <h1 className="title">
            {isLoggedIn ? (
              <>
                Bienvenido
                {displayName ? (
                  <>
                    , <span className="username-highlight">{displayName}</span>
                  </>
                ) : null}
                <br />a tu Credencial Digital
              </>
            ) : (
              <>
                Bienvenido a tu
                <br />
                Credencial Digital
              </>
            )}
          </h1>

          {/* SUBTÍTULO (puede ser igual para ambos o levemente distinto) */}

          {/* CTA: si NO está logueado, mostrar Login/Registro.
              Si SÍ está logueado, mostrar accesos directos. */}
          <div
            className="cta"
            style={{
              display: "flex",
              flexDirection: "column",
              gap: "12px",
              alignItems: "center",
            }}
          >
            {!isLoggedIn ? (
              <>      <ul
            className="features"
            style={{ listStyle: "none", padding: 0, marginTop: "20px" }}
          ></ul><p>Seguridad con biometría y NFC - Disponible offline y online -
                  Acceso a biblioteca, comedor, etc.
                </p>
                <p>
                  Accede a todos tus servicios institucionales de forma segura y
                  sencilla desde tu celular. </p>
                        <ul
            className="features"
            style={{ listStyle: "none", padding: 0, marginTop: "20px" }}
          ></ul>

                <Link to="/login" className="btn primary">
                  Iniciar sesión
                </Link>
                <a href="/registrarse" className="btn">
                  Registrarse
                </a>    

              </>
            ) : (
              <>      <ul
            className="features"
            style={{ listStyle: "none", padding: 0, marginTop: "20px" }}
          ></ul>
                <Link to="/credencial" className="btn primary">
                  Ver mi credencial
                </Link>
                <Link to="/novedades" className="btn">
                  Ver novedades
                </Link>
                <Link to="/accesos" className="btn">
                  Ver accesos
                </Link>
                <Link to="/perfil" className="btn">
                  Editar perfil
                </Link>
              </>
            )}
          </div>


        </section>
      </main>
    </>
  );
}
