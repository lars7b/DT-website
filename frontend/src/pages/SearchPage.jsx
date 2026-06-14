// SearchPage.jsx
import React, { useEffect, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import Navbar from '../components/Navbar';

export default function SearchPage() {
  // useSearchParams haalt de zoekterm uit de URL.
  // Voorbeeld: /zoeken?q=bank
  const [searchParams, setSearchParams] = useSearchParams();

  // Zelfde stijl als ProfilePage: basis-url apart opslaan.
  // .env voorbeeld: VITE_API_URL=https://localhost:7042/api
  const baseUrl = import.meta.env.VITE_API_URL;

  // Zoekterm uit de URL halen.
  // Als er geen q in de URL staat, gebruiken we een lege string.
  const queryFromUrl = searchParams.get('q') || '';

  // State variabelen voor zoeken
  const [searchTerm, setSearchTerm] = useState(queryFromUrl);
  const [products, setProducts] = useState([]);

  // State variabelen voor laadstatus en foutmelding
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => {
    setSearchTerm(queryFromUrl);
  }, [queryFromUrl]);

  useEffect(() => {
    // Als er nog geen zoekterm is, tonen we nog geen resultaten.
    if (!queryFromUrl.trim()) {
      setProducts([]);
      return;
    }

    // Backend request voor zoeken.
    // Endpoint uit ProductsController: GET /api/products?search=zoekterm
    const fetchSearchResults = async () => {
      try {
        setIsLoading(true);
        setError(null);

        const params = new URLSearchParams();
        params.append('search', queryFromUrl);

        const url = `${baseUrl}/products?${params.toString()}`;
        console.log('Zoeken via:', url);

        const response = await fetch(url);

        console.log('Search response status:', response.status);

        if (!response.ok) {
          throw new Error(`Zoeken mislukt. Status: ${response.status}`);
        }

        const data = await response.json();
        console.log('Search results:', data);

        setProducts(data);
      } catch (error) {
        console.error('Fout bij zoeken:', error);
        setError(error.message || 'Fout bij het zoeken naar producten.');
      } finally {
        setIsLoading(false);
      }
    };

    if (baseUrl) {
      fetchSearchResults();
    } else {
      setError('VITE_API_URL ontbreekt.');
    }
  }, [queryFromUrl, baseUrl]);

  const handleSearchSubmit = (event) => {
    event.preventDefault();

    const trimmedSearchTerm = searchTerm.trim();

    if (!trimmedSearchTerm) {
      setSearchParams({});
      setProducts([]);
      return;
    }

    // Zet de zoekterm in de URL.
    // Hierdoor wordt useEffect opnieuw uitgevoerd en haalt de pagina resultaten op.
    setSearchParams({ q: trimmedSearchTerm });
  };

  return (
    <div className="search-page-container min-h-screen bg-gray-50 font-sans">
      <Navbar />

      <main className="max-w-6xl mx-auto px-16 py-12">
        <section className="search-header bg-white p-8 shadow-sm mb-8">
          <h1 className="text-3xl font-bold mb-6">
            Zoeken
          </h1>

          <form onSubmit={handleSearchSubmit} className="flex gap-3">
            <input
              type="text"
              value={searchTerm}
              onChange={(event) => setSearchTerm(event.target.value)}
              placeholder="Zoek naar producten, bijvoorbeeld bank of stoel..."
              className="flex-1 border border-gray-300 rounded px-4 py-3 focus:outline-none focus:border-blue-600"
            />

            <button
              type="submit"
              className="bg-blue-600 hover:bg-blue-700 text-white font-bold px-8 py-3 rounded transition"
            >
              Zoeken
            </button>
          </form>

          {queryFromUrl && (
            <p className="text-sm text-gray-500 mt-4">
              Zoekresultaten voor: <span className="font-semibold">“{queryFromUrl}”</span>
            </p>
          )}
        </section>

        <section className="search-results bg-white p-8 shadow-sm">
          {isLoading ? (
            <div className="loading-state text-center py-10 text-gray-500">
              Producten zoeken...
            </div>
          ) : error ? (
            <div className="error-state text-center py-10 text-red-500">
              {error}
              <p className="text-gray-500 text-sm mt-4">
                Controleer in de console welke URL wordt aangeroepen en welke statuscode terugkomt.
              </p>
            </div>
          ) : !queryFromUrl ? (
            <div className="empty-search-state text-center py-10 text-gray-500">
              Vul een zoekterm in om producten te zoeken.
            </div>
          ) : products.length === 0 ? (
            <div className="no-results-state text-center py-10 text-gray-500">
              Geen producten gevonden voor “{queryFromUrl}”.
            </div>
          ) : (
            <div className="product-grid grid grid-cols-3 gap-6">
              {products.map((product) => (
                <Link
                  to={`/product/${product.id}`}
                  key={product.id}
                  className="product-card group cursor-pointer bg-white p-4 block hover:shadow-md transition-shadow border border-gray-100"
                >
                  <div className="product-image-container relative w-full h-48 bg-gray-100 mb-4 flex items-center justify-center">
                    <span className="image-placeholder text-gray-400">
                      [Afbeelding]
                    </span>
                  </div>

                  <h3 className="product-name font-semibold text-sm">
                    {product.name}
                  </h3>

                  <p className="product-price text-sm text-gray-600">
                    €{' '}
                    {parseFloat(product.price).toLocaleString('nl-NL', {
                      minimumFractionDigits: 2,
                      maximumFractionDigits: 2,
                    })}
                  </p>
                </Link>
              ))}
            </div>
          )}
        </section>
      </main>
    </div>
  );
}
