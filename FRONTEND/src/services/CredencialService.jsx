// src/services/CredencialService.jsx
import { toApi } from "./AuthService.jsx";

// Ajusta estas rutas según tu Swagger:
const CREDENCIAL_ME_PATH = "/credenciales/me";          // GET datos de la credencial del usuario logueado
const CREDENCIAL_RENOVAR_PATH = "/credenciales/renovar"; // POST para renovar (ejemplo)

/**
 * Trae la credencial del usuario actual.
 * GET /api/credenciales/me
 *
 * Devuelve:
 *  - objeto credencial si existe
 *  - null si el backend responde 404 o 204 (sin credencial)
 */
export async function fetchMyCredencial() {
  const res = await fetch(toApi(CREDENCIAL_ME_PATH), {
    method: "GET",
    credentials: "include",
    headers: {
      Accept: "application/json",
    },
  });

  if (res.status === 404 || res.status === 204) {
    // usuario sin credencial
    return null;
  }

  if (!res.ok) {
    const txt = await res.text();
    throw new Error(txt || `Error al obtener credencial: ${res.status}`);
  }

  return res.json(); // devuelve el objeto que mande tu API
}

/**
 * Renueva la credencial.
 * Ejemplo: POST /api/credenciales/renovar
 * Si tu backend usa otra ruta (por id, etc.), cámbialo aquí.
 */
export async function renovarCredencial(credencialId) {
  const res = await fetch(toApi(CREDENCIAL_RENOVAR_PATH), {
    method: "POST",
    credentials: "include",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ id: credencialId }),
  });

  if (!res.ok) {
    const txt = await res.text();
    throw new Error(txt || `Error al renovar: ${res.status}`);
  }

  return res.json().catch(() => null); // por si el backend devuelve vacío
}
