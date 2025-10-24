import React from "react";
import { Link, useLocation } from "react-router-dom";
import "../styles/Navbar.css";

export default function Navbar({ isLoggedIn, onToggle }) {
  const { pathname } = useLocation();

  const authedMenu = [
    { label: "Accesos", to: "/accesos" },
    { label: "Credencial", to: "/credencial" },
    { label: "Beneficios", to: "/beneficios" },
    { label: "Canjes", to: "/canjes" },
    { label: "Autenticación", to: "/autenticacion" },
    { label: "Dispositivos", to: "/dispositivos" },
  ];

  // anon has no extra menu items
  const anonMenu = [];

  const menu = isLoggedIn ? authedMenu : anonMenu;

  return (
    <header className="nav">
      <div className="nav-inner">
        {/* Book -> Home */}
        <Link to="/" className="logo" aria-label="Inicio">
          📘
        </Link>

        {/* Menu */}
        <nav className="menu" aria-label="Principal">
          {menu.map((item) => (
            <MenuBtn
              key={item.to}
              to={item.to}
              active={pathname === item.to}
            >
              {item.label}
            </MenuBtn>
          ))}
        </nav>

        {/* Right side */}
        <div className="right">
          {!isLoggedIn ? (
            <Link to="/login" className="login-pill">Login</Link>
          ) : (
            <div className="icons" aria-hidden>
              <span className="bell">
                🔔<span className="badge">1</span>
              </span>
              {/* 👤 avatar links to profile */}
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
