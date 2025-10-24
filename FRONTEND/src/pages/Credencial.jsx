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
        <section className="cred-card">
          {/* panel principal */}
          <div className="cred-top">
            <div className="cred-info">
              <div className="cred-row">
                <span className="k">Credencial</span>
                <span className="v">ID-000123</span>
              </div>
              <div className="cred-row">
                <span className="k">Usuario</span>
                <span className="v">UsuarioNombre</span>
              </div>
              <div className="cred-row">
                <span className="k">Roles</span>
                <span className="v">Estudiante, Biblioteca</span>
              </div>
              <div className="cred-row">
                <span className="k">Tipo</span>
                <span className="v">Digital</span>
              </div>
              <div className="cred-row">
                <span className="k">Estado</span>
                <span className={`badge ${estado === "Activa" ? "on" : ""}`}>{estado}</span>
              </div>
              <div className="cred-row">
                <span className="k">Fecha emisión</span>
                <span className="v">01/05/2025</span>
              </div>
              <div className="cred-row">
                <span className="k">Fecha expiración</span>
                <span className="v">01/05/2026</span>
              </div>
              <div className="cred-row">
                <span className="k">NFC</span>
                <span className="v">Disponible</span>
              </div>
              <div className="cred-row">
                <span className="k">ID</span>
                <span className="v mono">iddeejemplo123</span>
              </div>
            </div>

            {/* QR placeholder */}
            <div className="qr-box" aria-label="Código QR">
              <div className="qr" key={qrSeed}>
                {/* placeholder geométrico simple */}
                {[...Array(25)].map((_, i) => (
                  <span key={i} className={Math.random() > 0.5 ? "b" : ""} />
                ))}
              </div>
              <div className="qr-caption">Código QR</div>
            </div>
          </div>

          {/* secciones inferiores */}
          <div className="cred-grid">
            <div className="panel">
              <h3>Últimos accesos</h3>
              <ul className="list">
                <li>31/10/2025 – Biblioteca Central (NFC)</li>
                <li>30/10/2025 – Comedor Universitario (QR)</li>
                <li>29/10/2025 – Laboratorio 2 (NFC)</li>
              </ul>
            </div>

            <div className="panel">
              <h3>Beneficios Activos</h3>
              <ul className="list">
                <li>Descuento en Comedor</li>
                <li>Acceso a Biblioteca</li>
              </ul>
            </div>
          </div>

          {/* acciones */}
          <div className="cred-actions">
            <button className="btn ghost" onClick={recargarQR}>Recargar QR</button>
            <button className="btn" onClick={renovar}>Renovar</button>
            <button className="btn ghost" onClick={validarBiometria}>Validar Biometría</button>
          </div>
        </section>
      </main>
    </>
  );
}
