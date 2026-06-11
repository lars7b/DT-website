// CategoriesPage.jsx
import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import Navbar from '../components/Navbar';

export default function CategoriesPage() {
  // State variabelen voor het opslaan van de database gegevens en laad-statussen
  const [categories, setCategories] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    // Backend request: GET /api/categories
    // De backend stuurt data terug op basis van de 'categories' tabel: id, name, description.
    const controller = new AbortController();

    const fetchCategories = async () => {
      try {
        setIsLoading(true);
        setError(null);

        const response = await fetch(`${import.meta.env.VITE_API_URL}/categories`, {
          signal: controller.signal
        });

        if (!response.ok) {
          throw new Error('Kon categorieën niet ophalen.');
        }

        const data = await response.json();
        setCategories(Array.isArray(data) ? data : []);
      } catch (err) {
        if (err.name !== 'AbortError') {
          setError(err.message || 'Fout bij het ophalen van de categorieën.');
        }
      } finally {
        setIsLoading(false);
      }
    };

    fetchCategories();
    return () => controller.abort();
  }, []);

  // UI weergave tijdens het wachten op de backend
  return (
    <div className="page-container min-h-screen bg-gray-50 font-sans">
      <Navbar />
      
      <main className="content-wrapper max-w-6xl mx-auto px-16 py-12">
        <h1 className="page-title text-2xl font-bold text-center mb-10 uppercase">
          Al Onze Categorieën
        </h1>
        
        {isLoading ? (
          <div className="loading-container text-center py-12 text-gray-500">Categorieën laden...</div>
        ) : error ? (
          <div className="error-container text-center py-12 text-red-500">{error}</div>
        ) : (
          <div className="categories-grid grid grid-cols-3 gap-6">
            {categories.map((category) => (
              <Link
                key={category.id}
                to={`/categorieen/${category.id}`}
                className="category-card relative h-64 bg-gray-800 rounded-lg overflow-hidden group cursor-pointer flex items-center justify-center"
              >
                <div className="category-overlay absolute inset-0 bg-gray-600 opacity-60 group-hover:opacity-50 transition"></div>
                
                <div className="category-content relative z-10 text-center px-6">
                  <h2 className="category-name text-white text-2xl font-bold tracking-wide">
                    {category.name}
                  </h2>
                  {category.description && (
                    <p className="category-description text-white/80 text-sm mt-3">
                      {category.description}
                    </p>
                  )}
                </div>
              </Link>
            ))}
            {categories.length === 0 && (
              <div className="col-span-3 text-center py-12 text-gray-500">
                Geen categorieën gevonden.
              </div>
            )}
          </div>
        )}
      </main>
    </div>
  );
}