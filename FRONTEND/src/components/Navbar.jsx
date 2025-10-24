import React, { useState, useEffect, useRef } from "react";
import { Link, useLocation } from "react-router-dom";
import "../styles/Navbar.css";
import logo2 from "../assets/logo2.png";

export default function Navbar({ isLoggedIn, onToggle }) {
  const { pathname } = useLocation();
  const [open, setOpen] = useState(false);
  const menuRef = useRef(null);

  // Close the mobile menu when the route changes
  useEffect(() => setOpen(false), [pathname]);

  // Close on Escape
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

  const anonMenu = [];
  const menu = isLoggedIn ? authedMenu : anonMenu;

  return (
    <header className="nav">
      <div className="nav-inner">
        {/* Logo -> Home */}
        <Link to="/" className="logo" aria-label="Inicio">
          <img src={logo2} alt="Logo" className="logo-img" />
        </Link>

        {/* Mobile hamburger */}
        <button
          className="hamburger"
          aria-label={open ? "Cerrar menú" : "Abrir menú"}
          aria-expanded={open}
          aria-controls="primary-menu"
          onClick={() => setOpen((v) => !v)}
        >
          <span className="hamburger-box" aria-hidden />
        </button>

        {/* Menu */}
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

          {/* Optional: put auth controls inside the slideout on small screens */}
          <div className="menu-mobile-cta">
            {!isLoggedIn ? (
              <Link to="/login" className="login-pill" onClick={() => setOpen(false)}>
                Login
              </Link>
            ) : (
              <div className="icons" aria-hidden>
                <span className="bell">
                  🔔<span className="badge">1</span>
                </span>
                <Link to="/perfil" className="avatar" onClick={() => setOpen(false)}>
                  👤
                </Link>
              </div>
            )}
          </div>
        </nav>

        {/* Right side (shown on desktop) */}
        <div className="right">
          {!isLoggedIn ? (
            <Link to="/login" className="login-pill">Login</Link>
          ) : (
            <div className="icons" aria-hidden>
              <span className="bell">
                🔔<span className="badge">1</span>
              </span>
              <Link to="/perfil" className="avatar">👤</Link>
            </div>
          )}
          <button className="toggle" onClick={onToggle}>Toggle User</button>
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
