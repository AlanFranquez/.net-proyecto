import React, { useState } from "react";
import Navbar from "../components/Navbar";
import "../styles/Dispositivos.css";

export default function Dispositivos({ isLoggedIn, onToggle }) {
  const [devices, setDevices] = useState([
    {
      id: "123456",
      name: "Laptop",
      expires: "2 de mayo de 2024",
      type: "laptop",
      active: true,
    },
    {
      id: "789012",
      name: "Smartphone",
      expires: "15 de marzo de 2024",
      type: "phone",
      active: true,
    },
  ]);

  const [confirming, setConfirming] = useState(null); // device id

  const iconFor = (type) => {
    if (type === "laptop") return "💻";
    if (type === "phone") return "📱";
    return "🔧";
  };

  const darDeBaja = (id) => {
    setDevices((prev) =>
      prev.map((d) => (d.id === id ? { ...d, active: false } : d))
    );
    setConfirming(null);
  };

  return (
    <>
      <Navbar isLoggedIn={isLoggedIn} onToggle={onToggle} />

      <main className="disp-wrap">
        <section className="disp-card">
          <h1 className="disp-title">Dispositivos conectados</h1>

          <ul className="disp-list">
            {devices.map((d, idx) => (
              <li key={d.id} className={`disp-row ${idx ? "with-top" : ""}`}>
                <div className="left">
                  <div className="dev-icon" aria-hidden>
                    {iconFor(d.type)}
                  </div>
                  <div className="dev-texts">
                    <div className="dev-name">{d.name}</div>
                    <div className="dev-meta">
                      ID de dispositivo interno: {d.id}
                    </div>
                    <div className="dev-meta">Expira el {d.expires}</div>
                  </div>
                </div>

                <div className="right">
                  {d.active ? (
                    <button
                      className="btn-baja"
                      onClick={() => setConfirming(d.id)}
                    >
                      Dar de baja
                    </button>
                  ) : (
                    <span className="estado-baja">Dado de baja</span>
                  )}
                </div>
              </li>
            ))}
          </ul>
        </section>
      </main>

      {confirming && (
        <div className="disp-modal-overlay" onClick={() => setConfirming(null)}>
          <div className="disp-modal" onClick={(e) => e.stopPropagation()}>
            <h3>Confirmar baja</h3>
            <p>
              ¿Seguro que deseas dar de baja el dispositivo con ID{" "}
              <strong>{confirming}</strong>?
            </p>
            <div className="modal-actions">
              <button className="btn-cancel" onClick={() => setConfirming(null)}>
                Cancelar
              </button>
              <button className="btn-confirm" onClick={() => darDeBaja(confirming)}>
                Confirmar
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}
