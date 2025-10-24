import React, { useState } from "react";
import Navbar from "../components/Navbar";
import "../styles/Credencial.css";

export default function Credencial({ isLoggedIn, onToggle }) {
  const [qrSeed, setQrSeed] = useState(Date.now()); // para “recargar” el QR
  const [estado, setEstado] = useState("Activa");

  const recargarQR = () => setQrSeed(Date.now());
  const validarBiometria = () => alert("Validación biométrica (placeholder)");
  const renovar = () => {
    setEstado("Renovada");
    alert("Credencial renovada (placeholder)");
  };

  return (
    <>
      <Navbar isLoggedIn={isLoggedIn} onToggle={onToggle} />

      <main className="cred-wrap">
        <section className="cred-card" aria-labelledby="cred-title">
          {/* Título visible para lectores y anclas */}
          <h1 id="cred-title" className="sr-only">Credencial</h1>

          {/* panel principal */}
          <div className="cred-top">
            <div className="cred-info" role="table" aria-label="Información de la credencial">
              <div className="cred-row" role="row">
                <span className="k" role="rowheader">Credencial</span>
                <span className="v" role="cell">ID-000123</span>
              </div>
              <div className="cred-row" role="row">
                <span className="k" role="rowheader">Usuario</span>
                <span className="v" role="cell">UsuarioNombre</span>
              </div>
              <div className="cred-row" role="row">
                <span className="k" role="rowheader">Roles</span>
                <span className="v" role="cell">Estudiante, Biblioteca</span>
              </div>
              <div className="cred-row" role="row">
                <span className="k" role="rowheader">Tipo</span>
                <span className="v" role="cell">Digital</span>
              </div>
              <div className="cred-row" role="row">
                <span className="k" role="rowheader">Estado</span>
                <span className={`badge ${estado === "Activa" ? "on" : ""}`} role="cell">
                  {estado}
                </span>
              </div>
              <div className="cred-row" role="row">
                <span className="k" role="rowheader">Fecha emisión</span>
                <span className="v" role="cell">01/05/2025</span>
              </div>
              <div className="cred-row" role="row">
                <span className="k" role="rowheader">Fecha expiración</span>
                <span className="v" role="cell">01/05/2026</span>
              </div>
              <div className="cred-row" role="row">
                <span className="k" role="rowheader">NFC</span>
                <span className="v" role="cell">Disponible</span>
              </div>
              <div className="cred-row" role="row">
                <span className="k" role="rowheader">ID</span>
                <span className="v mono" role="cell">iddeejemplo123</span>
              </div>
            </div>

            {/* QR */}
            <div className="qr-box" aria-label="Código QR">
              <div className="qr" key={qrSeed} aria-hidden>
                {[...Array(25)].map((_, i) => (
                  <span key={i} className={Math.random() > 0.5 ? "b" : ""} />
                ))}
              </div>
              <div className="qr-caption">Código QR</div>
            </div>
          </div>

          {/* secciones inferiores */}
          <div className="cred-grid">
            <section className="panel" aria-labelledby="ultimos-accesos">
              <h3 id="ultimos-accesos">Últimos accesos</h3>
              <ul className="list">
                <li>31/10/2025 – Biblioteca Central (NFC)</li>
                <li>30/10/2025 – Comedor Universitario (QR)</li>
                <li>29/10/2025 – Laboratorio 2 (NFC)</li>
              </ul>
            </section>

            <section className="panel" aria-labelledby="beneficios-activos">
              <h3 id="beneficios-activos">Beneficios Activos</h3>
              <ul className="list">
                <li>Descuento en Comedor</li>
                <li>Acceso a Biblioteca</li>
              </ul>
            </section>
          </div>

          {/* acciones */}
          <div className="cred-actions" role="toolbar" aria-label="Acciones de credencial">
            <button type="button" className="btn ghost" onClick={recargarQR}>
              Recargar QR
            </button>
            <button type="button" className="btn" onClick={renovar}>
              Renovar
            </button>
            <button type="button" className="btn ghost" onClick={validarBiometria}>
              Validar Biometría
            </button>
          </div>
        </section>
      </main>
    </>
  );
}
