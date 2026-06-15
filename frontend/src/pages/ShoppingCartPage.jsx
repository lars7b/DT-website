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
  const calculateTotal = () => {
    return cart.items.reduce(
      (sum, item) => sum + item.pricePerUnit * item.quantity,
      0,
    );
  };
  const calculateItem = () => {
    return cart.items.reduce(
      (sum, item) => sum + item.pricePerUnit * item.quantity,
      0,
    );
  };

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
          method: "POST",
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
      // alert("Fout bij plaatsen van bestelling");
      navigate("/login");
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
                className="bg-white p-5 rounded-lg shadow-sm flex justify-between items-start gap-6"
              >
                {/* LEFT SIDE */}
                <div className="flex-1">
                  <Link
                    to={`/product/${item.productId}`}
                    className="font-semibold text-blue-600 hover:underline text-lg"
                  >
                    {item.productName}
                  </Link>

                  <p className="text-sm text-gray-500 mt-1">
                    {item.productDescription}
                  </p>

                  <p className="text-sm text-gray-600 mt-2">
                    € {item.pricePerUnit.toFixed(2)} per stuk
                  </p>
                </div>

                {/* RIGHT SIDE */}
                <div className="flex flex-col items-end gap-3">
                  {/* quantity controls */}
                  <div className="flex items-center border rounded overflow-hidden">
                    <button
                      className="px-3 py-1 bg-gray-100 hover:bg-gray-200"
                      onClick={() =>
                        handleQuantityChange(item, item.quantity - 1)
                      }
                    >
                      -
                    </button>

                    <input
                      type="number"
                      min={1}
                      value={item.quantity}
                      onChange={(e) =>
                        handleQuantityChange(item, Number(e.target.value))
                      }
                      className="w-16 text-center border mx-2"
                    />

                    <button
                      className="px-3 py-1 bg-gray-100 hover:bg-gray-200"
                      onClick={() =>
                        handleQuantityChange(item, item.quantity + 1)
                      }
                    >
                      +
                    </button>
                  </div>

                  {/* totals */}
                  <div className="text-right">
                    <p className="text-sm text-gray-500">Totaal</p>

                    <p className="font-bold text-lg">
                      € {(item.pricePerUnit * item.quantity).toFixed(2)}
                    </p>
                  </div>

                  {/* remove */}
                  <button
                    onClick={() => handleRemoveItem(item.id)}
                    className="text-red-500 hover:text-red-700 text-sm"
                  >
                    Verwijder
                  </button>
                </div>
              </div>
            ))}

            <div className="mt-6 bg-white p-4 shadow-sm flex justify-between items-center">
              <p className="font-semibold">Totaal items: {totalItems}</p>
              {/* TOTAL */}
              <div className="flex justify-between pt-4 font-bold">
                <span>Totaal</span>
                <span>€ {calculateTotal().toFixed(2)}</span>
              </div>

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
