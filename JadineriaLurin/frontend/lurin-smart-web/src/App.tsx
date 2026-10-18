import { useState } from 'react';
import { BrowserRouter, Routes, Route, Navigate, Outlet, useLocation } from 'react-router-dom';
import { Toaster } from 'react-hot-toast';
import { CartProvider } from './contexts/CartContext';
import { AuthProvider, useAuth } from './contexts/AuthContext';
import { GuestRoute, ProtectedRoute } from './components/auth/ProtectedRoute';
import { InactivityGuard } from './components/auth/InactivityGuard';
import { Sidebar } from './components/layout/Sidebar';
import { ViveroLogo } from './components/layout/ViveroLogo';
import { rolesForPath } from './constants/menu';
import { PosPage } from './pages/PosPage';
import { LogisticsDashboard } from './pages/LogisticsDashboard';
import { DriverMobileApp } from './pages/DriverMobileApp';
import IoTMonitoringPage from './pages/IoTMonitoringPage';
import IrrigationDashboardPage from './pages/IrrigationDashboardPage';
import InventoryPage from './pages/InventoryPage';
import { LoginPage } from './pages/auth/LoginPage';
import { ForgotPasswordPage } from './pages/auth/ForgotPasswordPage';
import { ResetPasswordPage } from './pages/auth/ResetPasswordPage';
import './index.css';

/** Rutas públicas (sin menú lateral ni sesión). */
const PUBLIC_PATHS = ['/login', '/forgot-password', '/reset-password'];

const ProtectedLayout: React.FC = () => {
  const location = useLocation();
  const { isAuthenticated } = useAuth();
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);

  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location }} />;
  }

  // La app del repartidor es full-screen para móvil.
  const isDriverMobile = location.pathname.startsWith('/logistics/driver');
  const isPublicPage = PUBLIC_PATHS.some((path) => location.pathname.startsWith(path));

  if (isDriverMobile || isPublicPage) {
    return <Outlet />;
  }

  return (
    <div className="min-h-screen bg-slate-50">
      <header className="lg:hidden sticky top-0 z-30 flex items-center gap-3 border-b border-slate-200 bg-white px-4 py-3">
        <button
          type="button"
          onClick={() => setIsMobileMenuOpen(true)}
          aria-label="Abrir menú"
          className="rounded-lg p-2 text-slate-600 hover:bg-slate-100"
        >
          <svg className="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
          </svg>
        </button>
        <div className="flex items-center gap-2">
          <ViveroLogo className="h-7 w-7" />
          <span className="text-sm font-bold text-slate-900">Jardinería Lurín Smart</span>
        </div>
      </header>

      <Sidebar
        isMobileOpen={isMobileMenuOpen}
        onCloseMobile={() => setIsMobileMenuOpen(false)}
      />

      <main className="lg:pl-64 min-h-screen">
        <Outlet />
      </main>
    </div>
  );
};

function App() {
  return (
    <BrowserRouter>
      <CartProvider>
        <AuthProvider>
          <Toaster position="top-center" toastOptions={{ duration: 4000 }} />
          <InactivityGuard />

          <Routes>
            <Route path="/login" element={<GuestRoute><LoginPage /></GuestRoute>} />
            <Route path="/forgot-password" element={<GuestRoute><ForgotPasswordPage /></GuestRoute>} />
            <Route path="/reset-password" element={<GuestRoute><ResetPasswordPage /></GuestRoute>} />

            <Route element={<ProtectedLayout />}>
              <Route path="/" element={<Navigate to="/pos" replace />} />
              <Route
                path="/pos"
                element={
                  <ProtectedRoute roles={rolesForPath('/pos')}>
                    <PosPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/inventory"
                element={
                  <ProtectedRoute roles={rolesForPath('/inventory')}>
                    <InventoryPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/iot"
                element={
                  <ProtectedRoute roles={rolesForPath('/iot')}>
                    <IoTMonitoringPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/irrigation"
                element={
                  <ProtectedRoute roles={rolesForPath('/irrigation')}>
                    <IrrigationDashboardPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/logistics/admin"
                element={
                  <ProtectedRoute roles={rolesForPath('/logistics/admin')}>
                    <LogisticsDashboard />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/logistics/driver"
                element={
                  <ProtectedRoute roles={rolesForPath('/logistics/driver')}>
                    <DriverMobileApp />
                  </ProtectedRoute>
                }
              />
            </Route>

            <Route path="*" element={<Navigate to="/login" replace />} />
          </Routes>
        </AuthProvider>
      </CartProvider>
    </BrowserRouter>
  );
}

export default App;
