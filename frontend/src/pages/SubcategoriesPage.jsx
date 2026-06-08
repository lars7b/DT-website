// SubcategoriesPage.jsx
import React, { useEffect, useMemo, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import Navbar from '../components/Navbar';

export default function SubcategoriesPage() {
  const { categoryId } = useParams();

  const [subcategories, setSubcategories] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const controller = new AbortController();

    const fetchSubcategories = async () => {
      try {
        setIsLoading(true);
        setError(null);

        const response = await fetch(
          `${import.meta.env.VITE_API_URL}/subcategories?categoryId=${encodeURIComponent(categoryId ?? '')}`,
          { signal: controller.signal }
        );

        if (!response.ok) {
          throw new Error('Kon subcategorieen niet ophalen.');
        }

        const data = await response.json();
        setSubcategories(Array.isArray(data) ? data : []);
      } catch (err) {
        if (err.name !== 'AbortError') {
          setError(err.message || 'Fout bij het ophalen van de subcategorieen.');
        }
      } finally {
        setIsLoading(false);
      }
    };

    fetchSubcategories();
    return () => controller.abort();
  }, [categoryId]);

  const categoryTitle = useMemo(() => {
    if (!subcategories.length) return 'Subcategorieen';
    return subcategories[0].categoryName || 'Subcategorieen';
  }, [subcategories]);

  return (
    <div className="page-container min-h-screen bg-gray-50 font-sans">
      <Navbar />

      <main className="content-wrapper max-w-6xl mx-auto px-16 py-12">
        <div className="flex items-center justify-between mb-10">
          <div>
            <p className="text-xs uppercase tracking-widest text-gray-500 mb-2">Categorie</p>
            <h1 className="text-2xl font-bold uppercase">{categoryTitle}</h1>
          </div>
          <Link
            to="/categorieen"
            className="text-sm font-semibold text-gray-600 hover:text-black transition"
          >
            {'<- Terug naar categorieen'}
          </Link>
        </div>

        {isLoading ? (
          <div className="loading-container text-center py-12 text-gray-500">Subcategorieen laden...</div>
        ) : error ? (
          <div className="error-container text-center py-12 text-red-500">{error}</div>
        ) : (
          <div className="categories-grid grid grid-cols-3 gap-6">
            {subcategories.map((subcategory) => (
              <div
                key={subcategory.id}
                className="category-card relative h-56 bg-white rounded-lg overflow-hidden border border-gray-200 group cursor-pointer flex items-center justify-center"
              >
                <div className="category-overlay absolute inset-0 bg-gray-100 opacity-70 group-hover:opacity-50 transition"></div>

                <div className="category-content relative z-10 text-center px-6">
                  <h2 className="category-name text-gray-900 text-xl font-bold tracking-wide">
                    {subcategory.name}
                  </h2>
                  {subcategory.description && (
                    <p className="category-description text-gray-600 text-sm mt-3">
                      {subcategory.description}
                    </p>
                  )}
                </div>
              </div>
            ))}

            {subcategories.length === 0 && (
              <div className="col-span-3 text-center py-12 text-gray-500">
                Geen subcategorieen gevonden.
              </div>
            )}
          </div>
        )}
      </main>
    </div>
  );
}
