import React, { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import Navbar from '../components/Navbar';

export default function OrderDetailPage() {
  const { id } = useParams();

  const [order, setOrder] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchOrder = async () => {
      setIsLoading(true);
      try {
        // const res = await fetch(`${import.meta.env.VITE_API_URL}/api/order/${id}`, {
        //   credentials: 'include'
        // });
        // if (!res.ok) throw new Error();
        // const data = await res.json();

        const mockOrder = {
          id: id,
          customerId: 1,
          orderDate: '2026-06-01T10:30:00',
          status: 'Processing',
          items: [
            {
              id: 1,
              orderId: id,
              productId: 1,
              productName: 'Bank "Rotterdam"',
              price: 899.00,
              quantity: 1
            },
            {
              id: 2,
              orderId: id,
              productId: 2,
              productName: 'Eetkamerstoel "Maes"',
              price: 149.00,
              quantity: 2
            }
          ]
        };

        setOrder(mockOrder);
        setIsLoading(false);
      } catch (err) {
        setError('Fout bij het ophalen van bestelling.');
        setIsLoading(false);
      }
    };

    fetchOrder();
  }, [id]);

  const calculateTotal = () => {
    return order.items.reduce(
      (sum, item) => sum + item.price * item.quantity,
      0
    );
  };

  const formatDate = (date) => {
    return new Date(date).toLocaleDateString('nl-NL', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  };

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-50">
        <Navbar />
        <div className="text-center py-20 text-gray-500">
          Bestelling laden...
        </div>
      </div>
    );
  }

  if (error || !order) {
    return (
      <div className="min-h-screen bg-gray-50">
        <Navbar />
        <div className="text-center py-20 text-red-500">
          {error || 'Bestelling niet gevonden.'}
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 font-sans">
      <Navbar />

      <div className="max-w-5xl mx-auto px-6 py-10">
        {/* HEADER */}
        <div className="bg-white p-6 shadow-sm mb-6">
          <h1 className="text-2xl font-bold">
            Bestelling #{order.id}
          </h1>

          <div className="flex justify-between mt-2 text-sm text-gray-500">
            <span>{formatDate(order.orderDate)}</span>

            <span
              className={`px-3 py-1 rounded-full ${
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
        </div>

        {/* ITEMS */}
        <div className="bg-white p-6 shadow-sm space-y-4">
          <h2 className="font-semibold text-lg mb-4">
            Artikelen
          </h2>

          {order.items.map((item) => (
            <div
              key={item.id}
              className="flex justify-between border-b pb-3"
            >
              <div>
                <p className="font-medium">
                  {item.productName || `Product #${item.productId}`}
                </p>
                <p className="text-sm text-gray-500">
                  Aantal: {item.quantity}
                </p>

                <Link
                  to={`/product/${item.productId}`}
                  className="text-blue-600 text-sm hover:underline"
                >
                  Bekijk product
                </Link>
              </div>

              <div className="text-right font-medium">
                € {(item.price * item.quantity).toFixed(2)}
              </div>
            </div>
          ))}

          {/* TOTAL */}
          <div className="flex justify-between pt-4 font-bold">
            <span>Totaal</span>
            <span>€ {calculateTotal().toFixed(2)}</span>
          </div>
        </div>

        {/* ACTIONS */}
        <div className="mt-6 flex gap-4">
          <Link
            to="/orders"
            className="text-blue-600 hover:underline"
          >
            ← Terug naar bestellingen
          </Link>

          {order.status === 'Processing' && (
            <button className="text-red-500 hover:underline">
              Annuleren
            </button>
          )}
        </div>
      </div>
    </div>
  );
}