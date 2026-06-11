// ProductDetailPage.jsx
import React, { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import Navbar from '../components/Navbar';
import { useAuth } from '../context/AuthContext';

export default function ProductDetailPage() {
  const { token, isLoggedIn } = useAuth();
  const baseUrl = import.meta.env.VITE_API_URL;

  // useParams haalt het product ID uit de URL, bijv. /product/1
  // Zorg ervoor dat de route in App.jsx is ingesteld als:
  // <Route path="/product/:id" element={<ProductDetailPage />} />
  const { id } = useParams();

  // Haal login-status en token op uit de AuthContext.
  // isLoggedIn gebruiken we om te bepalen of iemand een review mag plaatsen.
  // token kan later gebruikt worden voor beveiligde POST requests.
  const { isLoggedIn, token } = useAuth();

  // Zelfde stijl als ProfilePage: basis-url apart opslaan.
  // .env voorbeeld: VITE_API_URL=https://localhost:7042/api
  const baseUrl = import.meta.env.VITE_API_URL;

  // State variabelen voor het opslaan van de productgegevens en laad-statussen
  const [product, setProduct] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);

  // State variabelen voor reviews
  const [reviews, setReviews] = useState([]);
  const [isLoadingReviews, setIsLoadingReviews] = useState(true);
  const [reviewError, setReviewError] = useState(null);
  const [reviewSuccess, setReviewSuccess] = useState(null);

  // State variabelen voor het reviewformulier
  const [rating, setRating] = useState(5);
  const [comment, setComment] = useState('');
  const [isSubmittingReview, setIsSubmittingReview] = useState(false);
  const [customerId, setCustomerId] = useState(null);
  const [isFavorite, setIsFavorite] = useState(false);

  useEffect(() => {
    // Backend request voor het ophalen van specifieke product details.
    // Endpoint uit ProductsController: GET /api/products/{id}
    const fetchProductDetails = async () => {
      try {
        setIsLoading(true);
        setError(null);

        const url = `${baseUrl}/products/${id}`;
        console.log('Product ophalen via:', url);

        const response = await fetch(url);

        console.log('Product response status:', response.status);

        if (!response.ok) {
          throw new Error(`Product niet gevonden. Status: ${response.status}`);
        }

        const data = await response.json();
        console.log('Product data:', data);

        // Omdat dimensions en material waarschijnlijk nog niet in jouw Product model/backend zitten,
        // voegen we hier tijdelijke fallback-waarden toe voor de bestaande UI.
        setProduct({
          ...data,
          dimensions: data.dimensions || 'Niet bekend',
          material: data.material || 'Niet bekend',
        });
      } catch (error) {
        console.error('Fout bij ophalen product:', error);
        setError(error.message || 'Fout bij het ophalen van het product.');
      } finally {
        setIsLoading(false);
      }
    };

    if (baseUrl && id) {
      fetchProductDetails();
    } else {
      setError('VITE_API_URL of product-id ontbreekt.');
      setIsLoading(false);
    }
  }, [id, baseUrl]);

  // Backend request voor het ophalen van reviews van dit product.
  // Endpoint uit ReviewsController: GET /api/reviews/product/{productId}
  const fetchReviews = async () => {
    try {
      setIsLoadingReviews(true);

      const url = `${baseUrl}/reviews/product/${id}`;
      console.log('Reviews ophalen via:', url);

      const response = await fetch(url);

      console.log('Reviews response status:', response.status);

      if (!response.ok) {
        throw new Error(`Reviews konden niet worden opgehaald. Status: ${response.status}`);
      }

      const data = await response.json();
      console.log('Reviews data:', data);

      setReviews(data);
    } catch (error) {
      console.error('Fout bij ophalen reviews:', error);

      // Reviews zijn niet essentieel om de productpagina te tonen.
      // Daarom zetten we alleen een lege lijst in plaats van de hele pagina kapot te laten gaan.
      setReviews([]);
    } finally {
      setIsLoadingReviews(false);
    }
  };

  useEffect(() => {
    if (baseUrl && id) {
      fetchReviews();
    }
  }, [id, baseUrl]);

  useEffect(() => {
    if (!token) {
      setCustomerId(null);
      setIsFavorite(false);
      return;
    }

    const controller = new AbortController();

    const fetchCustomerAndFavorites = async () => {
      try {
        const profileResponse = await fetch(`${baseUrl}/customer/me`, {
          headers: { Authorization: `Bearer ${token}` },
          signal: controller.signal
        });

        if (!profileResponse.ok) {
          throw new Error('Kon profiel niet ophalen.');
        }

        const profile = await profileResponse.json();
        if (!profile?.id) {
          throw new Error('Klant id ontbreekt in profiel.');
        }

        setCustomerId(profile.id);

        const favoritesResponse = await fetch(
          `${baseUrl}/customers/${profile.id}/favorites`,
          { headers: { Authorization: `Bearer ${token}` }, signal: controller.signal }
        );

        if (favoritesResponse.ok) {
          const favorites = await favoritesResponse.json();
          const favoriteSet = new Set((favorites || []).map((fav) => fav.productId));
          setIsFavorite(favoriteSet.has(Number(id)));
        }
      } catch (err) {
        if (err.name !== 'AbortError') {
          console.error('Fout bij ophalen favorieten', err);
        }
      }
    };

    fetchCustomerAndFavorites();
    return () => controller.abort();
  }, [token, baseUrl, id]);

  // Handler voor het toevoegen aan de winkelwagen
  const handleAddToCart = () => {
    // Backend request moet hier later komen.
    // Voorbeeld endpoint: POST /api/cart/items
    // Verwachte payload: { cart_id: (uit user sessie), product_id: product.id, quantity: 1 }
    // De backend voegt dit toe aan de 'cart_items' tabel.
    console.log(`Product ${product.id} toegevoegd aan winkelwagen`);
  };

  // Handler voor het toevoegen aan favorieten
  const handleToggleFavorite = async () => {
    if (!isLoggedIn || !token || !customerId) {
      console.warn('Je moet ingelogd zijn om favorieten op te slaan.');
      return;
    }

    const endpoint = `${baseUrl}/customers/${customerId}/favorites/${product.id}`;
    try {
      const response = await fetch(endpoint, {
        method: isFavorite ? 'DELETE' : 'POST',
        headers: { Authorization: `Bearer ${token}` }
      });

      if (!response.ok && response.status !== 409) {
        throw new Error('Favoriet bijwerken mislukt.');
      }

      setIsFavorite((prev) => !prev);
    } catch (err) {
      console.error(err);
    }
  };

  // Handler voor het plaatsen van een review
  const handleSubmitReview = async (event) => {
    event.preventDefault();

    if (!comment.trim()) {
      setReviewError('Vul eerst een reviewtekst in.');
      return;
    }

    try {
      setIsSubmittingReview(true);
      setReviewError(null);
      setReviewSuccess(null);

      const newReview = {
        // Tijdelijk hardcoded. Later beter ophalen uit de ingelogde gebruiker.
        customerId: 1,
        productId: Number(id),
        rating: Number(rating),
        comment: comment.trim(),
      };

      const url = `${baseUrl}/reviews`;
      console.log('Review plaatsen via:', url);
      console.log('Nieuwe review:', newReview);

      const response = await fetch(url, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',

          // Als jouw ReviewsController later [Authorize] krijgt,
          // dan is deze Authorization header alvast voorbereid.
          ...(token ? { Authorization: `Bearer ${token}` } : {}),
        },
        body: JSON.stringify(newReview),
      });

      console.log('Review POST response status:', response.status);

      let data = null;
      try {
        data = await response.json();
      } catch {
        // Sommige backends sturen geen JSON terug bij een fout.
      }

      if (!response.ok) {
        throw new Error(data?.message || 'Fout bij het plaatsen van je review.');
      }

      setComment('');
      setRating(5);
      setReviewSuccess('Je review is geplaatst.');

      // Reviews opnieuw ophalen, zodat de nieuwe review direct zichtbaar wordt.
      await fetchReviews();
    } catch (error) {
      console.error('Fout bij plaatsen review:', error);
      setReviewError(error.message || 'Fout bij het plaatsen van je review.');
    } finally {
      setIsSubmittingReview(false);
    }
  };

  // UI weergave tijdens het laden
  if (isLoading) {
    return (
      <div className="page-container min-h-screen bg-gray-50 font-sans">
        <Navbar />
        <div className="loading-container text-center py-20 text-gray-500">
          Product laden...
        </div>
      </div>
    );
  }

  // UI weergave bij een foutmelding
  if (error || !product) {
    return (
      <div className="page-container min-h-screen bg-gray-50 font-sans">
        <Navbar />

        <div className="error-container text-center py-20 text-red-500">
          {error || 'Product niet gevonden.'}

          <p className="text-gray-500 text-sm mt-4">
            Controleer in de console welke URL wordt aangeroepen en welke statuscode terugkomt.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="page-container min-h-screen bg-gray-50 font-sans">
      <Navbar />

      <main className="max-w-6xl mx-auto px-16 py-12">
        <section className="product-details-wrapper flex gap-12 bg-white shadow-sm p-8">
          {/* Linkerzijde: Afbeeldingen galerij */}
          <div className="product-images-section w-1/2">
            {/* TODO: In de toekomst kunnen product afbeeldingen uit een aparte tabel ('product_images') gehaald worden */}
            <div className="main-image-container w-full h-96 bg-gray-100 mb-4 flex items-center justify-center text-gray-400">
              [Hoofdafbeelding {product.name}]
            </div>

            <div className="thumbnail-gallery flex space-x-4">
              {[1, 2, 3, 4].map((thumb) => (
                <div
                  key={thumb}
                  className="thumbnail-item w-20 h-20 bg-gray-100 flex items-center justify-center text-xs text-gray-400 cursor-pointer border hover:border-black transition"
                >
                  [Thumb {thumb}]
                </div>
              ))}
            </div>
          </div>

          {/* Rechterzijde: Product Informatie */}
          <div className="product-info-section w-1/2 flex flex-col justify-start pt-4">
            <h1 className="product-title text-3xl font-bold mb-2">
              {product.name}
            </h1>

            <p className="product-price text-2xl font-bold text-black mb-6">
              €{' '}
              {parseFloat(product.price).toLocaleString('nl-NL', {
                minimumFractionDigits: 2,
                maximumFractionDigits: 2,
              })}
            </p>

            <p className="product-description text-gray-600 text-sm mb-6 leading-relaxed">
              {product.description || 'Geen beschrijving beschikbaar.'}
            </p>

            <div className="product-attributes text-sm mb-6 space-y-1 font-medium">
              <p>
                Afmetingen:{' '}
                <span className="font-normal text-gray-600">
                  {product.dimensions}
                </span>
              </p>

              <p>
                Materiaal:{' '}
                <span className="font-normal text-gray-600">
                  {product.material}
                </span>
              </p>
            </div>

            {/* TODO: Voorraad status zou dynamisch berekend kunnen worden als er een 'inventory' tabel wordt toegevoegd */}
            <div className="stock-status flex items-center text-green-600 text-sm font-semibold mb-8">
              <span className="status-icon mr-2">✔</span> Op voorraad
            </div>

            <div className="product-actions flex items-center space-x-4">
              <button
                onClick={handleAddToCart}
                className="add-to-cart-button bg-orange-500 hover:bg-orange-600 text-white font-bold py-3 px-8 rounded flex-1 transition"
              >
                IN WINKELWAGEN
              </button>

              <button
                onClick={handleToggleFavorite}
                className="favorite-button p-3 border border-gray-300 rounded hover:bg-gray-50 text-gray-500 transition"
                title="Toevoegen aan favorieten"
              >
                ♡
              </button>
            </div>
          </div>
        </section>

        {/* Reviews sectie */}
        <section className="reviews-section mt-10 bg-white p-8 shadow-sm">
          <h2 className="reviews-title text-2xl font-bold mb-6">
            Reviews
          </h2>

          {/* Alleen ingelogde gebruikers mogen een review plaatsen */}
          {isLoggedIn ? (
            <form
              onSubmit={handleSubmitReview}
              className="review-form mb-8 border-b pb-8"
            >
              <h3 className="font-semibold mb-4">
                Schrijf een review
              </h3>

              <div className="mb-4">
                <label className="block text-sm font-medium mb-1">
                  Rating
                </label>

                <select
                  value={rating}
                  onChange={(event) => setRating(event.target.value)}
                  className="border border-gray-300 rounded p-2 bg-white"
                >
                  <option value="5">5 sterren</option>
                  <option value="4">4 sterren</option>
                  <option value="3">3 sterren</option>
                  <option value="2">2 sterren</option>
                  <option value="1">1 ster</option>
                </select>
              </div>

              <div className="mb-4">
                <label className="block text-sm font-medium mb-1">
                  Review
                </label>

                <textarea
                  value={comment}
                  onChange={(event) => setComment(event.target.value)}
                  className="w-full border border-gray-300 rounded p-3 min-h-28"
                  placeholder="Wat vind je van dit product?"
                />
              </div>

              {reviewError && (
                <p className="text-red-500 text-sm mb-3">
                  {reviewError}
                </p>
              )}

              {reviewSuccess && (
                <p className="text-green-600 text-sm mb-3">
                  {reviewSuccess}
                </p>
              )}

              <button
                type="submit"
                disabled={isSubmittingReview}
                className="bg-black text-white px-6 py-2 rounded hover:bg-gray-800 disabled:bg-gray-400"
              >
                {isSubmittingReview ? 'Review plaatsen...' : 'Review plaatsen'}
              </button>
            </form>
          ) : (
            <div className="login-message mb-8 border-b pb-8 text-gray-600">
              <p>
                Wil je een review plaatsen?{' '}
                <Link
                  to="/login"
                  className="text-blue-600 font-semibold hover:underline"
                >
                  Log dan eerst in
                </Link>
                .
              </p>
            </div>
          )}

          {/* Reviews tonen mag voor iedereen */}
          {isLoadingReviews ? (
            <p className="reviews-loading text-gray-500">
              Reviews laden...
            </p>
          ) : reviews.length === 0 ? (
            <p className="no-reviews text-gray-500">
              Er zijn nog geen reviews voor dit product.
            </p>
          ) : (
            <div className="reviews-list space-y-4">
              {reviews.map((review) => (
                <div key={review.id} className="review-card border rounded p-4">
                  <div className="review-header flex items-center justify-between mb-2">
                    <p className="review-rating font-semibold">
                      {'★'.repeat(review.rating)}
                      {'☆'.repeat(5 - review.rating)}
                    </p>

                    <div className="review-meta text-right">
                      {review.customerId && (
                        <p className="text-xs text-gray-400">
                          Klant #{review.customerId}
                        </p>
                      )}

                      {review.reviewDate && (
                        <p className="text-xs text-gray-400">
                          {review.reviewDate}
                        </p>
                      )}
                    </div>
                  </div>

                  <p className="review-comment text-gray-700">
                    {review.comment || 'Geen tekst toegevoegd.'}
                  </p>
                </div>
              ))}
            </div>
          )}
        </section>
              {isFavorite ? '♥' : '♡'}
            </button>
          </div>
        </div>
      </main>
    </div>
  );
}
