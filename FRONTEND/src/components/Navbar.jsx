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
    { label: "Novedades", to: "/novedades" },
    { label: "Accesos", to: "/accesos" },
    { label: "Espacios", to: "/espacios" },
    { label: "Beneficios", to: "/beneficios" },
    { label: "Canjes", to: "/canjes" },
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

          {/* Mobile CTA */}
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
                  aria-label="Notificaciones"
                  onClick={() => setOpen(false)}
                >
                  <BellIcon />
                  {notifCount > 0 && (
                    <span className="badge">{notifCount}</span>
                  )}
                </Link>

                <Link
                  to="/perfil"
                  className="avatar"
                  aria-label="Perfil"
                  onClick={() => setOpen(false)}
                >
                  <UserIcon />
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

        {/* Desktop right cluster */}
        <div className="right">
          {!isLoggedIn ? (
            <Link to="/login" className="login-pill">
              Login
            </Link>
          ) : (
            <div className="icons">
              <Link to="/notificaciones" className="bell" aria-label="Notificaciones">
                <BellIcon />
                {notifCount > 0 && <span className="badge">{notifCount}</span>}
              </Link>

              <Link to="/perfil" className="avatar" aria-label="Perfil">
                <UserIcon />
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

/* ---------- Icons (black/white placeholders) ---------- */

function BellIcon() {
  return (
    <svg
      className="icon-svg"
      viewBox="0 0 24 24"
      aria-hidden="true"
      role="img"
    >
      <path
        d="M15 17H9m6-8a3 3 0 00-6 0c0 3-1.5 4.5-2.5 5.5h11C16.5 13.5 15 12 15 9z"
        fill="none"
        stroke="currentColor"
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
      <path
        d="M10 17a2 2 0 004 0"
        fill="none"
        stroke="currentColor"
        strokeWidth="2"
        strokeLinecap="round"
      />
    </svg>
  );
}

function UserIcon() {
  return (
    <svg
      className="icon-svg"
      viewBox="0 0 24 24"
      aria-hidden="true"
      role="img"
    >
      <circle
        cx="12"
        cy="8"
        r="4"
        fill="none"
        stroke="currentColor"
        strokeWidth="2"
      />
      <path
        d="M4 20c2.5-4 6-6 8-6s5.5 2 8 6"
        fill="none"
        stroke="currentColor"
        strokeWidth="2"
        strokeLinecap="round"
      />
    </svg>
  );
}
