import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import ProtectedRoute from './components/ProtectedRoute';
import HomePage from './pages/HomePage';
import ProductsPage from './pages/ProductsPage';
import CategoriesPage from './pages/CategoriesPage';
import SubcategoriesPage from './pages/SubcategoriesPage';
import ProductDetailPage from './pages/ProductDetailPage';
import LoginPage from './pages/LoginPage';
import ProfilePage from './pages/ProfilePage';
import AdminDashboard from './pages/AdminDashboard';
import OrdersPage from './pages/OrdersPage';
import OrderDetailPage from './pages/OrderDetailPage';
import ShoppingCartPage from './pages/ShoppingCartPage';
import CheckOutPage from './pages/CheckOutPage';
import SearchPage from './pages/SearchPage';


function App() {
  return (
    <AuthProvider>
      <Router>
        <Routes>
          {/* Publieke routes */}
          <Route path="/" element={<HomePage />} />
          <Route path="/categorieen" element={<CategoriesPage />} />
          <Route path="/categorieen/:categoryId" element={<SubcategoriesPage />} />
          <Route path="/producten" element={<ProductsPage />} />
          <Route path="/product/:id" element={<ProductDetailPage />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/zoeken" element={<SearchPage />} />


          {/* Routes uitsluitend voor Customers */}
          <Route element={<ProtectedRoute allowedRoles={['Customer']} />}>
            <Route path="/profiel" element={<ProfilePage />} />
            <Route path="/winkelwagen" element={<ShoppingCartPage />} />
            <Route path="/afrekenen" element={<CheckOutPage />} />
            <Route path="/bestellingen" element={<OrdersPage />} />
            <Route path="/bestelling/:id" element={<OrderDetailPage />} />
          </Route>

          {/* Routes uitsluitend voor Admin / Employee */}
          <Route element={<ProtectedRoute allowedRoles={['Admin', 'Employee']} />}>
            <Route path="/admin" element={<AdminDashboard />} />
          </Route>
        </Routes>
      </Router>
    </AuthProvider>
  );
}

export default App;