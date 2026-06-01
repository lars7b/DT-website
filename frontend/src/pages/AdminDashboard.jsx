// src/AdminDashboard.jsx
import React, { useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { Link } from 'react-router-dom';

export default function AdminDashboard() {
  const { logout, role } = useAuth();
  const [activeTab, setActiveTab] = useState('dashboard');

  return (
    <div className="min-h-screen bg-gray-100 flex font-sans">
      
      {/* Admin Sidebar */}
      <aside className="w-64 bg-gray-900 text-white flex flex-col">
        <div className="p-6 border-b border-gray-800">
          <h1 className="text-2xl font-bold">WoonWereld</h1>
          <p className="text-sm text-gray-400 mt-1">Beheerpaneel ({role})</p>
        </div>
        
        <nav className="flex-1 p-4 space-y-2">
          <button 
            onClick={() => setActiveTab('dashboard')}
            className={`w-full text-left px-4 py-2 rounded ${activeTab === 'dashboard' ? 'bg-blue-600' : 'hover:bg-gray-800'}`}
          >
            Overzicht
          </button>
          <button 
            onClick={() => setActiveTab('products')}
            className={`w-full text-left px-4 py-2 rounded ${activeTab === 'products' ? 'bg-blue-600' : 'hover:bg-gray-800'}`}
          >
            Producten Beheer
          </button>
          <button 
            onClick={() => setActiveTab('orders')}
            className={`w-full text-left px-4 py-2 rounded ${activeTab === 'orders' ? 'bg-blue-600' : 'hover:bg-gray-800'}`}
          >
            Bestellingen
          </button>
          <button 
            onClick={() => setActiveTab('users')}
            className={`w-full text-left px-4 py-2 rounded ${activeTab === 'users' ? 'bg-blue-600' : 'hover:bg-gray-800'}`}
          >
            Klanten & Medewerkers
          </button>
        </nav>
        
        <div className="p-4 border-t border-gray-800">
          <Link to="/" className="block w-full text-left px-4 py-2 text-sm text-gray-400 hover:text-white mb-2">
            ← Naar de winkel
          </Link>
          <button onClick={logout} className="w-full text-left px-4 py-2 bg-red-600 hover:bg-red-700 rounded transition">
            Uitloggen
          </button>
        </div>
      </aside>

      {/* Admin Main Content */}
      <main className="flex-1 p-8 overflow-auto">
        <div className="bg-white rounded shadow-sm border border-gray-200 p-6 min-h-full">
          <h2 className="text-2xl font-bold mb-6 capitalize">{activeTab}</h2>
          
          {activeTab === 'dashboard' && (
            <p className="text-gray-600">Welkom in het admin dashboard. Selecteer een tab aan de linkerkant om gegevens te beheren.</p>
          )}

          {activeTab === 'products' && (
            <div className="text-gray-600">
              <p>Hier komt de tabel voor de ProductRepository (CRUD operaties).</p>
              {/* Hier komt later een request naar GET /api/products en POST /api/products */}
            </div>
          )}

          {/* Voeg hier de andere weergaven toe... */}
        </div>
      </main>
      
    </div>
  );
}