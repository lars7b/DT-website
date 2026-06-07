// ProductsPage.jsx
import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import Navbar from '../components/Navbar';

export default function ProductsPage() {
  // State variabelen voor het opslaan van producten, categorieën en laad-statussen
  const [products, setProducts] = useState([]);
  const [categories, setCategories] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);

  // State voor filters en sortering (klaar om mee te sturen in een API request)
  const [sortOrder, setSortOrder] = useState('price_asc');
  const [selectedCategories, setSelectedCategories] = useState([]);

  useEffect(() => {
    // Backend request moet hier komen voor de filter zijbalk.
    // Voorbeeld endpoint: GET /api/categories
    // Haalt data op uit de 'categories' tabel.
    const fetchCategories = async () => {
      try {
        // TODO: Vervang door daadwerkelijke fetch()
        // const res = await fetch(`${import.meta.env.VITE_API_URL}/categories');
        // const data = await res.json();
        
        const mockCategories = [
          { id: 1, name: 'Banken' },
          { id: 2, name: 'Stoelen' },
          { id: 3, name: 'Slaapkamer' },
          { id: 4, name: 'Woonkamer' },
          { id: 5, name: 'Kasten' },
          { id: 6, name: 'Verlichting' }
        ];
        setCategories(mockCategories);
      } catch (err) {
        console.error("Fout bij ophalen categorieën", err);
      }
    };

    fetchCategories();
  }, []);

  useEffect(() => {
    // Backend request moet hier komen voor de producten.
    // Voorbeeld endpoint: GET /api/products?sort=price_asc&category=1,2
    // Haalt data op uit de 'products' tabel, eventueel gefilterd op 'category_id'.
    const fetchProducts = async () => {
      setIsLoading(true);
      try {
        // TODO: Vervang door daadwerkelijke fetch() die rekening houdt met filters/sortering
        // const queryParams = new URLSearchParams({ sort: sortOrder, categories: selectedCategories.join(',') });
        // const res = await fetch(`${import.meta.env.VITE_API_URL}/products?${queryParams}`);
        // const data = await res.json();

        // Tijdelijke mock data gebaseerd op het schema (price is DECIMAL)
        const mockProducts = [
          { id: 1, name: 'Eetkamerstoel "Maes"', price: '149.00', category_id: 2 },
          { id: 2, name: 'Houten Eettafel "Rijn"', price: '120.00', category_id: 2 },
          { id: 3, name: 'Modulaire Bank "Hof"', price: '145.00', category_id: 1 },
          { id: 4, name: 'Kledingkast "Lund"', price: '299.50', category_id: 5 },
          { id: 5, name: 'Nachtkastje "Daan"', price: '45.00', category_id: 3 },
          { id: 6, name: 'Vloerlamp "Licht"', price: '89.99', category_id: 6 },
        ];
        
        setProducts(mockProducts);
        setIsLoading(false);
      } catch (err) {
        setError('Fout bij het ophalen van de producten.');
        setIsLoading(false);
      }
    };

    fetchProducts();
  }, [sortOrder, selectedCategories]); // Request wordt opnieuw uitgevoerd als sortering of filters veranderen

  const handleToggleFavorite = (e, productId) => {
    e.preventDefault(); // Voorkomt dat de Link naar de detailpagina wordt geactiveerd
    // Backend request moet hier komen.
    // Voorbeeld endpoint: POST /api/favorites
    // Verwachte payload: { customer_id: (uit user sessie), product_id: productId }
    console.log(`Product ${productId} toegevoegd aan favorieten`);
  };

  const handleCategoryFilterChange = (categoryId) => {
    setSelectedCategories(prev => 
      prev.includes(categoryId) 
        ? prev.filter(id => id !== categoryId) 
        : [...prev, categoryId]
    );
  };

  return (
    <div className="products-page-container min-h-screen bg-gray-50 font-sans">
      <Navbar />
      
      <div className="page-layout flex px-16 py-8 max-w-7xl mx-auto">
        {/* Zijbalk Filters */}
        <aside className="sidebar-filters w-64 pr-8 flex-shrink-0">
          <h2 className="filter-header font-bold mb-6">FILTEREN OP</h2>
          
          <div className="filter-section-category mb-6 border-b pb-4">
            <h3 className="filter-title font-semibold flex justify-between w-full mb-3">Categorie <span>^</span></h3>
            <ul className="category-list space-y-2 text-sm text-gray-600">
              {categories.map(cat => (
                <li key={cat.id} className="category-list-item">
                  <label className="cursor-pointer flex items-center">
                    <input 
                      type="checkbox" 
                      className="category-checkbox mr-2"
                      checked={selectedCategories.includes(cat.id)}
                      onChange={() => handleCategoryFilterChange(cat.id)}
                    /> 
                    {cat.name}
                  </label>
                </li>
              ))}
            </ul>
          </div>

          <div className="filter-section-price mb-6 border-b pb-4">
            <h3 className="filter-title font-semibold flex justify-between w-full mb-3">Prijs <span>^</span></h3>
            {/* Opmerking: Voor een werkende slider moet een aparte component of library gebruikt worden */}
            <div className="price-slider h-1 bg-gray-200 relative mb-4 mt-2">
              <div className="price-slider-track absolute left-0 right-1/4 h-full bg-blue-600"></div>
              <div className="price-slider-thumb-min absolute left-0 -top-1 w-3 h-3 bg-blue-600 rounded-full"></div>
              <div className="price-slider-thumb-max absolute right-1/4 -top-1 w-3 h-3 bg-blue-600 rounded-full"></div>
            </div>
            <div className="price-range-labels flex justify-between text-sm text-gray-600">
              <span>0</span>
              <span>-15500</span>
              <span>€ 149.00</span>
            </div>
          </div>

          <div className="filter-section-color mb-6 border-b pb-4">
            <h3 className="filter-title font-semibold flex justify-between w-full mb-3">Kleur <span>^</span></h3>
            <div className="color-options flex gap-2 flex-wrap">
              {/* Opmerking: Kleur zit momenteel niet in het database schema, kan later worden toegevoegd als attribuut */}
              {['bg-orange-800', 'bg-blue-600', 'bg-teal-500', 'bg-yellow-600', 'bg-gray-400', 'bg-black'].map((color, i) => (
                <div key={i} className={`color-swatch w-6 h-6 rounded ${color} cursor-pointer border border-gray-300`}></div>
              ))}
            </div>
          </div>

          <div className="filter-section-availability">
            <h3 className="filter-title font-semibold flex justify-between w-full mb-3">Beschikbaarheid <span>^</span></h3>
            <label className="availability-label text-sm text-gray-600 flex items-center cursor-pointer">
              <input type="checkbox" className="availability-checkbox mr-2" /> 
              Alleen op voorraad
            </label>
          </div>
        </aside>

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