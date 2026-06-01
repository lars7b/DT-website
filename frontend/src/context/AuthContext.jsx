// src/context/AuthContext.jsx
import React, { createContext, useState, useEffect, useContext } from 'react';
import { jwtDecode } from 'jwt-decode';

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [token, setToken] = useState(null);
  const [role, setRole] = useState(null);
  const [authLoading, setAuthLoading] = useState(true);

  // Helper functie om de rol uit de token te halen
  const extractRoleFromToken = (jwt) => {
    try {
      const decoded = jwtDecode(jwt);
      // .NET zet de rol vaak in deze specifieke ClaimType URI, of gewoon in 'role'
      return decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || decoded.role || 'Customer';
    } catch (error) {
      console.error("Fout bij decoderen JWT", error);
      return null;
    }
  };

  useEffect(() => {
    const storedToken = localStorage.getItem('token');
    if (storedToken) {
      setToken(storedToken);
      setRole(extractRoleFromToken(storedToken));
    }
    setAuthLoading(false);
  }, []);

  const login = (newToken) => {
    localStorage.setItem('token', newToken);
    setToken(newToken);
    setRole(extractRoleFromToken(newToken));
  };

  const logout = () => {
    localStorage.removeItem('token');
    setToken(null);
    setRole(null);
  };

  const isLoggedIn = !!token;
  const isAdmin = role === 'Admin' || role === 'Employee'; // Afhankelijk van wie in het dashboard mag

  return (
    <AuthContext.Provider value={{ token, role, isLoggedIn, isAdmin, login, logout, authLoading }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  return useContext(AuthContext);
}