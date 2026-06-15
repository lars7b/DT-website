import React, { useState, useEffect, useMemo } from "react";
import { Link, useSearchParams } from "react-router-dom";
import Navbar from "../components/Navbar";
import { useAuth } from "../context/AuthContext";
import { categoryPlaceholders } from "../data/categoryPlaceholders";
import { useQuery } from "@tanstack/react-query";

export default function ProductsPage() {
  const { token, isLoggedIn } = useAuth();
  const baseUrl = import.meta.env.VITE_API_URL;

  const [categories, setCategories] = useState([]);
  const [searchParams] = useSearchParams();
  const [searchInput, setSearchInput] = useState("");
  const [search, setSearch] = useState("");

  const [page, setPage] = useState(1);
  const pageSize = 20;

  const subcategoryId = searchParams.get("subcategoryId");

  const [sortOrder, setSortOrder] = useState("price_asc");
  const [selectedCategory, setSelectedCategory] = useState(null);

  const [minPrice, setMinPrice] = useState(3);
  const [maxPrice, setMaxPrice] = useState(9585);

  const [customerId, setCustomerId] = useState(null);
  const [favoriteIds, setFavoriteIds] = useState(new Set());

  // -------------------------
  // PRODUCTS (React Query)
  // -------------------------
  const {
    data: products = [],
    isLoading,
    error,
  } = useQuery({
    queryKey: [
      "products",
      {
        sortOrder,
        selectedCategory,
        subcategoryId,
        minPrice,
        maxPrice,
        search,
        page,
      },
    ],
    queryFn: async () => {
      const params = new URLSearchParams();

      params.append("sort", sortOrder);

      if (selectedCategory) params.append("categoryId", selectedCategory);

      if (minPrice != null) params.append("minPrice", minPrice);

      if (maxPrice != null) params.append("maxPrice", maxPrice);

      if (search) params.append("search", search);
      if (subcategoryId) {
        params.append("subcategoryId", subcategoryId);
      }
      params.append("offset", (page - 1) * pageSize);

      params.append("limit", pageSize);

      const res = await fetch(`${baseUrl}/products?${params.toString()}`);

      if (!res.ok) throw new Error("Fout bij ophalen producten");

      return res.json();
    },
    staleTime: 1000 * 60 * 5,
  });

  // -------------------------
  // CATEGORIES
  // -------------------------
  useEffect(() => {
    const fetchCategories = async () => {
      const res = await fetch(`${baseUrl}/categories`);
      const data = await res.json();
      setCategories(data);
    };

    fetchCategories();
  }, [baseUrl]);

  // -------------------------
  // FAVORITES
  // -------------------------
  useEffect(() => {
    if (!token) return;

    const load = async () => {
      const profile = await fetch(`${baseUrl}/customer/me`, {
        headers: { Authorization: `Bearer ${token}` },
      }).then((r) => r.json());

      setCustomerId(profile.id);

      const favorites = await fetch(
        `${baseUrl}/customers/${profile.id}/favorites`,
        {
          headers: { Authorization: `Bearer ${token}` },
        },
      ).then((r) => r.json());

      setFavoriteIds(new Set((favorites || []).map((f) => f.productId)));
    };

    load();
  }, [token, baseUrl]);

  // -------------------------
  // FAVORITE TOGGLE
  // -------------------------
  const handleToggleFavorite = async (e, productId) => {
    e.preventDefault();

    if (!token || !customerId) return;

    const isFav = favoriteIds.has(productId);

    await fetch(`${baseUrl}/customers/${customerId}/favorites/${productId}`, {
      method: isFav ? "DELETE" : "POST",
      headers: { Authorization: `Bearer ${token}` },
    });

    setFavoriteIds((prev) => {
      const next = new Set(prev);
      isFav ? next.delete(productId) : next.add(productId);
      return next;
    });
  };

  // -------------------------
  // CATEGORY FILTER
  // -------------------------
  const handleCategoryFilterChange = (id) => {
    setPage(1);
    setSelectedCategory((prev) => (prev === id ? null : id));
  };
  // -------------------------
  // SEARCH TIMER
  // -------------------------
  useEffect(() => {
    const timeout = setTimeout(() => {
      setSearch(searchInput);
    }, 500);

    return () => clearTimeout(timeout);
  }, [searchInput]);

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar />

      <div className="flex px-16 py-8 max-w-7xl mx-auto gap-8">
        {/* FILTERS */}
        <aside className="w-64">
          <h2 className="font-bold mb-4">Filters</h2>

          {/* CATEGORY */}
          <div className="mb-6">
            {categories.map((cat) => (
              <label key={cat.id} className="block text-sm">
                <input
                  type="checkbox"
                  checked={selectedCategory === cat.id}
                  onChange={() => handleCategoryFilterChange(cat.id)}
                />
                <span className="ml-2">{cat.name}</span>
              </label>
            ))}
          </div>

          {/* PRICE */}
          <div className="mb-6">
            <h3 className="font-semibold mb-2">Prijs</h3>

            {/* MIN */}
            <label className="text-xs text-gray-500">Min: €{minPrice}</label>
            <input
              type="range"
              min={0}
              max={5000}
              value={minPrice}
              onChange={(e) => {
                const value = Number(e.target.value);
                setPage(1);
                if (value <= maxPrice) setMinPrice(value);
              }}
              className="w-full"
            />

            {/* MAX */}
            <label className="text-xs text-gray-500">Max: €{maxPrice}</label>
            <input
              type="range"
              min={0}
              max={5000}
              value={maxPrice}
              onChange={(e) => {
                const value = Number(e.target.value);
                setPage(1);
                if (value >= minPrice) setMaxPrice(value);
              }}
              className="w-full"
            />

            {/* VISUAL HELP */}
            <div className="text-sm text-gray-600 mt-2">
              €{minPrice} - €{maxPrice}
            </div>
          </div>
        </aside>

        {/* PRODUCTS */}
        <main className="flex-1">
          <div className="flex justify-between items-center mb-6">
            <input
              type="text"
              placeholder="Zoek producten..."
              value={searchInput}
              onChange={(e) => setSearchInput(e.target.value)}
              className="border rounded px-3 py-2"
            />
          </div>
          <select
            value={sortOrder}
            onChange={(e) => {
              setPage(1);
              setSortOrder(e.target.value);
            }}
            className="border rounded px-3 py-2"
          >
            <option value="price_asc">Prijs oplopend</option>
            <option value="price_desc">Prijs aflopend</option>
            <option value="newest">Nieuwste</option>
          </select>
          <div className="grid grid-cols-3 gap-6">
            {isLoading && <p>Loading...</p>}
            {error && <p>{error.message}</p>}

            {products.map((product) => {
              const image =
                categoryPlaceholders[product.categoryId] ?? "/placeholder.jpg";

              return (
                <Link
                  to={`/product/${product.id}`}
                  key={product.id}
                  className="bg-white p-4 block"
                >
                  <div className="relative h-40 mb-2">
                    <img src={image} className="w-full h-full object-cover" />

                    <button
                      onClick={(e) => handleToggleFavorite(e, product.id)}
                      className="absolute top-2 right-2"
                    >
                      {favoriteIds.has(product.id) ? "♥" : "♡"}
                    </button>
                  </div>

                  <h3>{product.name}</h3>

                  <p>
                    €{" "}
                    {Number(product.price).toLocaleString("nl-NL", {
                      minimumFractionDigits: 2,
                    })}
                  </p>
                </Link>
              );
            })}
          </div>
          <div className="flex justify-center gap-4 mt-8">
            <button
              disabled={page === 1}
              onClick={() => setPage((p) => p - 1)}
              className="px-4 py-2 border rounded disabled:opacity-50"
            >
              Vorige
            </button>

            <span className="flex items-center">Pagina {page}</span>

            <button
              onClick={() => setPage((p) => p + 1)}
              className="px-4 py-2 border rounded"
            >
              Volgende
            </button>
          </div>
        </main>
      </div>
    </div>
  );
}
