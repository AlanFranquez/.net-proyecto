import React, { useState } from "react";
import Navbar from "../components/Navbar";
import { useAuth, toApi } from "../services/AuthService.jsx";
import "../styles/Autenticacion.css";

export default function Autenticacion({ isLoggedIn, onToggle }) {
  const { user, loading: authLoading } = useAuth();

  const [busy, setBusy] = useState(false);
  const [message, setMessage] = useState(null);
  const [error, setError] = useState(null);

  // formulario de cambio de contraseña
  const [pwdForm, setPwdForm] = useState({
    current: "",
    next: "",
    confirm: "",
  });

  const loggedIn = !!user;

  const setStatus = (msg = null, err = null) => {
    setMessage(msg);
    setError(err);
  };

  // -------- Cambiar contraseña --------
  const handlePasswordChangeSubmit = async (e) => {
    e.preventDefault();
    setStatus();

    const currentPassword = pwdForm.current.trim();
    const newPassword = pwdForm.next.trim();
    const confirmPassword = pwdForm.confirm.trim();

    if (!currentPassword || !newPassword || !confirmPassword) {
      setStatus(null, "Completa todos los campos de contraseña.");
      return;
    }

    if (newPassword.length < 8) {
      setStatus(null, "La nueva contraseña debe tener al menos 8 caracteres.");
      return;
    }

    if (newPassword !== confirmPassword) {
      setStatus(null, "La nueva contraseña y la confirmación no coinciden.");
      return;
    }

    try {
      setBusy(true);

      const res = await fetch(toApi("/usuarios/cambiar-password"), {
        method: "POST",
        credentials: "include",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          currentPassword,
          newPassword,
        }),
      });

      if (!res.ok) {
        const txt = await res.text();
        throw new Error(
          txt || `Error al cambiar la contraseña: ${res.status}`
        );
      }

      setPwdForm({ current: "", next: "", confirm: "" });
      setStatus("Contraseña cambiada correctamente.", null);
      alert("Contraseña cambiada correctamente.");
    } catch (err) {
      console.error("Error al cambiar contraseña:", err);
      setStatus(
        null,
        err.message ||
          "No se pudo cambiar la contraseña. Verifica tus datos e intenta nuevamente."
      );
    } finally {
      setBusy(false);
    }
  };

  // -------- Vistas --------
  if (authLoading) {
    return (
      <>
        <Navbar isLoggedIn={isLoggedIn} onToggle={onToggle} />
        <main className="auth-wrap">
          <h1 className="auth-title">Autenticación</h1>
          <section className="auth-card">
            <p>Cargando usuario…</p>
          </section>
        </main>
      </>
    );
  }

  if (!loggedIn) {
    return (
      <>
        <Navbar isLoggedIn={false} onToggle={onToggle} />
        <main className="auth-wrap">
          <h1 className="auth-title">Autenticación</h1>
          <section className="auth-card">
            <p>Debes iniciar sesión para gestionar la autenticación.</p>
          </section>
        </main>
      </>
    );
  }

  return (
    <>
      <Navbar isLoggedIn={true} onToggle={onToggle} />

      <main className="auth-wrap">
        <h1 className="auth-title">Autenticación</h1>

        {/* Solo cambio de contraseña */}
        <section className="auth-card">
          <h2 className="auth-subtitle">Cambiar contraseña</h2>
          <form className="auth-form" onSubmit={handlePasswordChangeSubmit}>
            <label className="auth-label">
              Contraseña actual
              <input
                type="password"
                autoComplete="current-password"
                value={pwdForm.current}
                onChange={(e) =>
                  setPwdForm((f) => ({ ...f, current: e.target.value }))
                }
                disabled={busy}
              />
            </label>

            <label className="auth-label">
              Nueva contraseña
              <input
                type="password"
                autoComplete="new-password"
                value={pwdForm.next}
                onChange={(e) =>
                  setPwdForm((f) => ({ ...f, next: e.target.value }))
                }
                disabled={busy}
              />
            </label>

            <label className="auth-label">
              Confirmar nueva contraseña
              <input
                type="password"
                autoComplete="new-password"
                value={pwdForm.confirm}
                onChange={(e) =>
                  setPwdForm((f) => ({ ...f, confirm: e.target.value }))
                }
                disabled={busy}
              />
            </label>

            <button
              type="submit"
              className="auth-btn primary"
              disabled={busy}
            >
              Guardar contraseña
            </button>
          </form>
        </section>

        {(message || error) && (
          <section
            className="auth-status"
            aria-live="polite"
            style={{ marginTop: "16px" }}
          >
            {message && <p className="auth-msg">{message}</p>}
            {error && (
              <p className="auth-error">
                <strong>Error:</strong> {error}
              </p>
            )}
          </section>
        )}
      </main>
    </>
  );
}
