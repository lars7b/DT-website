// src/context/AuthContext.jsx
import React, { createContext, useState, useEffect, useContext } from 'react';

// De Context aanmaken
const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [token, setToken] = useState(null);
  const [authLoading, setAuthLoading] = useState(true);

  useEffect(() => {
    // Bij het opstarten van de app controleren we of er al een token in localStorage staat
    const storedToken = localStorage.getItem('token');
    if (storedToken) {
      setToken(storedToken);
    }
    setAuthLoading(false);
  }, []);

  // Functie die wordt aangeroepen na een succesvolle login request
  const login = (newToken) => {
    localStorage.setItem('token', newToken);
    setToken(newToken);
  };

  // Functie om uit te loggen
  const logout = () => {
    localStorage.removeItem('token');
    setToken(null);
  };

  // Helper om snel te zien of de gebruiker is ingelogd
  const isLoggedIn = !!token;

  return (
    <AuthContext.Provider value={{ token, isLoggedIn, login, logout, authLoading }}>
      {children}
    </AuthContext.Provider>
  );
}

// Custom hook om de auth context gemakkelijk te gebruiken in andere componenten
export function useAuth() {
  return useContext(AuthContext);
}