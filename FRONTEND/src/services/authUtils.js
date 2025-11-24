// src/services/authUtils.js

// Base de la API (cambiable por env)
export const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL || "http://localhost:8080";

// Origin real (por si el env trae /api)
export const API_ORIGIN = (() => {
  try {
    return new URL(API_BASE_URL).origin;
  } catch {
    return API_BASE_URL;
  }
})();

// ✅ browser id key + helper
export const BROWSER_ID_KEY = "e_browser_id";

export const getBrowserId = () => {
  try {
    if (typeof window === "undefined") return "server";
    let id = localStorage.getItem(BROWSER_ID_KEY);
    if (!id) {
      id = crypto?.randomUUID?.() ?? `${Date.now()}-${Math.random()}`;
      localStorage.setItem(BROWSER_ID_KEY, id);
    }
    return id;
  } catch {
    return `${Date.now()}-${Math.random()}`;
  }
};

// build /api url
export const toApi = (p = "") => {
  const clean = p.startsWith("/") ? p : `/${p}`;
  const apiPath = clean.startsWith("/api/") ? clean : `/api${clean}`;
  return `${API_BASE_URL}${apiPath}`;
};
