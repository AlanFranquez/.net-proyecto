// src/services/AuthService.jsx
import React, {
  createContext,
  useContext,
  useEffect,
  useState,
  useCallback,
} from "react";

const AuthContext = createContext(null);

// Base de la API (cambiable por env)
const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL || "http://localhost:8080";

// Convierte "/usuarios/login" → "http://localhost:8080/api/usuarios/login"
const toApi = (p = "") => {
  const clean = p.startsWith("/") ? p : `/${p}`;
  const apiPath = clean.startsWith("/api/") ? clean : `/api${clean}`;
  return `${API_BASE_URL}${apiPath}`;
};

export default function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const setErrorFromCatch = (err) => {
    console.error(err);
    const msg = err?.message || String(err) || "Error desconocido";
    setError(msg.replace('{"error":"', "").replace('"}', ""));
  };

  // GET /api/usuarios/me
  const fetchMe = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await fetch(toApi("/usuarios/me"), {
        method: "GET",
        credentials: "include", // envía cookie espectaculos_session
        headers: { Accept: "application/json" },
      });

      if (res.status === 204 || res.status === 401) {
        setUser(null);
        setLoading(false);
        return null;
      }

      if (!res.ok) {
        const txt = await res.text();
        throw new Error(`Unexpected /me response: ${res.status} ${txt}`);
      }

      const data = await res.json();
      setUser(data);
      setLoading(false);
      return data;
    } catch (err) {
      console.error("fetchMe error:", err);
      setUser(null);
      setLoading(false);
      return null;
    }
  }, []);

  useEffect(() => {
    fetchMe();
  }, [fetchMe]);

  // POST /api/usuarios/registro
  const register = useCallback(
    async (body = {}) => {
      setLoading(true);
      setError(null);
      try {
        const res = await fetch(toApi("/usuarios/registro"), {
          method: "POST",
          credentials: "include",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify(body),
        });

        if (!res.ok) {
          const txt = await res.text();
          throw new Error(txt || `Register failed: ${res.status}`);
        }

        await fetchMe();
        return true;
      } catch (err) {
        console.error("register error:", err);
        setErrorFromCatch(err);
        setLoading(false);
        return false;
      }
    },
    [fetchMe]
  );

  // POST /api/usuarios/login
  const login = useCallback(
    async (body = {}) => {
      setLoading(true);
      setError(null);
      try {
        const res = await fetch(toApi("/usuarios/login"), {
          method: "POST",
          credentials: "include",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify(body),
        });

        if (!res.ok) {
          const txt = await res.text();
          throw new Error(txt || `Login failed: ${res.status}`);
        }

        await fetchMe();
        return true;
      } catch (err) {
        console.error("login error:", err);
        setErrorFromCatch(err);
        setLoading(false);
        return false;
      }
    },
    [fetchMe]
  );

  // POST /api/usuarios/logout
  const logout = useCallback(
    async () => {
      setLoading(true);
      setError(null);
      try {
        const res = await fetch(toApi("/usuarios/logout"), {
          method: "POST",
          credentials: "include",
        });

        if (!res.ok) {
          const txt = await res.text();
          throw new Error(txt || `Logout failed: ${res.status}`);
        }

        setUser(null);
        await fetchMe();

        return true;
      } catch (err) {
        console.error("logout error:", err);
        setErrorFromCatch(err);
        return false;
      } finally {
        setLoading(false);
      }
    },
    [fetchMe]
  );

  const value = {
    user,
    loading,
    error,
    refetchUser: fetchMe,
    register,
    login,
    logout,
    setUser,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

// eslint-disable-next-line react-refresh/only-export-components
export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within an AuthProvider");
  return ctx;
}

// Ruta protegida simple
export function ProtectedRoute({ children, fallback = null }) {
  const { user, loading } = useAuth();
  if (loading) return null;
  if (!user) return fallback;
  return children;
}
