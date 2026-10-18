import React from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import { DEFAULT_PATH_BY_ROLE } from '../../constants/menu';
import type { UserRole } from '../../types/auth';

interface ProtectedRouteProps {
  children: React.ReactNode;
  /** Roles autorizados; si se omite, basta con estar autenticado. */
  roles?: UserRole[];
}

/**
 * Guard de rutas: exige sesión válida y, opcionalmente, pertenencia a uno de
 * los roles indicados. Si no hay sesión, redirige al Login preservando el destino.
 */
export const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ children, roles }) => {
  const { isAuthenticated, user } = useAuth();
  const location = useLocation();

  if (!isAuthenticated || !user) {
    return <Navigate to="/login" replace state={{ from: location }} />;
  }

  if (roles && roles.length > 0 && !roles.includes(user.role)) {
    return <Navigate to={DEFAULT_PATH_BY_ROLE[user.role]} replace />;
  }

  return <>{children}</>;
};

interface GuestRouteProps {
  children: React.ReactNode;
}

/** Solo para visitantes: si ya hay sesión, manda al home de su rol. */
export const GuestRoute: React.FC<GuestRouteProps> = ({ children }) => {
  const { isAuthenticated, user } = useAuth();

  if (isAuthenticated && user) {
    return <Navigate to={DEFAULT_PATH_BY_ROLE[user.role]} replace />;
  }

  return <>{children}</>;
};
