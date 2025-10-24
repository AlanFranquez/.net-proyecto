import React, { useState } from "react";
import { Routes, Route, Navigate } from "react-router-dom";

import Home from "./pages/Homepage.jsx";
import Dispositivos from "./pages/Dispositivos.jsx";
import Canjes from "./pages/Canjes.jsx";
import Login from "./pages/Login.jsx";
import Perfil from "./pages/Perfil.jsx";
import Beneficios from "./pages/Beneficios.jsx";
import Credencial from "./pages/Credencial.jsx";
import HistorialDeAccesos from "./pages/HistorialDeAccesos.jsx";
import Autenticacion from "./pages/Autenticacion.jsx";
import Registrarse from "./pages/Registrarse.jsx";

export default function App() {
  const [isLoggedIn, setIsLoggedIn] = useState(false);

  const Private = ({ children }) =>
    isLoggedIn ? children : <Navigate to="/login" replace />;

  const AnonOnly = ({ children }) =>
    !isLoggedIn ? children : <Navigate to="/" replace />;

  return (
    <Routes>
      <Route
        path="/"
        element={
          <Home
            isLoggedIn={isLoggedIn}
            onToggle={() => setIsLoggedIn((v) => !v)}
          />
        }
      />
      <Route
        path="/canjes"
        element={
          <Canjes
            isLoggedIn={isLoggedIn}
            onToggle={() => setIsLoggedIn((v) => !v)}
          />
        }
      />
      <Route
  path="/registrarse"
  element={
    <AnonOnly>
      <Registrarse
        isLoggedIn={isLoggedIn}
        onToggle={() => setIsLoggedIn((v) => !v)}
      />
    </AnonOnly>
  }
/>
      <Route
        path="/login"
        element={
          <AnonOnly>
            <Login
              isLoggedIn={isLoggedIn}
              onToggle={() => setIsLoggedIn((v) => !v)}
            />
          </AnonOnly>
        }
      />
      <Route
        path="/perfil"
        element={
          <Perfil
            isLoggedIn={isLoggedIn}
            onToggle={() => setIsLoggedIn((v) => !v)}
          />
        }
      />
      <Route
        path="/dispositivos"
        element={
          <Dispositivos
            isLoggedIn={isLoggedIn}
            onToggle={() => setIsLoggedIn((v) => !v)}
          />
        }
      />
      <Route
        path="/credencial"
        element={
          <Credencial
            isLoggedIn={isLoggedIn}
            onToggle={() => setIsLoggedIn((v) => !v)}
          />
        }
      />
      <Route
  path="/accesos"
  element={
    <HistorialDeAccesos
      isLoggedIn={isLoggedIn}
      onToggle={() => setIsLoggedIn((v) => !v)}
    />
  }
/><Route
  path="/autenticacion"
  element={
    <Autenticacion
      isLoggedIn={isLoggedIn}
      onToggle={() => setIsLoggedIn(v => !v)}
    />
  }
/>
      <Route path="/beneficios" element={<Beneficios  isLoggedIn={isLoggedIn}
      onToggle={() => setIsLoggedIn(v => !v)}/>} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
