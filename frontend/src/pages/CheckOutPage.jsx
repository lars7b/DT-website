import React, { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import Navbar from "../components/Navbar";

export default function CheckOutPage() {
  //   const { orderId } = useParams();
  const navigate = useNavigate();

  const [amount, setAmount] = useState("");
  const [paymentMethod, setPaymentMethod] = useState("iDEAL");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState("");

  const handlePayment = async (e) => {
    e.preventDefault();

    setError("");
    setIsSubmitting(true);

    try {
      const token = localStorage.getItem("token");

      const res = await fetch(`${API_URL}/Payment`, {
        method: "POST",
        headers: {
          Authorization: `Bearer ${token}`,
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          paymentMethod,
        }),
      });

      if (!res.ok) {
        throw new Error(`HTTP ${res.status}`);
      }

      navigate("/bestellingen");
    } catch (err) {
      console.error(err);
      setError("Betaling mislukt.");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 font-sans">
      <Navbar />

      <div className="max-w-xl mx-auto px-6 py-10">
        <div className="bg-white shadow-sm p-6 rounded">
          <h1 className="text-2xl font-bold mb-6">Afrekenen</h1>

          <p className="text-gray-600 mb-4">Bestelling #{orderId}</p>

          <form onSubmit={handlePayment} className="space-y-4">
            <div>
              <label className="block mb-1 font-medium">Bedrag (€)</label>

              <p>Totaal: € {order.totalAmount}</p>
            </div>

            <div>
              <label className="block mb-1 font-medium">Betaalmethode</label>

              <select
                value={paymentMethod}
                onChange={(e) => setPaymentMethod(e.target.value)}
                className="w-full border p-2 rounded"
              >
                <option value="iDEAL">iDEAL</option>
                <option value="Credit Card">Credit Card</option>
                <option value="PayPal">PayPal</option>
                <option value="Bank Transfer">Bank Transfer</option>
              </select>
            </div>

            {error && <div className="text-red-500">{error}</div>}

            <button
              type="submit"
              disabled={isSubmitting}
              className="w-full bg-orange-500 hover:bg-orange-600 text-white font-bold py-3 rounded disabled:opacity-50"
            >
              {isSubmitting ? "Betaling verwerken..." : "Betaal nu"}
            </button>
          </form>
        </div>
      </div>
    </div>
  );
}
