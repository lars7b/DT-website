import React, { useEffect, useState } from 'react';
import Navbar from '../components/Navbar';
import { Link } from 'react-router-dom';

export default function OrdersPage() {
  const [orders, setOrders] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchOrders = async () => {
      setIsLoading(true);
      try {
        // const res = await fetch(`${import.meta.env.VITE_API_URL}/api/order`, {
        //   credentials: 'include'
        // });
        // if (!res.ok) throw new Error();
        // const data = await res.json();

        const mockOrders = [
          {
            id: 1,
            customerId: 1,
            orderDate: '2026-06-01T10:30:00',
            status: 'Processing',
            items: [
              { id: 1, orderId: 1, productId: 1, quantity: 2, price: 149.00 },
              { id: 2, orderId: 1, productId: 2, quantity: 1, price: 120.00 }
            ]
          },
          {
            id: 2,
            customerId: 1,
            orderDate: '2026-05-28T14:10:00',
            status: 'Delivered',
            items: [
              { id: 3, orderId: 2, productId: 5, quantity: 1, price: 45.00 }
            ]
          }
        ];

        setOrders(mockOrders);
        setIsLoading(false);
      } catch (err) {
        setError('Fout bij het ophalen van bestellingen.');
        setIsLoading(false);
      }
    };

    fetchOrders();
  }, []);

  const calculateTotal = (items) => {
    return items.reduce((sum, item) => sum + item.price * item.quantity, 0);
  };

  const formatDate = (dateString) => {
    return new Date(dateString).toLocaleDateString('nl-NL', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  };

  return (
    <div className="min-h-screen bg-gray-50 font-sans">
      <Navbar />

      <div className="max-w-5xl mx-auto px-6 py-10">
        <h1 className="text-2xl font-bold mb-6">Mijn bestellingen</h1>

        {isLoading ? (
          <div className="text-gray-500">Bestellingen laden...</div>
        ) : error ? (
          <div className="text-red-500">{error}</div>
        ) : orders.length === 0 ? (
          <div className="text-gray-500">Je hebt nog geen bestellingen.</div>
        ) : (
          <div className="space-y-6">
            {orders.map((order) => (
              <div
                key={order.id}
                className="bg-white shadow-sm p-5 rounded-md"
              >
                {/* ORDER HEADER */}
                <div className="flex justify-between items-center mb-4">
                  <div>
                    <p className="font-semibold">
                      Bestelling #{order.id}
                    </p>
                    <p className="text-sm text-gray-500">
                      {formatDate(order.orderDate)}
                    </p>
                  </div>

                  <span
                    className={`text-sm px-3 py-1 rounded-full ${
                      order.status === 'Delivered'
                        ? 'bg-green-100 text-green-700'
                        : order.status === 'Cancelled'
                        ? 'bg-red-100 text-red-600'
                        : 'bg-yellow-100 text-yellow-700'
                    }`}
                  >
                    {order.status}
                  </span>
                </div>

                {/* ITEMS */}
                <div className="border-t pt-4 space-y-2">
                  {order.items.map((item) => (
                    <div
                      key={item.id}
                      className="flex justify-between text-sm"
                    >
                      <div>
                        <p className="font-medium">
                          Product #{item.productId}
                        </p>
                        <p className="text-gray-500">
                          Aantal: {item.quantity}
                        </p>
                      </div>

                      <div className="text-right">
                        € {(item.price * item.quantity).toFixed(2)}
                      </div>
                    </div>
                  ))}
                </div>

                {/* TOTAL */}
                <div className="border-t mt-4 pt-4 flex justify-between font-semibold">
                  <span>Totaal</span>
                  <span>
                    € {calculateTotal(order.items).toFixed(2)}
                  </span>
                </div>

                {/* ACTIONS */}
                <div className="mt-4 flex gap-3">
                  <Link
                    to={`/order/${order.id}`}
                    className="text-blue-600 text-sm hover:underline"
                  >
                    Bekijk details
                  </Link>

                  {order.status === 'Processing' && (
                    <button className="text-red-500 text-sm hover:underline">
                      Annuleren
                    </button>
                  )}
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}