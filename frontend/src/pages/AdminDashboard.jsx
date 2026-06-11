// src/AdminDashboard.jsx
import React, { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import { Link } from 'react-router-dom';

export default function AdminDashboard() {
  const { token, logout, role } = useAuth();
  const [activeTab, setActiveTab] = useState('dashboard');

  const [foundCustomer, setFoundCustomer] = useState(null);
  const [customerOrders, setCustomerOrders] = useState([]);
  const [error, setError] = useState("");

  const [employeeInfo, setEmployeeInfo] = useState(null);
  const [isInfoLoading, setIsInfoLoading] = useState(false);
  const [infoError, setInfoError] = useState("");

  const [employeeForm, setEmployeeForm] = useState({email: '', password: '', firstName: '', lastName: '', phone: '', position: ''});
  const [createStatus, setCreateStatus] = useState({ message: "", error: "" });

  const baseUrl = import.meta.env.VITE_API_URL;

  const tabTitles = {
  'dashboard': 'Overzicht',
  'products': 'Producten Beheer',
  'users': 'Bestellingen',
  'create-employee': 'Nieuwe Medewerker'
  };

  useEffect(() => {
  if (activeTab !== 'dashboard' || !token) return;

  const fetchEmployeeProfile = async () => {
    setIsInfoLoading(true);
    setInfoError("");
    try {
      const res = await fetch(`${baseUrl}/employee/me`, {
        headers: { 'Authorization': `Bearer ${token}` }
      });
      if (!res.ok) throw new Error('Kon medewerkersprofiel niet laden.');
      const data = await res.json();
      setEmployeeInfo(data);
    } catch (err) {
      setInfoError(err.message);
    } finally {
      setIsInfoLoading(false);
    }
  };

  fetchEmployeeProfile();
  }, [activeTab, token, baseUrl]);

  const handleCreateEmployee = async (e) => {
  e.preventDefault();
  setCreateStatus({ message: "", error: "" });

    try {
      // Let op: pas de URL aan als de endpoint in een andere controller staat (bijv. /employee/employees)
      const res = await fetch(`${baseUrl}/admin/employees`, { 
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(employeeForm)
      });
      
      const data = await res.json();
      if (!res.ok) throw new Error(data.message || 'Fout bij het aanmaken van de medewerker.');
      
      setCreateStatus({ message: data.message, error: "" });
      setEmployeeForm({ email: '', password: '', firstName: '', lastName: '', phone: '', position: '' });
      } catch (err) {
        setCreateStatus({ message: "", error: err.message });
      }
  };

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
            onClick={() => setActiveTab('users')}
            className={`w-full text-left px-4 py-2 rounded ${activeTab === 'users' ? 'bg-blue-600' : 'hover:bg-gray-800'}`}
          >
            Bestellingen
          </button>
          {role === 'Admin' && (
          <button
            onClick={() => setActiveTab('create-employee')}
            className={`w-full text-left px-4 py-2 rounded ${activeTab === 'create-employee' ? 'bg-blue-600' : 'hover:bg-gray-800'}`}
          >
            Nieuwe Medewerker
          </button>
          )}
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
          <h2 className="text-2xl font-bold mb-6 capitalize">{tabTitles[activeTab]}</h2>
          
          {activeTab === 'dashboard' && (
            <div className="space-y-6">
              <div className="border-b border-gray-200 pb-4">
                <p className="text-gray-600">
                  Welkom in het admin dashboard. Hieronder vind je jouw actuele profiel- en organisatiegegevens.
                </p>
              </div>

              {isInfoLoading && <p className="text-sm text-gray-500">Je profielgegevens worden geladen...</p>}
              {infoError && <p className="text-sm text-red-600 font-medium">{infoError}</p>}

              {employeeInfo && (
                <div className="max-w-xl bg-gray-50 rounded border border-gray-200 p-6 space-y-4">
                  {/* Profiel Header met Initialen */}
                  <div className="flex items-center space-x-4">
                    <div className="h-12 w-12 rounded-full bg-gray-800 text-white flex items-center justify-center font-bold text-lg">
                      {employeeInfo.firstName ? employeeInfo.firstName[0] : ''}
                      {employeeInfo.lastName ? employeeInfo.lastName[0] : ''}
                    </div>
                    <div>
                      <h3 className="text-lg font-bold text-gray-900">
                        {employeeInfo.firstName} {employeeInfo.lastName}
                      </h3>
                      <span className="inline-block bg-blue-100 text-blue-800 text-xs px-2.5 py-0.5 rounded-full font-semibold capitalize mt-0.5">
                        {employeeInfo.position || 'Medewerker'}
                      </span>
                    </div>
                  </div>

                  {/* Alleen-lezen Gegevenslijst */}
                  <div className="grid grid-cols-1 gap-4 text-sm border-t border-gray-200 pt-4">
                    <div>
                      <span className="block font-semibold text-gray-400 text-xs uppercase tracking-wider mb-0.5">
                        E-mailadres
                      </span>
                      <span className="text-gray-900 font-medium">{employeeInfo.email}</span>
                    </div>
                    
                    <div>
                      <span className="block font-semibold text-gray-400 text-xs uppercase tracking-wider mb-0.5">
                        Telefoonnummer
                      </span>
                      <span className="text-gray-900 font-medium">
                        {employeeInfo.phone || 'Niet opgegeven'}
                      </span>
                    </div>

                    <div>
                      <span className="block font-semibold text-gray-400 text-xs uppercase tracking-wider mb-0.5">
                        Functieomschrijving
                      </span>
                      <span className="text-gray-900 font-medium">
                        {employeeInfo.position || 'Algemeen Medewerker'}
                      </span>
                    </div>

                    <div>
                      <span className="block font-semibold text-gray-400 text-xs uppercase tracking-wider mb-0.5">
                        Toegangsniveau (Systeemrol)
                      </span>
                      <span className="text-gray-900 font-medium capitalize">{role}</span>
                    </div>
                  </div>
                </div>
              )}
            </div>
          )}

          {activeTab === 'users' && (
            <div className="space-y-6">
              {/* Zoekbalk Sectie */}
              <div>
                <label className="block text-sm font-semibold mb-2">Zoek klant op e-mailadres</label>
                <div className="flex gap-2">
                  <input 
                    type="email" 
                    placeholder="bijv. klant@email.nl"
                    id="searchEmail"
                    className="flex-1 border border-gray-300 p-2 rounded text-sm focus:outline-none focus:border-blue-600"
                  />
                  <button 
                    onClick={async () => {
                      const email = document.getElementById('searchEmail').value;
                      if (!email) return;
                      
                      // Reset eerdere resultaten
                      setFoundCustomer(null);
                      setCustomerOrders([]);
                      setError("");

                      try {
                        const res = await fetch(`${baseUrl}/employee/customer-by-email?email=${encodeURIComponent(email)}`, {
                          headers: { 'Authorization': `Bearer ${token}` }
                        });
                        const data = await res.json();
                        if (!res.ok) throw new Error(data.message || 'Klant niet gevonden');
                        setFoundCustomer(data);
                      } catch (err) {
                        setError(err.message);
                      }
                    }}
                    className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded text-sm font-semibold transition"
                  >
                    Zoeken
                  </button>
                </div>
                {error && <p className="text-red-600 text-xs mt-2 font-medium">{error}</p>}
              </div>

              {/* Gevonden Klant Gegevens */}
              {foundCustomer && (
                <div className="border border-gray-200 rounded p-6 bg-white space-y-4 max-w-2xl">
                  <div className="flex justify-between items-start">
                    <div>
                      <h3 className="text-lg font-bold text-gray-900">
                        {foundCustomer.firstName} {foundCustomer.lastName || '(Geen naam ingevuld)'}
                      </h3>
                      <p className="text-sm text-gray-500">{foundCustomer.email}</p>
                    </div>
                    <button
                      onClick={async () => {
                        try {
                          const res = await fetch(`${baseUrl}/employee/customers/${foundCustomer.userId}/orders`, {
                            headers: { 'Authorization': `Bearer ${token}` }
                          });
                          const data = await res.json();
                          setCustomerOrders(data);
                        } catch (err) {
                          console.error('Fout bij ophalen orders:', err);
                        }
                      }}
                      className="text-sm bg-gray-800 hover:bg-black text-white px-3 py-1.5 rounded transition"
                    >
                      Toon Bestellingen
                    </button>
                  </div>

                  <div className="grid grid-cols-2 gap-4 text-sm border-t border-gray-100 pt-4">
                    <div>
                      <span className="block font-semibold text-gray-500">Telefoonnummer</span>
                      <span>{foundCustomer.phone || 'Niet opgegeven'}</span>
                    </div>
                    <div>
                      <span className="block font-semibold text-gray-500">Adres</span>
                      <span>{foundCustomer.address || 'Niet opgegeven'}</span>
                    </div>
                  </div>

                  {/* Bestellingen Sectie (Lazy Loaded) */}
                  {customerOrders.length > 0 ? (
                    <div className="border-t border-gray-100 pt-4 mt-4">
                      <h4 className="font-bold text-sm mb-3">Bestelgeschiedenis</h4>
                      <div className="overflow-x-auto">
                        <table className="w-full text-left text-sm">
                          <thead>
                            <tr className="bg-gray-50 text-gray-600 border-b border-gray-200">
                              <th className="p-2 font-semibold">Order ID</th>
                              <th className="p-2 font-semibold">Datum</th>
                              <th className="p-2 font-semibold">Status</th>
                            </tr>
                          </thead>
                          <tbody>
                            {customerOrders.map(order => (
                              <tr key={order.id} className="border-b border-gray-100 last:border-0 hover:bg-gray-50">
                                <td className="p-2 font-medium">#{order.id}</td>
                                <td className="p-2 text-gray-600">{new Date(order.orderDate).toLocaleDateString('nl-NL')}</td>
                                <td className="p-2">
                                  <span className="inline-block bg-blue-50 text-blue-700 px-2 py-0.5 rounded text-xs font-semibold">
                                    {order.status}
                                  </span>
                                </td>
                              </tr>
                            ))}
                          </tbody>
                        </table>
                      </div>
                    </div>
                  ) : customerOrders && (
                    <p className="text-xs text-gray-400 italic pt-2">Klik op 'Toon Bestellingen' om de geschiedenis in te laden.</p>
                  )}
                </div>
              )}
            </div>
          )}

          {activeTab === 'create-employee' && role === 'Admin' && (
            <div>
              
              
              <form onSubmit={handleCreateEmployee} className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-semibold text-gray-700 mb-1">Voornaam</label>
                    <input 
                      type="text" 
                      required
                      value={employeeForm.firstName}
                      onChange={(e) => setEmployeeForm({...employeeForm, firstName: e.target.value})}
                      className="w-full border border-gray-300 p-2 rounded text-sm focus:outline-none focus:border-blue-600"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-semibold text-gray-700 mb-1">Achternaam</label>
                    <input 
                      type="text" 
                      required
                      value={employeeForm.lastName}
                      onChange={(e) => setEmployeeForm({...employeeForm, lastName: e.target.value})}
                      className="w-full border border-gray-300 p-2 rounded text-sm focus:outline-none focus:border-blue-600"
                    />
                  </div>
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-semibold text-gray-700 mb-1">E-mailadres</label>
                    <input 
                      type="email" 
                      required
                      value={employeeForm.email}
                      onChange={(e) => setEmployeeForm({...employeeForm, email: e.target.value})}
                      className="w-full border border-gray-300 p-2 rounded text-sm focus:outline-none focus:border-blue-600"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-semibold text-gray-700 mb-1">Telefoonnummer</label>
                    <input 
                      type="text" 
                      required
                      value={employeeForm.phone}
                      onChange={(e) => setEmployeeForm({...employeeForm, phone: e.target.value})}
                      className="w-full border border-gray-300 p-2 rounded text-sm focus:outline-none focus:border-blue-600"
                    />
                  </div>
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-semibold text-gray-700 mb-1">Functie (Position)</label>
                    <input 
                      type="text" 
                      required
                      placeholder="bijv. Klantenservice"
                      value={employeeForm.position}
                      onChange={(e) => setEmployeeForm({...employeeForm, position: e.target.value})}
                      className="w-full border border-gray-300 p-2 rounded text-sm focus:outline-none focus:border-blue-600"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-semibold text-gray-700 mb-1">Tijdelijk Wachtwoord</label>
                    <input 
                      type="password" 
                      required
                      value={employeeForm.password}
                      onChange={(e) => setEmployeeForm({...employeeForm, password: e.target.value})}
                      className="w-full border border-gray-300 p-2 rounded text-sm focus:outline-none focus:border-blue-600"
                    />
                  </div>
                </div>

                <div className="pt-4 flex items-center justify-between">
                  <button 
                    type="submit"
                    className="bg-gray-800 hover:bg-black text-white px-6 py-2 rounded text-sm font-semibold transition"
                  >
                    Opslaan als Medewerker
                  </button>

                  {createStatus.message && <span className="text-sm text-green-600 font-medium">{createStatus.message}</span>}
                  {createStatus.error && <span className="text-sm text-red-600 font-medium">{createStatus.error}</span>}
                </div>
              </form>
            </div>
          )}

          {/* Voeg hier de andere weergaven toe... */}
        </div>
      </main>
      
    </div>
  );
}