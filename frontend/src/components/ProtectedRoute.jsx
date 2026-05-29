// src/components/ProtectedRoute.jsx
import React from 'react';
import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function ProtectedRoute({ allowedRoles }) {
  const { role, isLoggedIn, authLoading } = useAuth();

  if (authLoading) {
    return <div className="text-center py-20">Laden...</div>;
  }

  if (!isLoggedIn) {
    // Niet ingelogd? Naar login.
    return <Navigate to="/login" replace />;
  }

  if (allowedRoles && !allowedRoles.includes(role)) {
    // Wel ingelogd, maar niet de juiste rol? Naar homepagina.
    return <Navigate to="/" replace />;
  }

  // Alles is goed, laat de gevraagde component(en) zien
  return <Outlet />;
}