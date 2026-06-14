import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import Navbar from "../components/Navbar";

export default function CheckOutPage() {
  const navigate = useNavigate();

  const [paymentInfo, setPaymentInfo] = useState(null);
  const [paymentMethod, setPaymentMethod] = useState("iDEAL");
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    const fetchPaymentInfo = async () => {
      try {
        const token = localStorage.getItem("token");

        const res = await fetch(
          `${import.meta.env.VITE_API_URL}/Payment`,
          {
            headers: {
              Authorization: `Bearer ${token}`,
            },
          }
        );

        if (!res.ok) {
          throw new Error(`HTTP ${res.status}`);
        }

        const data = await res.json();

        setPaymentInfo(data);
      } catch (err) {
        console.error(err);
        setError("Kon betaalinformatie niet ophalen.");
      } finally {
        setIsLoading(false);
      }
    };

    fetchPaymentInfo();
  }, []);

  const handlePayment = async (e) => {
    e.preventDefault();

    setError("");
    setIsSubmitting(true);

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
          }),
        }
      );

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
        <div className="bg-white shadow-sm rounded p-6">
          <h1 className="text-2xl font-bold mb-6">Afrekenen</h1>

          {isLoading ? (
            <p className="text-gray-500">
              Betaalinformatie laden...
            </p>
          ) : error && !paymentInfo ? (
            <p className="text-red-500">{error}</p>
          ) : (
            <>
              <div className="mb-6">
                <p className="text-gray-600">
                  Bestelling #{paymentInfo?.orderId}
                </p>

                <p className="text-lg font-semibold mt-2">
                  Totaal: €{" "}
                  {paymentInfo?.amount?.toFixed(2)}
                </p>
              </div>

              <form
                onSubmit={handlePayment}
                className="space-y-4"
              >
                <div>
                  <label className="block mb-1 font-medium">
                    Betaalmethode
                  </label>

                  <select
                    value={paymentMethod}
                    onChange={(e) =>
                      setPaymentMethod(e.target.value)
                    }
                    className="w-full border p-2 rounded"
                  >
                    <option value="iDEAL">iDEAL</option>
                    <option value="Credit Card">
                      Credit Card
                    </option>
                    <option value="PayPal">PayPal</option>
                    <option value="Bank Transfer">
                      Bank Transfer
                    </option>
                  </select>
                </div>

                {error && (
                  <div className="text-red-500">
                    {error}
                  </div>
                )}

                <button
                  type="submit"
                  disabled={isSubmitting}
                  className="w-full bg-orange-500 hover:bg-orange-600 text-white font-bold py-3 rounded disabled:opacity-50"
                >
                  {isSubmitting
                    ? "Betaling verwerken..."
                    : "Betaal nu"}
                </button>
              </form>
            </>
          )}
        </div>
      </div>
    </div>
  );
}