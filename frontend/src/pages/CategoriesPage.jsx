// CategoriesPage.jsx
import React, { useState, useEffect } from 'react';
import Navbar from '../components/Navbar';

export default function CategoriesPage() {
  // State variabelen voor het opslaan van de database gegevens en laad-statussen
  const [categories, setCategories] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    // Backend request moet hier komen.
    // Voorbeeld endpoint: GET /api/categories
    // De backend moet data terugsturen op basis van de 'categories' tabel: id, name, description.
    
    const fetchCategories = async () => {
      try {
        // TODO: Vervang dit door de daadwerkelijke fetch() of axios.get() request
        // const response = await fetch(`${import.meta.env.VITE_API_URL}/categories');
        // const data = await response.json();
        
        // Tijdelijke mock data gebaseerd op het meegeleverde database schema
        const mockData = [
          { id: 1, name: 'Woonkamer', description: 'Meubels voor de woonkamer' },
          { id: 2, name: 'Eetkamer', description: 'Alles voor de eethoek' },
          { id: 3, name: 'Slaapkamer', description: 'Bedden en kasten' },
          { id: 4, name: 'Kasten', description: 'Opbergruimte' },
          { id: 5, name: 'Verlichting', description: 'Sfeer en functie' },
          { id: 6, name: 'Tuin', description: 'Buitenmeubelen' }
        ];
        
        // setCategories(data); // Activeer dit wanneer de backend is gekoppeld
        setCategories(mockData); 
        setIsLoading(false);
      } catch (err) {
        setError('Fout bij het ophalen van de categorieën.');
        setIsLoading(false);
      }
    };

    fetchCategories();
  }, []);

  // UI weergave tijdens het wachten op de backend
  if (isLoading) {
    return <div className="loading-container text-center mt-20">Laden...</div>;
  }

  // UI weergave bij een foutmelding van de backend
  if (error) {
    return <div className="error-container text-center mt-20 text-red-500">{error}</div>;
  }

  return (
    <div className="page-container min-h-screen bg-gray-50 font-sans">
      <Navbar />
      
      <main className="content-wrapper max-w-6xl mx-auto px-16 py-12">
        <h1 className="page-title text-2xl font-bold text-center mb-10 uppercase">
          Al Onze Categorieën
        </h1>
        
        <div className="categories-grid grid grid-cols-3 gap-6">
          {categories.map((category) => (
            // De 'key' gebruikt nu de unieke 'id' uit de database in plaats van de array index
            <div 
              key={category.id} 
              className="category-card relative h-64 bg-gray-800 rounded-lg overflow-hidden group cursor-pointer flex items-center justify-center"
            >
              {/* Overlay voor de categorie achtergrond */}
              <div className="category-overlay absolute inset-0 bg-gray-600 opacity-60 group-hover:opacity-50 transition"></div>
              
              <div className="category-content relative z-10 text-center">
                {/* De categorienaam uit de database wordt hier gerenderd */}
                <h2 className="category-name text-white text-2xl font-bold tracking-wide">
                  {category.name}
                </h2>
              </div>
            </div>
          ))}
        </div>
      </main>
    </div>
  );
}