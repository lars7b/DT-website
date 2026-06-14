// src/ProfilePage.jsx
import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import Navbar from "../components/Navbar";

export default function ProfilePage() {
  const { token, logout } = useAuth();
  const navigate = useNavigate();

  // Tab state
  const [activeTab, setActiveTab] = useState("gegevens");

  // Form states
  const [profileData, setProfileData] = useState({
    firstName: "",
    lastName: "",
    phone: "",
    address: "",
  });
  const [emailData, setEmailData] = useState({ newEmail: "", password: "" });
  const [passwordData, setPasswordData] = useState({
    currentPassword: "",
    newPassword: "",
  });
  const [deletePassword, setDeletePassword] = useState("");

  const [customerId, setCustomerId] = useState(null);
  const [orders, setOrders] = useState([]);
  const [ordersLoading, setOrdersLoading] = useState(false);
  const [ordersError, setOrdersError] = useState("");

  // Status states
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [message, setMessage] = useState({ type: "", text: "" });

  const baseUrl = import.meta.env.VITE_API_URL;

  // Haal klantgegevens op bij het laden van de component
  useEffect(() => {
    if (!token) {
      navigate("/login");
      return;
    }

    const fetchProfile = async () => {
      try {
        const response = await fetch(`${baseUrl}/customer/me`, {
          headers: { Authorization: `Bearer ${token}` },
        });

        if (response.ok) {
          const data = await response.json();
          setProfileData({
            firstName: data.firstName || "",
            lastName: data.lastName || "",
            phone: data.phone || "",
            address: data.address || "",
          });
          setCustomerId(data.id ?? null);
        } else {
          throw new Error("Kon profiel niet ophalen.");
        }
      } catch (error) {
        setMessage({ type: "error", text: error.message });
      } finally {
        setIsLoading(false);
      }
    };

    fetchProfile();
  }, [token, baseUrl, navigate]);

  useEffect(() => {
    if (!token || !customerId) return;

    const controller = new AbortController();

    const fetchOrders = async () => {
      try {
        setOrdersLoading(true);
        setOrdersError("");

        const response = await fetch(
          `${baseUrl}/customers/${customerId}/orders`,
          {
            headers: { Authorization: `Bearer ${token}` },
            signal: controller.signal,
          },
        );

        if (!response.ok) {
          throw new Error("Kon bestelgeschiedenis niet ophalen.");
        }

        const data = await response.json();
        setOrders(Array.isArray(data) ? data : []);
      } catch (error) {
        if (error.name !== "AbortError") {
          setOrdersError(error.message);
        }
      } finally {
        setOrdersLoading(false);
      }
    };

    fetchOrders();
    return () => controller.abort();
  }, [token, customerId, baseUrl]);

  const formatCurrency = (value) =>
    Number(value || 0).toLocaleString("nl-NL", {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    });

  // Handler: Update Persoonsgegevens (PATCH /api/customer/me)
  const handleUpdateProfile = async (e) => {
    e.preventDefault();
    setIsSaving(true);
    setMessage({ type: "", text: "" });

    try {
      const response = await fetch(`${baseUrl}/customer/me`, {
        method: "PATCH",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify(profileData),
      });

      const data = await response.json();
      if (!response.ok) throw new Error(data.message || "Fout bij opslaan.");

      setMessage({ type: "success", text: "Profiel succesvol bijgewerkt!" });
    } catch (error) {
      setMessage({ type: "error", text: error.message });
    } finally {
      setIsSaving(false);
    }
  };

  // Handler: Update E-mail (PUT /api/auth/change-email)
  const handleUpdateEmail = async (e) => {
    e.preventDefault();
    setIsSaving(true);
    setMessage({ type: "", text: "" });

    try {
      const response = await fetch(`${baseUrl}/auth/change-email`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify(emailData),
      });

      const data = await response.json();
      if (!response.ok)
        throw new Error(data.message || "Fout bij wijzigen e-mail.");

      setMessage({
        type: "success",
        text: "E-mailadres succesvol gewijzigd! Log opnieuw in.",
      });
      setTimeout(() => logout(), 3000); // Log gebruiker uit na succesvolle wijziging
    } catch (error) {
      setMessage({ type: "error", text: error.message });
    } finally {
      setIsSaving(false);
    }
  };

  // Handler: Update Wachtwoord (PUT /api/auth/change-password)
  const handleUpdatePassword = async (e) => {
    e.preventDefault();
    setIsSaving(true);
    setMessage({ type: "", text: "" });

    try {
      const response = await fetch(`${baseUrl}/auth/change-password`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify(passwordData),
      });

      const data = await response.json();
      if (!response.ok)
        throw new Error(data.message || "Fout bij wijzigen wachtwoord.");

      setMessage({ type: "success", text: "Wachtwoord succesvol gewijzigd!" });
      setPasswordData({ currentPassword: "", newPassword: "" });
    } catch (error) {
      setMessage({ type: "error", text: error.message });
    } finally {
      setIsSaving(false);
    }
  };

  const handleDeleteAccount = async (e) => {
    e.preventDefault(); // Voorkom page reload als het in een form zit

    if (!deletePassword) {
      setMessage({
        type: "error",
        text: "Vul je huidige wachtwoord in om je account te verwijderen.",
      });
      return;
    }

    if (
      !window.confirm(
        "Weet je zeker dat je je account wilt verwijderen? Dit kan niet ongedaan worden gemaakt.",
      )
    )
      return;

    try {
      const response = await fetch(`${baseUrl}/customer/me`, {
        method: "PUT", // Zorg dat dit overeenkomt met je backend (PUT of DELETE)
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        // Stuur het wachtwoord mee in de body (zorg dat je backend DTO dit verwacht)
        body: JSON.stringify({ password: deletePassword }),
      });

      if (!response.ok) {
        const data = await response.json();
        throw new Error(
          data.message ||
            "Fout bij verwijderen account. Controleer je wachtwoord.",
        );
      }

      logout(); // Account is succesvol verwijderd, log de gebruiker uit
    } catch (error) {
      setMessage({ type: "error", text: error.message });
    }
  };

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-50 font-sans">
        <Navbar />
        <div className="text-center py-20 text-gray-500">Gegevens laden...</div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 font-sans">
      <Navbar />

      <div className="max-w-5xl mx-auto px-8 py-12 flex gap-8">
        {/* Zijbalk Navigatie */}
        <aside className="w-64 flex-shrink-0">
          <div className="bg-white p-6 shadow-sm rounded border border-gray-200">
            <h2 className="font-bold text-lg mb-6">Mijn Account</h2>
            <ul className="space-y-3 text-sm">
              <li>
                <button
                  onClick={() => {
                    setActiveTab("gegevens");
                    setMessage({ type: "", text: "" });
                  }}
                  className={`w-full text-left font-medium ${activeTab === "gegevens" ? "text-blue-600" : "text-gray-600 hover:text-black"}`}
                >
                  Persoonsgegevens
                </button>
              </li>
              <li>
                <button
                  onClick={() => {
                    setActiveTab("beveiliging");
                    setMessage({ type: "", text: "" });
                  }}
                  className={`w-full text-left font-medium ${activeTab === "beveiliging" ? "text-blue-600" : "text-gray-600 hover:text-black"}`}
                >
                  E-mail & Wachtwoord
                </button>
              </li>
              <li>
                <button
                  onClick={() => {
                    setActiveTab("bestellingen");
                    setMessage({ type: "", text: "" });
                  }}
                  className={`w-full text-left font-medium ${activeTab === "bestellingen" ? "text-blue-600" : "text-gray-600 hover:text-black"}`}
                >
                  Bestelgeschiedenis
                </button>
              </li>
            </ul>
          </div>
        </aside>

        {/* Hoofd Content Area */}
        <main className="flex-1 bg-white p-8 shadow-sm rounded border border-gray-200">
          {message.text && (
            <div
              className={`p-4 mb-6 rounded text-sm font-semibold border ${message.type === "error" ? "bg-red-50 text-red-600 border-red-200" : "bg-green-50 text-green-600 border-green-200"}`}
            >
              {message.text}
            </div>
          )}

          {activeTab === "gegevens" && (
            <section>
              <h1 className="text-2xl font-bold mb-6">Persoonsgegevens</h1>
              <form
                onSubmit={handleUpdateProfile}
                className="space-y-5 max-w-md"
              >
                <div className="flex space-x-4">
                  <div className="flex-1">
                    <label className="block text-sm font-semibold mb-1">
                      Voornaam
                    </label>
                    <input
                      type="text"
                      value={profileData.firstName}
                      onChange={(e) =>
                        setProfileData({
                          ...profileData,
                          firstName: e.target.value,
                        })
                      }
                      className="w-full border border-gray-300 p-2 rounded focus:outline-none focus:border-blue-600"
                    />
                  </div>
                  <div className="flex-1">
                    <label className="block text-sm font-semibold mb-1">
                      Achternaam
                    </label>
                    <input
                      type="text"
                      value={profileData.lastName}
                      onChange={(e) =>
                        setProfileData({
                          ...profileData,
                          lastName: e.target.value,
                        })
                      }
                      className="w-full border border-gray-300 p-2 rounded focus:outline-none focus:border-blue-600"
                    />
                  </div>
                </div>
                <div>
                  <label className="block text-sm font-semibold mb-1">
                    Telefoonnummer
                  </label>
                  <input
                    type="text"
                    value={profileData.phone}
                    onChange={(e) =>
                      setProfileData({ ...profileData, phone: e.target.value })
                    }
                    className="w-full border border-gray-300 p-2 rounded focus:outline-none focus:border-blue-600"
                  />
                </div>
                <div>
                  <label className="block text-sm font-semibold mb-1">
                    Adres
                  </label>
                  <input
                    type="text"
                    value={profileData.address}
                    onChange={(e) =>
                      setProfileData({
                        ...profileData,
                        address: e.target.value,
                      })
                    }
                    className="w-full border border-gray-300 p-2 rounded focus:outline-none focus:border-blue-600"
                  />
                </div>
                <button
                  type="submit"
                  disabled={isSaving}
                  className="bg-blue-600 hover:bg-blue-700 text-white font-bold py-2 px-6 rounded transition"
                >
                  {isSaving ? "Opslaan..." : "Wijzigingen Opslaan"}
                </button>
              </form>
              <hr className="my-10 border-gray-200" />
              <div>
                <h1 className="text-2xl font-bold mb-6 text-red-600">
                  Account Verwijderen
                </h1>
                <p className="text-sm text-gray-600 mb-4">
                  Let op: Als je je account verwijdert, worden al je gegevens
                  permanent gewist. Dit kan niet ongedaan worden gemaakt.
                </p>
                <form
                  onSubmit={handleDeleteAccount}
                  className="space-y-4 max-w-md bg-red-50 p-4 rounded border border-red-200"
                >
                  <div>
                    <label className="block text-sm font-semibold mb-1 text-red-800">
                      Bevestig met je wachtwoord
                    </label>
                    <input
                      type="password"
                      value={deletePassword}
                      onChange={(e) => setDeletePassword(e.target.value)}
                      required
                      placeholder="Huidig wachtwoord"
                      className="w-full border border-gray-300 p-2 rounded focus:outline-none focus:border-red-500"
                    />
                  </div>
                  <button
                    type="submit"
                    disabled={isSaving}
                    className="bg-red-600 hover:bg-red-700 text-white font-bold py-2 px-6 rounded transition"
                  >
                    Mijn Account Definitief Verwijderen
                  </button>
                </form>
              </div>
            </section>
          )}

          {activeTab === "beveiliging" && (
            <section className="space-y-10">
              {/* E-mail Formulier */}
              <div>
                <h1 className="text-2xl font-bold mb-6">
                  E-mailadres Wijzigen
                </h1>
                <form
                  onSubmit={handleUpdateEmail}
                  className="space-y-4 max-w-md"
                >
                  <div>
                    <label className="block text-sm font-semibold mb-1">
                      Nieuw E-mailadres
                    </label>
                    <input
                      type="email"
                      value={emailData.newEmail}
                      required
                      onChange={(e) =>
                        setEmailData({ ...emailData, newEmail: e.target.value })
                      }
                      className="w-full border border-gray-300 p-2 rounded focus:outline-none focus:border-blue-600"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-semibold mb-1">
                      Huidig Wachtwoord (ter controle)
                    </label>
                    <input
                      type="password"
                      value={emailData.password}
                      required
                      onChange={(e) =>
                        setEmailData({ ...emailData, password: e.target.value })
                      }
                      className="w-full border border-gray-300 p-2 rounded focus:outline-none focus:border-blue-600"
                    />
                  </div>
                  <button
                    type="submit"
                    disabled={isSaving}
                    className="bg-gray-800 hover:bg-black text-white font-bold py-2 px-6 rounded transition"
                  >
                    E-mailadres Bijwerken
                  </button>
                </form>
              </div>

              <hr className="border-gray-200" />

              {/* Wachtwoord Formulier */}
              <div>
                <h1 className="text-2xl font-bold mb-6">Wachtwoord Wijzigen</h1>
                <form
                  onSubmit={handleUpdatePassword}
                  className="space-y-4 max-w-md"
                >
                  <div>
                    <label className="block text-sm font-semibold mb-1">
                      Huidig Wachtwoord
                    </label>
                    <input
                      type="password"
                      value={passwordData.currentPassword}
                      required
                      onChange={(e) =>
                        setPasswordData({
                          ...passwordData,
                          currentPassword: e.target.value,
                        })
                      }
                      className="w-full border border-gray-300 p-2 rounded focus:outline-none focus:border-blue-600"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-semibold mb-1">
                      Nieuw Wachtwoord
                    </label>
                    <input
                      type="password"
                      value={passwordData.newPassword}
                      required
                      onChange={(e) =>
                        setPasswordData({
                          ...passwordData,
                          newPassword: e.target.value,
                        })
                      }
                      className="w-full border border-gray-300 p-2 rounded focus:outline-none focus:border-blue-600"
                    />
                  </div>
                  <button
                    type="submit"
                    disabled={isSaving}
                    className="bg-gray-800 hover:bg-black text-white font-bold py-2 px-6 rounded transition"
                  >
                    Wachtwoord Bijwerken
                  </button>
                </form>
              </div>
            </section>
          )}

          {activeTab === "bestellingen" && (
            <section>
              <h1 className="text-2xl font-bold mb-6">Bestelgeschiedenis</h1>

              {ordersLoading ? (
                <div className="text-center py-10 text-gray-500">
                  Bestellingen laden...
                </div>
              ) : ordersError ? (
                <div className="text-center py-10 text-red-500">
                  {ordersError}
                </div>
              ) : orders.length === 0 ? (
                <div className="text-center py-10 text-gray-500">
                  Nog geen bestellingen gevonden.
                </div>
              ) : (
                <div className="space-y-6">
                  {orders.map((order) => {
                    const itemsTotal = (order.items || []).reduce(
                      (sum, item) =>
                        sum + item.quantity * Number(item.unitPrice || 0),
                      0,
                    );

                    return (
                      <div
                        key={order.orderId}
                        className="border border-gray-200 rounded p-5"
                      >
                        <div className="flex items-center justify-between mb-4">
                          <div>
                            <h2 className="text-lg font-bold">
                              Bestelling #{order.orderId}
                            </h2>
                            <p className="text-sm text-gray-500">
                              {new Date(order.orderDate).toLocaleDateString(
                                "nl-NL",
                              )}
                            </p>
                          </div>
                          <span className="text-sm font-semibold text-gray-600">
                            {order.status || "Onbekend"}
                          </span>
                        </div>

                        <div className="space-y-3">
                          {!order.items || order.items.length === 0 ? (
                            <p className="text-sm text-gray-500">
                              Geen items gevonden.
                            </p>
                          ) : (
                            order.items.map((item) => (
                              <div
                                key={item.orderItemId}
                                className="flex items-center justify-between text-sm"
                              >
                                <div>
                                  <p className="font-semibold">
                                    {item.productName}
                                  </p>
                                  <p className="text-gray-500">
                                    Aantal: {item.quantity}
                                  </p>
                                </div>
                                <p className="font-semibold">
                                  € {formatCurrency(item.unitPrice)}
                                </p>
                              </div>
                            ))
                          )}
                        </div>

                        <div className="mt-4 pt-4 border-t border-gray-200 flex items-center justify-between">
                          <span className="text-sm text-gray-500">Totaal</span>
                          <span className="font-bold">
                            € {formatCurrency(itemsTotal)}
                          </span>
                        </div>
                      </div>
                    );
                  })}
                </div>
              )}
            </section>
          )}
        </main>
      </div>
    </div>
  );
}
