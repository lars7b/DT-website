// src/LoginPage.jsx
import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
// import { useCart } from '../context/CartContext';
import Navbar from '../components/Navbar';

export default function LoginPage() {
  const navigate = useNavigate();
  const { login } = useAuth();
  
  const [isLoginView, setIsLoginView] = useState(true);
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');

  const [isLoading, setIsLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState('');
  const [successMessage, setSuccessMessage] = useState('');
  // const { refreshCart } = useCart();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setIsLoading(true);
    setErrorMessage('');
    setSuccessMessage('');
    
    // De toevoeging van /api is vereist om overeen te komen met de [Route("api/[controller]")] in de backend
    const baseUrl = `${import.meta.env.VITE_API_URL}/auth`;

    try {
      const endpoint = isLoginView ? '/login' : '/register';
      const payload = isLoginView 
        ? { email, password } 
        : { firstName, lastName, email, password };

      const response = await fetch(`${baseUrl}${endpoint}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      });
      
      // Lees het antwoord eerst als tekst om crashes op lege of HTML-responsen (zoals 404's) te voorkomen
      const text = await response.text();
      let data = {};
      
      if (text) {
        try {
          data = JSON.parse(text);
        } catch (parseError) {
          throw new Error('De server stuurde een onverwacht antwoord terug.');
        }
      }

      if (!response.ok) {
        // Vang FluentValidation validatiefouten op (status 400 Bad Request)
        if (response.status === 400 && data.errors) {
          // Haal alle specifieke foutmeldingen uit het errors object en voeg ze samen
          const validationErrors = Object.values(data.errors).flat().join('\n');
          throw new Error(validationErrors);
        }
        
        // Standaard foutmelding als het geen validatiefout is
        throw new Error(data.message || (isLoginView ? 'Inloggen mislukt.' : 'Registratie mislukt.'));
      }

      if (isLoginView) {
        login(data.token);
        navigate('/'); 
        // await refreshCart();
      } else {
        setSuccessMessage(data.message || 'Account succesvol aangemaakt! Je kunt nu inloggen.');
        setIsLoginView(true); 
        setPassword(''); 
      }
    } catch (error) {
      setErrorMessage(error.message);
    } finally {
      setIsLoading(false);
    }
  };

  const toggleView = () => {
    setIsLoginView(!isLoginView);
    setErrorMessage('');
    setSuccessMessage('');
  };

  return (
    <div className="login-page-container min-h-screen bg-gray-50 font-sans flex flex-col">
      <Navbar />
      
      <main className="auth-main-content flex-1 flex items-center justify-center py-12 px-4">
        <div className="auth-card bg-white p-8 border border-gray-200 shadow-sm w-full max-w-md rounded">
          <h1 className="auth-title text-2xl font-bold text-center mb-8 uppercase">
            {isLoginView ? 'Inloggen bij WoonWereld' : 'Account Aanmaken'}
          </h1>

          {errorMessage && (
            <div className="error-message bg-red-50 text-red-600 p-3 rounded mb-4 text-sm border border-red-200 whitespace-pre-line">
              {errorMessage}
            </div>
          )}
          {successMessage && (
            <div className="success-message bg-green-50 text-green-600 p-3 rounded mb-4 text-sm border border-green-200">
              {successMessage}
            </div>
          )}

          <form onSubmit={handleSubmit} className="auth-form flex flex-col space-y-5">
            {!isLoginView && (
              <div className="name-fields-container flex space-x-4">
                <div className="input-group flex-1">
                  <label className="input-label block text-sm font-semibold mb-1">Voornaam</label>
                  <input 
                    type="text" 
                    value={firstName}
                    onChange={(e) => setFirstName(e.target.value)}
                    className="text-input w-full border border-gray-300 p-2 rounded focus:outline-none focus:border-blue-600" 
                    required={!isLoginView}
                    disabled={isLoading}
                  />
                </div>
                <div className="input-group flex-1">
                  <label className="input-label block text-sm font-semibold mb-1">Achternaam</label>
                  <input 
                    type="text" 
                    value={lastName}
                    onChange={(e) => setLastName(e.target.value)}
                    className="text-input w-full border border-gray-300 p-2 rounded focus:outline-none focus:border-blue-600" 
                    required={!isLoginView}
                    disabled={isLoading}
                  />
                </div>
              </div>
            )}

            <div className="input-group">
              <label className="input-label block text-sm font-semibold mb-1">E-mailadres</label>
              <input 
                type="email" 
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                className="text-input w-full border border-gray-300 p-2 rounded focus:outline-none focus:border-blue-600" 
                required 
                disabled={isLoading}
              />
            </div>

            <div className="input-group">
              <label className="input-label block text-sm font-semibold mb-1">Wachtwoord</label>
              <input 
                type="password" 
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                className="text-input w-full border border-gray-300 p-2 rounded focus:outline-none focus:border-blue-600" 
                required 
                disabled={isLoading}
              />
            </div>

            <button 
              type="submit" 
              disabled={isLoading}
              className={`submit-button text-white font-bold py-3 rounded transition mt-4 ${isLoading ? 'bg-blue-400 cursor-not-allowed' : 'bg-blue-600 hover:bg-blue-700'}`}
            >
              {isLoading ? 'Bezig...' : (isLoginView ? 'INLOGGEN' : 'REGISTREREN')}
            </button>
          </form>

          <div className="auth-footer mt-8 text-center text-sm flex flex-col space-y-3">
            <button 
              onClick={toggleView} 
              disabled={isLoading}
              className="toggle-view-button text-gray-600 hover:text-black transition underline decoration-gray-300 underline-offset-4"
            >
              {isLoginView ? 'Nog geen account? Maak er een.' : 'Al een account? Log hier in.'}
            </button>
          </div>
        </div>
      </main>
    </div>
  );
}