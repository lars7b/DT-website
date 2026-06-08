import React, { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import Navbar from "../components/Navbar";

export default function OrderDetailPage() {
  const { id } = useParams();

  const [order, setOrder] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchOrder = async () => {
      setIsLoading(true);

      try {
        const token = localStorage.getItem("token");

        const res = await fetch(`${import.meta.env.VITE_API_URL}/order/${id}`, {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        });

        if (res.status === 404) {
          setError("Bestelling niet gevonden.");
          return;
        }

        if (!res.ok) {
          throw new Error(`HTTP ${res.status}`);
        }

        const data = await res.json();

        setOrder(data);
      } catch (err) {
        console.error(err);
        setError("Fout bij het ophalen van bestelling.");
      } finally {
        setIsLoading(false);
      }
    };

    fetchOrder();
  }, [id]);
  const handleCancelOrder = async () => {
    try {
      const token = localStorage.getItem("token");

      const res = await fetch(
        `${import.meta.env.VITE_API_URL}/order/${order.id}/cancel`,
        {
          method: "PUT",
          headers: {
            Authorization: `Bearer ${token}`,
          },
        },
      );

      if (!res.ok) {
        throw new Error();
      }

      setOrder((prev) => ({
        ...prev,
        status: "Cancelled",
      }));
    } catch (err) {
      console.error(err);
      alert("Bestelling kon niet worden geannuleerd.");
    }
  };

  const calculateTotal = () => {
    return order.items.reduce(
      (sum, item) => sum + item.price * item.quantity,
      0,
    );
  };
  const [paymentMethod, setPaymentMethod] = useState("iDEAL");

const handlePay = async () => {
  try {
    const token = localStorage.getItem("token");

    const res = await fetch(
      `${import.meta.env.VITE_API_URL}/Payment`,
      {
        method: "POST",
        headers: {
          Authorization: `Bearer ${token}`,
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          paymentMethod,
          orderId: order.id,
        }),
      }
    );

    if (!res.ok) {
      throw new Error();
    }

    // alert("Betaling aangemaakt");

    // refresh order
  } catch (err) {
    console.error(err);
    alert("Betaling mislukt");
  }
};
  const formatDate = (date) => {
    return new Date(date).toLocaleDateString("nl-NL", {
      year: "numeric",
      month: "long",
      day: "numeric",
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
          {error || "Bestelling niet gevonden."}
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
          <h1 className="text-2xl font-bold">Bestelling #{order.id}</h1>

          <div className="flex justify-between mt-2 text-sm text-gray-500">
            <span>{formatDate(order.orderDate)}</span>

            <span
              className={`px-3 py-1 rounded-full ${
                order.status === "Delivered"
                  ? "bg-green-100 text-green-700"
                  : order.status === "Cancelled"
                    ? "bg-red-100 text-red-600"
                    : "bg-yellow-100 text-yellow-700"
              }`}
            >
              {order.status}
            </span>
          </div>
        </div>

        {/* ITEMS */}
        <div className="bg-white p-6 shadow-sm space-y-4">
          <h2 className="font-semibold text-lg mb-4">Artikelen</h2>

          {order.items.map((item) => (
            <div key={item.id} className="flex justify-between border-b pb-3">
              <div>
                <p className="font-medium">
                  {item.productName || `Product #${item.productId}`}
                </p>
                <p className="text-sm text-gray-500">Aantal: {item.quantity}</p>

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
          <Link to="/bestellingen" className="text-blue-600 hover:underline">
            ← Terug naar bestellingen
          </Link>

          {order.status === "Processing" && (
            <button
              onClick={handleCancelOrder}
              className="text-red-500 hover:underline"
            >
              Annuleren
            </button>
          )}
          {/* <div className="mt-6 bg-white p-6 shadow-sm">
            <h2 className="font-semibold text-lg mb-4">Betaling</h2>

            <p className="mb-4">Te betalen: € {calculateTotal().toFixed(2)}</p>

            <select
              value={paymentMethod}
              onChange={(e) => setPaymentMethod(e.target.value)}
              className="border p-2 rounded"
            >
              <option value="iDEAL">iDEAL</option>
              <option value="Credit Card">Credit Card</option>
              <option value="PayPal">PayPal</option>
            </select>

            <button
              onClick={handlePay}
              className="ml-4 bg-green-600 text-white px-4 py-2 rounded"
            >
              Betaal
            </button>
          </div> */}
        </div>
      </div>
    </div>
  );
}
