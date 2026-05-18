// src/components/Navbar.jsx
import React from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function Navbar() {
  // Haal de status en de logout functie op uit de AuthContext
  const { isLoggedIn, isAdmin, logout } = useAuth();

  return (
    <nav className="navbar-container flex items-center justify-between px-8 py-4 border-b border-gray-200 bg-white">
      <Link to="/" className="brand-logo text-2xl font-bold">WoonWereld</Link>
      
      <div className="nav-links flex space-x-8 text-sm font-semibold text-gray-700">
        <Link to="/categorieen" className="nav-link flex items-center hover:text-black">
          Categorieën <span className="dropdown-icon ml-1 text-xs">▼</span>
        </Link>
        <Link to="/producten" className="nav-link hover:text-black">Aanbiedingen</Link>
        <a href="#" className="nav-link hover:text-black">Blog</a>
      </div>
      
      <div className="nav-actions flex space-x-5 text-gray-600 items-center">
        <button className="search-button hover:text-black">🔍</button>
        
        {/* Schakel dynamisch tussen de profielknop en inlogpoppetje */}
        {isLoggedIn ? (
          <div className="flex items-center space-x-4">
            {/* Stuur admins naar /admin, en klanten naar /profiel */}
            {isAdmin ? (
              <Link to="/admin" className="text-sm font-bold text-blue-600 hover:text-blue-800">
                Admin Dashboard
              </Link>
            ) : (
              <Link to="/profiel" className="text-sm font-semibold hover:text-black">
                Mijn Profiel
              </Link>
            )}
            
            <button onClick={logout} className="text-sm font-semibold text-red-600 hover:text-red-700 cursor-pointer">
              Uitloggen
            </button>
          </div>
        ) : (
          <Link to="/login" className="account-button hover:text-black text-lg">👤</Link>
        )}
        
        <button className="cart-button relative hover:text-black">
          🛒
          <span className="cart-badge absolute -top-2 -right-2 bg-orange-500 text-white text-[10px] font-bold px-1.5 py-0.5 rounded-full">
            0
          </span>
        </button>
      </div>
    </nav>
  );
}