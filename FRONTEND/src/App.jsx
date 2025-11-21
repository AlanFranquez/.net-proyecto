// src/App.jsx
import React from "react";
import { Routes, Route, Navigate } from "react-router-dom";
import { ProtectedRoute } from "./services/AuthService.jsx";

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
import TestApi from "./pages/TestApi.jsx";
import Novedades from "./pages/Novedades.jsx";
import Notificaciones from "./pages/Notificaciones.jsx";


export default function App() {
  return (
    <Routes>
      <Route path="/" element={<Home />} />

      <Route path="/registrarse" element={<Registrarse />} />
      <Route path="/login" element={<Login />} />

      {/* Rutas protegidas */}
      <Route
        path="/perfil"
        element={
          <ProtectedRoute fallback={<Navigate to="/login" replace />}>
            <Perfil />
          </ProtectedRoute>
        }
      />

      <Route
        path="/dispositivos"
        element={
          <ProtectedRoute fallback={<Navigate to="/login" replace />}>
            <Dispositivos />
          </ProtectedRoute>
        }
      />

      <Route
        path="/credencial"
        element={
          <ProtectedRoute fallback={<Navigate to="/login" replace />}>
            <Credencial />
          </ProtectedRoute>
        }
      />

      <Route
        path="/accesos"
        element={
          <ProtectedRoute fallback={<Navigate to="/login" replace />}>
            <HistorialDeAccesos />
          </ProtectedRoute>
        }
      />

      <Route
        path="/autenticacion"
        element={
          <ProtectedRoute fallback={<Navigate to="/login" replace />}>
            <Autenticacion />
          </ProtectedRoute>
        }
      />

      <Route
        path="/beneficios"
        element={
          <ProtectedRoute fallback={<Navigate to="/login" replace />}>
            <Beneficios />
          </ProtectedRoute>
        }
      />

      <Route
        path="/novedades"
        element={
          <ProtectedRoute fallback={<Navigate to="/login" replace />}>
            <Novedades />
          </ProtectedRoute>
        }
      />
      <Route
        path="/notificaciones"
        element={
          <ProtectedRoute fallback={<Navigate to="/login" replace />}>
            <Notificaciones />
          </ProtectedRoute>
        }
      />
      <Route path="/canjes" element={<Canjes />} />
      <Route path="/test-api" element={<TestApi />} />

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
