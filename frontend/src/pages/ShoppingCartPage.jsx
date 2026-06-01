// ShoppingCartPage.jsx
import React, { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import Navbar from '../components/Navbar';

export default function ShoppingCartPage(){
    // State variabelen voor het opslaan van de database gegevens en laad-statussen
    const [shoppingcart, setshoppingcart] = useState(null);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
    // Backend request moet hier komen voor de shoppingcart.
    // Voorbeeld endpoint: GET /api/shoppingcart
    const fetchCart = async () => {
      setIsLoading(true);
      try {
        // TODO: Vervang door daadwerkelijke fetch()
        // const data = await res.json();

        // todo gebruikt mock data voor testen
        const mockShoppingCart = [
          { id: 1, customerid: 1, items: [{id: 1 , productid: 1 , quantity: 1}, {id: 2, productid: 2, quantity: 2}] },
        ];
        
        setshoppingcart(mockShoppingCart);
        setIsLoading(false);
      } catch (err) {
        setError('Fout bij het ophalen van de winkelwagen.');
        setIsLoading(false);
      }
    };
    // TODO Fetch products of the shopping Cart
    fetchCart();
    }, []);

    // todo fix rendering
  return (
    <div className="products-page-container min-h-screen bg-gray-50 font-sans">
      <Navbar />
      
      <div className="page-layout flex px-16 py-8 max-w-7xl mx-auto">
        {/* Producten Grid & Sortering */}
        <main className="main-content flex-1">
          <div className="sorting-controls flex justify-end mb-6 text-sm">
            <div className="sorting-wrapper flex items-center space-x-2">
              <span className="sorting-label text-gray-500">Sorteer op:</span>
              <select 
                className="sorting-dropdown border border-gray-300 p-1 rounded bg-white cursor-pointer"
                value={sortOrder}
                onChange={(e) => setSortOrder(e.target.value)}
              >
                <option value="price_asc">Prijs oplopend</option>
                <option value="price_desc">Prijs aflopend</option>
                <option value="newest">Nieuwste</option>
              </select>
            </div>
          </div>

          {isLoading ? (
            <div className="loading-state text-center py-10 text-gray-500">Producten laden...</div>
          ) : error ? (
            <div className="error-state text-center py-10 text-red-500">{error}</div>
          ) : (
            <div className="product-grid grid grid-cols-3 gap-6">
              {products.map((product) => (
                <Link to={`/product/${product.id}`} key={product.id} className="product-card group cursor-pointer bg-white p-4 block hover:shadow-md transition-shadow">
                  <div className="product-image-container relative w-full h-48 bg-gray-100 mb-4 flex items-center justify-center">
                    <button 
                      onClick={(e) => handleToggleFavorite(e, product.id)}
                      className="favorite-toggle-btn absolute top-2 right-2 text-gray-400 hover:text-red-500 z-10"
                      title="Toevoegen aan favorieten"
                    >
                      ♡
                    </button>
                    <span className="image-placeholder text-gray-400">[Afbeelding]</span>
                  </div>
                  <h3 className="product-name font-semibold text-sm">{product.name}</h3>
                  <p className="product-price text-sm text-gray-600">
                    € {parseFloat(product.price).toLocaleString('nl-NL', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                  </p>
                </Link>
              ))}
              
              {products.length === 0 && (
                <div className="no-results col-span-3 text-center py-10 text-gray-500">
                  Geen producten gevonden voor deze selectie.
                </div>
              )}
            </div>
          )}
        </main>
      </div>
    </div>
  );
}