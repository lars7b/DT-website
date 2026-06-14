import React, { useState, useEffect } from "react";
import Navbar from "../components/Navbar";
import { Link } from "react-router-dom";
import { categoryPlaceholders } from "../data/categoryPlaceholders";

export default function HomePage() {
  const [popularProducts, setPopularProducts] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchPopularProducts = async () => {
      try {
        setIsLoading(true);

        const response = await fetch(
          `${import.meta.env.VITE_API_URL}/products?limit=5`,
        );

        if (!response.ok) {
          throw new Error("Fout bij ophalen producten");
        }

        const data = await response.json();

        setPopularProducts(data);
      } catch (err) {
        setError(err.message);
      } finally {
        setIsLoading(false);
      }
    };

    fetchPopularProducts();
  }, []);

  return (
    <div className="home-page-container min-h-screen bg-gray-50 font-sans">
      <Navbar />

      <section className="hero-section relative w-full h-[400px] bg-gray-300 flex items-center px-16">
        <div className="hero-overlay absolute inset-0 bg-gray-400 opacity-50" />
        <div className="hero-content relative z-10 max-w-lg">
          <h1 className="text-5xl font-bold text-white mb-6">
            CREËER JE DROOMINTERIEUR
          </h1>
        </div>
      </section>

      <section className="px-16 py-12 bg-white">
        {isLoading ? (
          <p>Loading...</p>
        ) : error ? (
          <p className="text-red-500">{error}</p>
        ) : (
          <div className="grid grid-cols-5 gap-6">
            {popularProducts.map((product) => {
              const image =
                categoryPlaceholders[Number(product.categoryId)] ??
                "/placeholder-category.jpg";

              return (
                <Link
                  to={`/product/${product.id}`}
                  className="block bg-white p-4 hover:shadow-md transition"
                >
                  <div className="w-full h-40 bg-gray-100 overflow-hidden rounded mb-3">
                    <img
                      src={image}
                      alt={product.name}
                      className="w-full h-full object-cover"
                    />
                  </div>

                  <h3 className="text-sm font-semibold">{product.name}</h3>

                  <p className="text-sm text-gray-600">
                    €{" "}
                    {Number(product.price).toLocaleString("nl-NL", {
                      minimumFractionDigits: 2,
                    })}
                  </p>
                </Link>
              );
            })}
          </div>
        )}
      </section>
    </div>
  );
}
