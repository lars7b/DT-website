// HomePage.jsx
import React, { useState, useEffect } from 'react';
import Navbar from '../components/Navbar';

export default function HomePage() {
  // State variabelen voor het opslaan van de database gegevens en laad-statussen
  const [popularProducts, setPopularProducts] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    // Backend request moet hier komen.
    // Voorbeeld endpoint: GET /api/products/popular of GET /api/products?limit=5
    // De backend moet data terugsturen op basis van de 'products' tabel: id, name, price.
    
    const fetchPopularProducts = async () => {
      try {
        // TODO: Vervang dit door de daadwerkelijke fetch() of axios.get() request
        // const response = await fetch(`${import.meta.env.VITE_API_URL}/popular');
        // const data = await response.json();
        
        // Tijdelijke mock data gebaseerd op het meegeleverde database schema
        const mockData = [
          { id: 1, name: 'Modulaire Bank "Hof"', price: '145.00' },
          { id: 2, name: 'Houten Eettafel "Rijn"', price: '120.00' },
          { id: 3, name: 'Eetkamerstoel "Maes"', price: '149.00' },
          { id: 4, name: 'Lamp Temp', price: '145.06' },
          { id: 5, name: 'Lamps Lomeom', price: '59.00' },
        ];
        
        // setPopularProducts(data); // Activeer dit wanneer de backend is gekoppeld
        setPopularProducts(mockData);
        setIsLoading(false);
      } catch (err) {
        setError('Fout bij het ophalen van de populaire producten.');
        setIsLoading(false);
      }
    };

    fetchPopularProducts();
  }, []);

  return (
    <div className="home-page-container min-h-screen bg-gray-50 font-sans">
      <Navbar />
      
      {/* Hero Sectie */}
      <section className="hero-section relative w-full h-[400px] bg-gray-300 flex items-center px-16">
        {/* Placeholder voor hero afbeelding */}
        <div className="hero-overlay absolute inset-0 bg-gray-400 opacity-50"></div>
        <div className="hero-content relative z-10 max-w-lg">
          <h1 className="hero-title text-5xl font-bold text-white leading-tight mb-6">
            CREËER JE<br />DROOMINTERIEUR
          </h1>
          <button className="hero-button bg-blue-600 text-white px-8 py-3 font-semibold hover:bg-blue-700 transition">
            SHOP NU
          </button>
        </div>
      </section>

      {/* Populaire Producten Sectie */}
      <section className="popular-products-section px-16 py-12 bg-white">
        <div className="products-header flex justify-between items-center mb-6">
          <h2 className="products-title text-xl font-bold uppercase">Populaïr deze week</h2>
          <div className="products-navigation space-x-2">
            <button className="nav-button-prev p-2 border bg-gray-100 rounded">{'<'}</button>
            <button className="nav-button-next p-2 border bg-gray-100 rounded">{'>'}</button>
          </div>
        </div>
        
        {/* Weergave tijdens het laden of bij een foutmelding specifiek voor de producten */}
        {isLoading ? (
          <div className="loading-container text-center py-10 text-gray-500">Producten laden...</div>
        ) : error ? (
          <div className="error-container text-center py-10 text-red-500">{error}</div>
        ) : (
          <div className="products-grid grid grid-cols-5 gap-6">
            {popularProducts.map((product) => (
              <div key={product.id} className="product-card group cursor-pointer">
                <div className="product-image-container relative w-full h-48 bg-gray-100 mb-3 flex items-center justify-center rounded">
                  {/* TODO: Request toevoegen voor het opslaan van favorieten op basis van product.id */}
                  <button className="favorite-button absolute top-2 right-2 text-gray-400 hover:text-red-500">♡</button>
                  <span className="image-placeholder text-gray-400">[Afbeelding]</span>
                </div>
                <h3 className="product-name font-semibold text-sm">{product.name}</h3>
                {/* Prijs formattering (ervan uitgaande dat de backend DECIMAL(10,2) als string of number stuurt) */}
                <p className="product-price text-sm text-gray-600">
                  € {parseFloat(product.price).toLocaleString('nl-NL', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                </p>
              </div>
            ))}
          </div>
        )}
      </section>
    </div>
  );
}