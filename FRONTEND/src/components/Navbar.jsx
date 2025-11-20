// src/components/Navbar.jsx
import React, { useState, useEffect, useRef } from "react";
import { Link, useLocation } from "react-router-dom";
import { useAuth, toApi } from "../services/AuthService.jsx";
import "../styles/Navbar.css";
import logo2 from "../assets/logo2.png";

export default function Navbar() {
  const { pathname } = useLocation();
  const { user, logout } = useAuth();
  const isLoggedIn = !!user;

  const [open, setOpen] = useState(false);
  const menuRef = useRef(null);

  const [notifCount, setNotifCount] = useState(0);

  useEffect(() => setOpen(false), [pathname]);

  useEffect(() => {
    function onKeyDown(e) {
      if (e.key === "Escape") setOpen(false);
    }
    document.addEventListener("keydown", onKeyDown);
    return () => document.removeEventListener("keydown", onKeyDown);
  }, []);

  // --------- Cargar cantidad de notificaciones del usuario ----------
  useEffect(() => {
    if (!isLoggedIn) {
      setNotifCount(0);
      return;
    }

    let cancelled = false;

    const fetchNotifs = async () => {
      try {
        const usuarioId =
          user?.usuarioId ?? user?.UsuarioId ?? user?.id ?? null;

        const params = new URLSearchParams();
        params.set("onlyActive", "true");
        if (usuarioId) params.set("usuarioId", String(usuarioId));

        const res = await fetch(
          toApi(`/notificaciones/mias?${params.toString()}`),
          {
            method: "GET",
            credentials: "include",
            headers: { Accept: "application/json" },
          }
        );

        if (!res.ok) {
          if (!cancelled) setNotifCount(0);
          return;
        }

        const data = await res.json();
        const count = Array.isArray(data) ? data.length : 0;
        if (!cancelled) setNotifCount(count);
      } catch (err) {
        console.error("Error cargando notificaciones:", err);
        if (!cancelled) setNotifCount(0);
      }
    };

    fetchNotifs();

    return () => {
      cancelled = true;
    };
  }, [isLoggedIn, user?.usuarioId, user?.UsuarioId, user?.id]);

  const authedMenu = [
    { label: "Accesos", to: "/accesos" },
    { label: "Credencial", to: "/credencial" },
    { label: "Beneficios", to: "/beneficios" },
    { label: "Canjes", to: "/canjes" },
    { label: "Autenticación", to: "/autenticacion" },
    { label: "Dispositivos", to: "/dispositivos" },
    { label: "Novedades", to: "/novedades" },
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
                <Link
                  to="/notificaciones"
                  className="bell"
                  onClick={() => setOpen(false)}
                >
                  🔔
                  {notifCount > 0 && (
                    <span className="badge">{notifCount}</span>
                  )}
                </Link>
                <Link
                  to="/perfil"
                  className="avatar"
                  onClick={() => setOpen(false)}
                >
                  👤
                </Link>
                <button
                  className="logout-btn"
                  style={{ color: "black" }}
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
              <Link to="/notificaciones" className="bell">
                🔔
                {notifCount > 0 && (
                  <span className="badge">{notifCount}</span>
                )}
              </Link>
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
