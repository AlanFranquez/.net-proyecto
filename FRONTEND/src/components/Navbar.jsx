// src/components/Navbar.jsx
import React, { useState, useEffect, useRef } from "react";
import { Link, useLocation } from "react-router-dom";
import { useAuth } from "../services/AuthService.jsx";
import "../styles/Navbar.css";
import logo2 from "../assets/logo2.png";

export default function Navbar() {
  const { pathname } = useLocation();
  const { user, logout } = useAuth(); // <<--- AUTENTICACIÓN REAL
  const isLoggedIn = !!user;

  const [open, setOpen] = useState(false);
  const menuRef = useRef(null);

  useEffect(() => setOpen(false), [pathname]);

  useEffect(() => {
    function onKeyDown(e) {
      if (e.key === "Escape") setOpen(false);
    }
    document.addEventListener("keydown", onKeyDown);
    return () => document.removeEventListener("keydown", onKeyDown);
  }, []);

  const authedMenu = [
    { label: "Accesos", to: "/accesos" },
    { label: "Credencial", to: "/credencial" },
    { label: "Beneficios", to: "/beneficios" },
    { label: "Canjes", to: "/canjes" },
    { label: "Autenticación", to: "/autenticacion" },
    { label: "Dispositivos", to: "/dispositivos" },
  ];

  const menu = isLoggedIn ? authedMenu : [];

  return (
    <header className="nav">
      <div className="nav-inner">
        <Link to="/" className="logo">
          <img src={logo2} alt="Logo" className="logo-img" />
        </Link>

        <button
          className="hamburger"
          aria-expanded={open}
          onClick={() => setOpen((v) => !v)}
        >
          <span className="hamburger-box" aria-hidden />
        </button>

        <nav
          id="primary-menu"
          className={`menu ${open ? "is-open" : ""}`}
          aria-label="Principal"
          ref={menuRef}
        >
          {menu.map((item) => (
            <MenuBtn key={item.to} to={item.to} active={pathname === item.to}>
              {item.label}
            </MenuBtn>
          ))}

          <div className="menu-mobile-cta">
            {!isLoggedIn ? (
              <Link
                to="/login"
                className="login-pill"
                onClick={() => setOpen(false)}
              >
                Login
              </Link>
            ) : (
              <div className="icons">
                <span className="bell">
                  🔔<span className="badge">1</span>
                </span>
                <Link
                  to="/perfil"
                  className="avatar"
                  onClick={() => setOpen(false)}
                >
                  👤
                </Link>
                <button
                  className="logout-btn"
                  onClick={() => {
                    logout();
                    setOpen(false);
                  }}
                >
                  Salir
                </button>
              </div>
            )}
          </div>
        </nav>

        <div className="right">
          {!isLoggedIn ? (
            <Link to="/login" className="login-pill">
              Login
            </Link>
          ) : (
            <div className="icons">
              <span className="bell">
                🔔<span className="badge">1</span>
              </span>
              <Link to="/perfil" className="avatar">
                👤
              </Link>
              <button className="logout-btn" onClick={logout}>
                Salir
              </button>
            </div>
          )}
        </div>
      </div>
    </header>
  );
}

function MenuBtn({ to, active, children }) {
  return (
    <Link to={to} className={`menu-btn${active ? " is-active" : ""}`}>
      {children}
    </Link>
  );
}
