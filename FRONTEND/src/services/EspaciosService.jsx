// src/services/EspaciosService.jsx
import { toApi } from "./AuthService.jsx";

async function safeJson(res) {
  const text = await res.text();
  try {
    return text ? JSON.parse(text) : null;
  } catch {
    return null;
  }
}

export async function fetchEspacios() {
  const res = await fetch(toApi("/espacios"), {
    method: "GET",
    credentials: "include",
    headers: { Accept: "application/json" },
  });
  if (!res.ok) {
    const err = await safeJson(res);
    throw new Error(err?.message || "No se pudieron cargar los espacios.");
  }
  const data = await res.json();
  return Array.isArray(data) ? data : [];
}

export async function fetchReglas() {
  const res = await fetch(toApi("/reglas"), {
    method: "GET",
    credentials: "include",
    headers: { Accept: "application/json" },
  });
  if (!res.ok) {
    const err = await safeJson(res);
    throw new Error(err?.message || "No se pudieron cargar las reglas.");
  }
  const data = await res.json();
  return Array.isArray(data) ? data : [];
}
