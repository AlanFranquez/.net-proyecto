// src/pages/TestApi.jsx
import { useState } from "react";
import "./../styles/TestApi.css"; // opcional, si después querés estilos

const DEFAULT_BASE_URL = "http://localhost:8080";

const ENDPOINTS = [
  // Sin auth / simples
  { name: "Health (/health)", method: "GET", path: "/health" },
  { name: "Swagger JSON (/swagger/v1/swagger.json)", method: "GET", path: "/swagger/v1/swagger.json" },

  // API principales
  { name: "Usuarios (GET /api/usuarios)", method: "GET", path: "/api/usuarios" },
  { name: "Espacios (GET /api/espacios)", method: "GET", path: "/api/espacios" },
  { name: "Beneficios (GET /api/beneficios)", method: "GET", path: "/api/beneficios" },
  { name: "Canjes (GET /api/canjes)", method: "GET", path: "/api/canjes" },
  { name: "Eventos Accesos (GET /api/eventosaccesos)", method: "GET", path: "/api/eventosaccesos" },
  { name: "Notificaciones (GET /api/notificaciones)", method: "GET", path: "/api/notificaciones" },
  { name: "Credenciales (GET /api/credenciales)", method: "GET", path: "/api/credenciales" },
  { name: "Roles (GET /api/roles)", method: "GET", path: "/api/roles" },
  { name: "Sincronizaciones (GET /api/sincronizacion)", method: "GET", path: "/api/sincronizacion" },
  { name: "Dispositivos (GET /api/dispositivos)", method: "GET", path: "/api/dispositivos" },
];

export default function TestApi() {
  const [baseUrl, setBaseUrl] = useState(DEFAULT_BASE_URL);
  const [token, setToken] = useState(""); // JWT de Cognito (opcional)
  const [loading, setLoading] = useState(false);
  const [selectedEndpoint, setSelectedEndpoint] = useState(null);
  const [responseText, setResponseText] = useState("");
  const [status, setStatus] = useState(null);
  const [error, setError] = useState("");

  const callEndpoint = async (ep) => {
    setSelectedEndpoint(ep);
    setLoading(true);
    setResponseText("");
    setError("");
    setStatus(null);

    try {
      const url = baseUrl.replace(/\/$/, "") + ep.path;

      const headers = {
        "Accept": "application/json",
      };

      if (token.trim()) {
        headers["Authorization"] = `Bearer ${token.trim()}`;
      }

      const res = await fetch(url, {
        method: ep.method,
        headers,
      });

      setStatus(`${res.status} ${res.statusText}`);

      const text = await res.text();
      // Intentar parsear como JSON para mostrar bonito
      try {
        const json = JSON.parse(text);
        setResponseText(JSON.stringify(json, null, 2));
      } catch {
        setResponseText(text || "<sin contenido>");
      }
    } catch (err) {
      setError(err.message || String(err));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="testapi-container">
      <h1>Test API Espectáculos</h1>

      <div className="testapi-config">
        <div className="testapi-field">
          <label>Base URL del backend</label>
          <input
            type="text"
            value={baseUrl}
            onChange={(e) => setBaseUrl(e.target.value)}
            placeholder="http://localhost:8080"
          />
        </div>

        <div className="testapi-field">
          <label>JWT (Bearer token) de Cognito (opcional)</label>
          <textarea
            rows={3}
            value={token}
            onChange={(e) => setToken(e.target.value)}
            placeholder="Pega aquí tu token si el endpoint requiere auth"
          />
        </div>
      </div>

      <h2>Endpoints</h2>
      <div className="testapi-endpoints">
        {ENDPOINTS.map((ep) => (
          <button
            key={ep.name}
            className={
              "testapi-endpoint-btn" +
              (selectedEndpoint?.name === ep.name ? " selected" : "")
            }
            onClick={() => callEndpoint(ep)}
            disabled={loading}
          >
            {ep.method} {ep.path}
          </button>
        ))}
      </div>

      <div className="testapi-result">
        <h2>Resultado</h2>
        {selectedEndpoint && (
          <p>
            <strong>Endpoint:</strong> {selectedEndpoint.method}{" "}
            {selectedEndpoint.path}
          </p>
        )}
        {status && (
          <p>
            <strong>Status:</strong> {status}
          </p>
        )}
        {loading && <p>Llamando al endpoint...</p>}
        {error && (
          <pre className="testapi-error">
            Error: {error}
          </pre>
        )}
        {responseText && !error && (
          <pre className="testapi-response">
            {responseText}
          </pre>
        )}
      </div>
    </div>
  );
}
