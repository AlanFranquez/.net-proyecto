import { useEffect } from "react";
import { UAParser } from "ua-parser-js";
import { toApi, useAuth } from "../services/AuthService.jsx";

const BROWSER_ID_KEY = "e_browser_id";

const getBrowserId = () => {
  let id = localStorage.getItem(BROWSER_ID_KEY);
  if (!id) {
    id = crypto?.randomUUID?.() ?? `${Date.now()}-${Math.random()}`;
    localStorage.setItem(BROWSER_ID_KEY, id);
  }
  return id;
};

const getBrowserInfo = () => {
  const parser = new UAParser();
  const r = parser.getResult();
  return {
    browserId: getBrowserId(),
    browserName: r.browser.name || "Web",
    browserVersion: r.browser.version || "",
  };
};

export default function useAutoRegisterBrowserDevice() {
  const { user, loading: authLoading } = useAuth();

  useEffect(() => {
    if (authLoading || !user) return;

    const usuarioIdActual = user?.usuarioId ?? user?.UsuarioId ?? user?.id;
    if (!usuarioIdActual) return;

    const { browserId, browserName, browserVersion } = getBrowserInfo();

    (async () => {
      try {
        // 1) Trae dispositivos
        const res = await fetch(toApi("/dispositivos"), {
          method: "GET",
          credentials: "include",
          headers: { Accept: "application/json" },
        });
        if (!res.ok) return;
        const data = await res.json();
        if (!Array.isArray(data)) return;

        // 2) Ya existe este browser?
        const already = data.some(
          (d) =>
            String(d.HuellaDispositivo ?? d.huellaDispositivo ?? "") ===
            String(browserId)
        );
        if (already) return;

        // 3) Crear dispositivo Web automático
        const payload = {
          usuarioId: usuarioIdActual,
          numeroTelefono: null,
          plataforma: "Web",
          biometriaHabilitada: false,
          estado: "Enrolado",
          huellaDispositivo: browserId,
          navegadorNombre: browserName,
          navegadorVersion: browserVersion,
        };

        const res2 = await fetch(toApi("/dispositivos"), {
          method: "POST",
          credentials: "include",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify(payload),
        });

        if (!res2.ok) {
          const txt = await res2.text();
          console.error("Auto-register failed:", txt);
        }
      } catch (e) {
        console.error("Auto-register error:", e);
      }
    })();
  }, [authLoading, user?.usuarioId, user?.id]);
}
