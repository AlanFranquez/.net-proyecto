import React, { useState } from "react";
import Navbar from "../components/Navbar";
import "../styles/Beneficios.css";

export default function Beneficios({ isLoggedIn, onToggle }) {
  const [modal, setModal] = useState(null);

  const beneficios = [
    { id: 1, nombre: "Descuento en Comedor", estado: "disponible" },
    { id: 2, nombre: "Acceso a Biblioteca", estado: "obtenido" },
    { id: 3, nombre: "Descuento en Libros", estado: "disponible" },
    { id: 4, nombre: "Entrada a Eventos", estado: "disponible" },
  ];

  return (
    <>
      <Navbar isLoggedIn={isLoggedIn} onToggle={onToggle} />
      <div className="beneficios-container">
        <h2>Mis Beneficios</h2>
        <div className="beneficios-list">
          {beneficios.map((b) => (
            <div key={b.id} className="beneficio-item">
              <span>{b.nombre}</span>
              {b.estado === "obtenido" ? (
                <button className="btn-disabled">Obtenido</button>
              ) : (
                <button className="btn-obtener" onClick={() => setModal(b)}>
                  Obtener
                </button>
              )}
            </div>
          ))}
        </div>
      </div>

      {modal && (
        <div className="modal-overlay" onClick={() => setModal(null)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <h3>Petición de Beneficio</h3>
            <p><strong>Beneficio:</strong> {modal.nombre}</p>
            <p><strong>Vigencia:</strong> 01/10/2025 – 31/10/2025</p>
            <p><strong>Cupos:</strong> 3/5 disponibles</p>
            <p><strong>Costo:</strong> $0 (cubierto por UTEC)</p>
            <p><strong>Estado:</strong> Disponible</p>
            <div className="modal-actions">
              <button className="btn-cancelar" onClick={() => setModal(null)}>
                Cancelar
              </button>
              <button className="btn-solicitar" onClick={() => setModal(null)}>
                Solicitar
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}
