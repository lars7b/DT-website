import React, { useState, useEffect } from "react";
import Navbar from "../components/Navbar";
import { Link, useNavigate } from "react-router-dom";

export default function ShoppingCartPage() {
  const [cart, setCart] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);
  const navigate = useNavigate();
  useEffect(() => {
    const fetchCart = async () => {
      setIsLoading(true);

      try {
        const token = localStorage.getItem("token");

        const res = await fetch(
          `${import.meta.env.VITE_API_URL}/ShoppingCart`,
          {
            method: "GET",
            headers: {
              Authorization: `Bearer ${token}`,
              "Content-Type": "application/json",
            },
          },
        );

        if (res.status === 404) {
          setCart(null);
          return;
        }

        if (!res.ok) {
          throw new Error(`HTTP ${res.status}`);
        }

        const data = await res.json();

        setCart(data);
      } catch (err) {
        console.error(err);
        setError("Fout bij het ophalen van de winkelwagen.");
      } finally {
        setIsLoading(false);
      }
    };

    fetchCart();
  }, []);

  const handleRemoveItem = async (cartItemId) => {
    try {
      const token = localStorage.getItem("token");

      const res = await fetch(
        `${import.meta.env.VITE_API_URL}/ShoppingCart/items/${cartItemId}`,
        {
          method: "DELETE",
          headers: {
            Authorization: `Bearer ${token}`,
          },
        },
      );

      if (!res.ok) {
        throw new Error();
      }
      // await refreshCart();
      setCart((prev) => ({
        ...prev,
        items: prev.items.filter((i) => i.id !== cartItemId),
      }));
    } catch (err) {
      console.error(err);
    }
  };

  const handleQuantityChange = async (item, newQty) => {
    if (newQty < 1) return;

    try {
      const token = localStorage.getItem("token");

      const res = await fetch(
        `${import.meta.env.VITE_API_URL}/ShoppingCart/items`,
        {
          method: "PUT",
          headers: {
            Authorization: `Bearer ${token}`,
            "Content-Type": "application/json",
          },
          body: JSON.stringify({
            id: item.id,
            productId: item.productId,
            quantity: newQty,
          }),
        },
      );

      if (!res.ok) {
        throw new Error();
      }

      setCart((prev) => ({
        ...prev,
        items: prev.items.map((i) =>
          i.id === item.id ? { ...i, quantity: newQty } : i,
        ),
      }));
    } catch (err) {
      console.error(err);
    }
  };
  const handlePlaceOrder = async () => {
    try {
      const token = localStorage.getItem("token");

      const res = await fetch(`${import.meta.env.VITE_API_URL}/order`, {
        method: "POST",
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      if (!res.ok) {
        throw new Error(`HTTP ${res.status}`);
      }

      // optional UX improvement: clear cart UI
      setCart((prev) => ({
        ...prev,
        items: [],
      }));
      // navigate("/bestellingen"); 
       navigate("/afrekenen"); 
      // alert("Bestelling geplaatst!");
    } catch (err) {
      console.error(err);
      alert("Fout bij plaatsen van bestelling");
    }
  };

  const totalItems = cart?.items?.reduce((sum, i) => sum + i.quantity, 0) || 0;

  return (
    <div className="min-h-screen bg-gray-50 font-sans">
      <Navbar />

      <div className="max-w-5xl mx-auto px-6 py-10">
        <h1 className="text-2xl font-bold mb-6">Winkelwagen</h1>

        {isLoading ? (
          <div className="text-gray-500">Winkelwagen laden...</div>
        ) : error ? (
          <div className="text-red-500">{error}</div>
        ) : !cart || cart.items.length === 0 ? (
          <div className="text-gray-500">Je winkelwagen is leeg.</div>
        ) : (
          <div className="space-y-4">
            {cart.items.map((item) => (
              <div
                key={item.id}
                className="bg-white p-4 flex justify-between items-center shadow-sm"
              >
                <div>
                  <Link
                    to={`/product/${item.productId}`}
                    className="font-semibold text-blue-600 hover:underline"
                  >
                    {item.productName}
                  </Link>

                  <p className="text-sm text-gray-500">
                    {item.productDescription}
                  </p>

                  <p className="text-sm font-bold mt-1">
                    € {item.pricePerUnit?.toFixed(2)}
                  </p>

                  <Link
                    to={`/product/${item.productId}`}
                    className="text-sm text-blue-600 hover:underline"
                  >
                    Bekijk product
                  </Link>

                  {/* <div className="flex items-center gap-2 mt-2">
                    <button
                      className="px-2 py-1 bg-gray-200"
                      onClick={() =>
                        handleQuantityChange(item, item.quantity - 1)
                      }
                    >
                      -
                    </button>

                    <span>{item.quantity}</span>

                    <button
                      className="px-2 py-1 bg-gray-200"
                      onClick={() =>
                        handleQuantityChange(item, item.quantity + 1)
                      }
                    >
                      +
                    </button>
                  </div> */}
                </div>

                <button
                  onClick={() => handleRemoveItem(item.id)}
                  className="text-red-500 hover:underline"
                >
                  Verwijder
                </button>
              </div>
            ))}

            <div className="mt-6 bg-white p-4 shadow-sm flex justify-between items-center">
              <p className="font-semibold">Totaal items: {totalItems}</p>

              <button
                onClick={handlePlaceOrder}
                className="bg-orange-500 hover:bg-orange-600 text-white font-bold px-4 py-2 rounded"
              >
                Bestelling plaatsen
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
