import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import ProtectedRoute from './components/ProtectedRoute';
import HomePage from './pages/HomePage';
import ProductsPage from './pages/ProductsPage';
import CategoriesPage from './pages/CategoriesPage';
import ProductDetailPage from './pages/ProductDetailPage';
import LoginPage from './pages/LoginPage';
import ProfilePage from './pages/ProfilePage';
import AdminDashboard from './pages/AdminDashboard';
import SearchPage from './pages/SearchPage';

function App() {
  return (
    <AuthProvider>
      <Router>
        <Routes>
          {/* Publieke routes */}
          <Route path="/" element={<HomePage />} />
          <Route path="/categorieen" element={<CategoriesPage />} />
          <Route path="/producten" element={<ProductsPage />} />
          <Route path="/product/:id" element={<ProductDetailPage />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/zoeken" element={<SearchPage />} />


          {/* Routes uitsluitend voor Customers */}
          <Route element={<ProtectedRoute allowedRoles={['Customer']} />}>
            <Route path="/profiel" element={<ProfilePage />} />
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