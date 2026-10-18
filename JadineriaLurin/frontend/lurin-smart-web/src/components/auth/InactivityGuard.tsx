import React from 'react';
import { useAuth } from '../../contexts/AuthContext';
import { useInactivityLogout } from '../../hooks/useInactivityLogout';
import { InactivityModal } from './InactivityModal';

/**
 * Vigila la sesión del usuario autenticado: a las 2 horas de inactividad
 * muestra el modal; si no se confirma en 60 segundos, borra el JWT y
 * recarga la página (cierre de sesión por inactividad).
 */
export const InactivityGuard: React.FC = () => {
  const { isAuthenticated, logout } = useAuth();

  const { isWarningOpen, remainingSeconds, confirmPresence } = useInactivityLogout({
    isActive: isAuthenticated,
    onForceLogout: () => {
      logout();
      window.location.reload();
    },
  });

  return (
    <InactivityModal
      isOpen={isWarningOpen}
      remainingSeconds={remainingSeconds}
      onConfirm={confirmPresence}
    />
  );
};
