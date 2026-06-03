import React, { useState, useEffect } from 'react';
import Navbar from '../components/Navbar';
import { Link } from 'react-router-dom';

export default function ShoppingCartPage() {
  const [cart, setCart] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchCart = async () => {
      setIsLoading(true);
      try {
        // 🔁 replace with real API call
        // const res = await fetch(`${import.meta.env.VITE_API_URL}/api/shoppingcart`, {
        //   credentials: 'include'
        // });
        // if (!res.ok) throw new Error();
        // const data = await res.json();

        const mockCart = {
          id: 1,
          customerId: 1,
          items: [
            { id: 1, productId: 1, quantity: 1 },
            { id: 2, productId: 2, quantity: 2 }
          ]
        };

        setCart(mockCart);
        setIsLoading(false);
      } catch (err) {
        setError('Fout bij het ophalen van de winkelwagen.');
        setIsLoading(false);
      }
    };

    fetchCart();
  }, []);

  const handleRemoveItem = async (cartItemId) => {
    try {
      // await fetch(`${import.meta.env.VITE_API_URL}/api/shoppingcart/items/${cartItemId}`, {
      //   method: 'DELETE',
      //   credentials: 'include'
      // });

      setCart(prev => ({
        ...prev,
        items: prev.items.filter(i => i.id !== cartItemId)
      }));
    } catch (err) {
      console.error(err);
    }
  };

  const handleQuantityChange = async (item, newQty) => {
    if (newQty < 1) return;

    try {
      // await fetch(`${import.meta.env.VITE_API_URL}/api/shoppingcart/items`, {
      //   method: 'PUT',
      //   headers: { 'Content-Type': 'application/json' },
      //   credentials: 'include',
      //   body: JSON.stringify({ id: item.id, productId: item.productId, quantity: newQty })
      // });

      setCart(prev => ({
        ...prev,
        items: prev.items.map(i =>
          i.id === item.id ? { ...i, quantity: newQty } : i
        )
      }));
    } catch (err) {
      console.error(err);
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
                  <p className="font-semibold">Product #{item.productId}</p>

                  <div className="flex items-center gap-2 mt-2">
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
                  </div>
                </div>

                <button
                  onClick={() => handleRemoveItem(item.id)}
                  className="text-red-500 hover:underline"
                >
                  Verwijder
                </button>
              </div>
            ))}

            <div className="mt-6 bg-white p-4 shadow-sm">
              <p className="font-semibold">
                Totaal items: {totalItems}
              </p>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}