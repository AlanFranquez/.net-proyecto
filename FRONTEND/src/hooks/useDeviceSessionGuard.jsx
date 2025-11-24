// src/hooks/useDeviceSessionGuard.jsx
import { useEffect, useRef } from "react";
import { useAuth, toApi, getBrowserId } from "../services/AuthService.jsx";

/**
 * Periodically verifies that the current browser device is still allowed.
 * If the device was revoked from another browser, /me should return 401
 * and we force logout + clear browser id.
 */
export default function useDeviceSessionGuard() {
  const { user, loading, logout } = useAuth();
  const timerRef = useRef(null);
  const runningRef = useRef(false);

  useEffect(() => {
    if (loading) return;

    // If not logged in -> no guard needed
    if (!user) {
      if (timerRef.current) clearInterval(timerRef.current);
      timerRef.current = null;
      return;
    }

    const check = async () => {
      if (runningRef.current) return;
      runningRef.current = true;

      try {
        const browserId = getBrowserId();

        const res = await fetch(toApi("/usuarios/me"), {
          method: "GET",
          credentials: "include",
          headers: {
            Accept: "application/json",
            "X-Device-Id": browserId, // 👈 important for backend to know device
          },
        });

        if (res.status === 401 || res.status === 403) {
          // Device revoked (or session invalid)
          localStorage.removeItem("e_browser_id");
          await logout();
          return;
        }

        // Optional: if backend returns user but includes device status,
        // you can also check it here and logout if revoked.
      } catch (e) {
        console.log(e);
        // network errors: ignore, do not log out
        // console.warn("device guard check failed", e);
      } finally {
        runningRef.current = false;
      }
    };

    // Run immediately once
    check();

    // Poll every 5s (tweak as you like)
    timerRef.current = setInterval(check, 5000);

    // Pause polling when tab is hidden; resume on focus
    const onVis = () => {
      if (document.hidden) {
        if (timerRef.current) clearInterval(timerRef.current);
        timerRef.current = null;
      } else {
        check();
        if (!timerRef.current) {
          timerRef.current = setInterval(check, 5000);
        }
      }
    };

    document.addEventListener("visibilitychange", onVis);

    return () => {
      document.removeEventListener("visibilitychange", onVis);
      if (timerRef.current) clearInterval(timerRef.current);
      timerRef.current = null;
    };
  }, [user, loading, logout]);
}
