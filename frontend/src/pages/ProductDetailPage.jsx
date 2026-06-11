// ProductDetailPage.jsx
import React, { useState, useEffect } from "react";
import { useParams } from "react-router-dom";
import Navbar from "../components/Navbar";
import {useNavigate} from "react-router-dom";


export default function ProductDetailPage() {
  // useParams haalt het product ID uit de URL, bijv. /product/1
  // Zorg ervoor dat de route in App.jsx is ingesteld als: <Route path="/product/:id" element={<ProductDetailPage />} />
  const { id } = useParams();
  const navigate = useNavigate();

  // State variabelen voor het opslaan van de database gegevens en laad-statussen
  const [product, setProduct] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    // Backend request voor het ophalen van specifieke product details.
    // Voorbeeld endpoint: GET /api/products/${id}
    // De backend moet data terugsturen op basis van de 'products' tabel: id, name, description, price.

    const fetchProductDetails = async () => {
      try {
        // TODO: Vervang dit door de daadwerkelijke fetch() of axios.get() request
        // const response = await fetch(`${import.meta.env.VITE_API_URL}/${id || 1}`);
        // if (!response.ok) throw new Error('Product niet gevonden');
        // const data = await response.json();

        // Tijdelijke mock data gebaseerd op het meegeleverde database schema
        const mockData = {
          id: id || 1,
          name: 'Bank "Rotterdam"',
          description:
            "Lorem ipsum l-orcalor sit amet, consectetur adipiscing elit, sed diam nonummy nibh euismod tincidunt ut laoreet dolore magna aliquam erat volutpat.",
          price: "899.00",
          // Attributen zoals afmetingen en materiaal staan momenteel niet expliciet in de database schema,
          // deze kunnen toegevoegd worden in een JSON veld of in de description text.
          dimensions: "220x95x85 cm",
          material: "Stof",
        };

        // setProduct(data); // Activeer dit wanneer de backend is gekoppeld
        setProduct(mockData);
        setIsLoading(false);
      } catch (err) {
        setError("Fout bij het ophalen van het product.");
        setIsLoading(false);
      }
    };

    fetchProductDetails();
  }, [id]);

  // Handler voor het toevoegen aan de winkelwagen
  const [addingToCart, setAddingToCart] = useState(false);

  const handleAddToCart = async () => {
    try {
      setAddingToCart(true);

      const token = localStorage.getItem("token");

      const response = await fetch(
        `${import.meta.env.VITE_API_URL}/shoppingcart/items`,
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${token}`,
          },
          body: JSON.stringify({
            productId: product.id,
            quantity: 1,
          }),
        },
      );
      // await refreshCart();

      if (!response.ok) {
        throw new Error("Toevoegen aan winkelwagen mislukt");
      }
      navigate("/winkelwagen");
      // alert("Product toegevoegd aan winkelwagen");
    } catch (err) {
      console.error(err);
      alert("Fout bij toevoegen aan winkelwagen");
    } finally {
      setAddingToCart(false);
    }
  };

  // Handler voor het toevoegen aan favorieten
  const handleToggleFavorite = () => {
    // Backend request moet hier komen.
    // Voorbeeld endpoint: POST /api/favorites
    // Verwachte payload: { customer_id: (uit user sessie), product_id: product.id }
    // De backend voegt dit toe aan de 'favorites' tabel.
    console.log(`Product ${product.id} toegevoegd aan favorieten`);
  };

  // UI weergave tijdens het laden
  if (isLoading) {
    return (
      <div className="page-container min-h-screen bg-gray-50 font-sans">
        <Navbar />
        <div className="loading-container text-center py-20 text-gray-500">
          Product laden...
        </div>
      </div>
    );
  }

  // UI weergave bij een foutmelding
  if (error || !product) {
    return (
      <div className="page-container min-h-screen bg-gray-50 font-sans">
        <Navbar />
        <div className="error-container text-center py-20 text-red-500">
          {error || "Product niet gevonden."}
        </div>
      </div>
    );
  }

  return (
    <div className="page-container min-h-screen bg-gray-50 font-sans">
      <Navbar />

      <main className="product-details-wrapper max-w-6xl mx-auto px-16 py-12 flex gap-12 bg-white mt-8 shadow-sm">
        {/* Linkerzijde: Afbeeldingen galerij */}
        <div className="product-images-section w-1/2">
          {/* TODO: In de toekomst kunnen product afbeeldingen uit een aparte tabel ('product_images') gehaald worden */}
          <div className="main-image-container w-full h-96 bg-gray-100 mb-4 flex items-center justify-center text-gray-400">
            [Hoofdafbeelding {product.name}]
          </div>
          <div className="thumbnail-gallery flex space-x-4">
            {[1, 2, 3, 4].map((thumb) => (
              <div
                key={thumb}
                className="thumbnail-item w-20 h-20 bg-gray-100 flex items-center justify-center text-xs text-gray-400 cursor-pointer border hover:border-black transition"
              >
                [Thumb {thumb}]
              </div>
            ))}
          </div>
        </div>

        {/* Rechterzijde: Product Informatie */}
        <div className="product-info-section w-1/2 flex flex-col justify-start pt-4">
          <h1 className="product-title text-3xl font-bold mb-2">
            {product.name}
          </h1>

          <p className="product-price text-2xl font-bold text-black mb-6">
            €{" "}
            {parseFloat(product.price).toLocaleString("nl-NL", {
              minimumFractionDigits: 2,
              maximumFractionDigits: 2,
            })}
          </p>

          <p className="product-description text-gray-600 text-sm mb-6 leading-relaxed">
            {product.description}
          </p>

          <div className="product-attributes text-sm mb-6 space-y-1 font-medium">
            <p>
              Afmetingen:{" "}
              <span className="font-normal text-gray-600">
                {product.dimensions}
              </span>
            </p>
            <p>
              Materiaal:{" "}
              <span className="font-normal text-gray-600">
                {product.material}
              </span>
            </p>
          </div>

          {/* TODO: Voorraad status zou dynamisch berekend kunnen worden als er een 'inventory' tabel wordt toegevoegd */}
          <div className="stock-status flex items-center text-green-600 text-sm font-semibold mb-8">
            <span className="status-icon mr-2">✔</span> Op voorraad
          </div>

          <div className="product-actions flex items-center space-x-4">
            <button
              onClick={handleAddToCart}
              disabled={addingToCart}
              className="add-to-cart-button bg-orange-500 hover:bg-orange-600 text-white font-bold py-3 px-8 rounded flex-1 transition disabled:opacity-50"
            >
              {addingToCart ? "TOEVOEGEN..." : "IN WINKELWAGEN"}
            </button>
            <button
              onClick={handleToggleFavorite}
              className="favorite-button p-3 border border-gray-300 rounded hover:bg-gray-50 text-gray-500 transition"
              title="Toevoegen aan favorieten"
            >
              ♡
            </button>
          </div>
        </div>
      </main>
    </div>
  );
}
