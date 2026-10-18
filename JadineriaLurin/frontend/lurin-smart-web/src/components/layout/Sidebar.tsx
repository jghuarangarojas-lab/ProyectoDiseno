import React from 'react';
import { NavLink, useNavigate } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import { menuItemsForRole } from '../../constants/menu';
import { ViveroLogo } from './ViveroLogo';

const ROLE_LABELS: Record<string, string> = {
  ADMIN: 'Administrador',
  BOTANIST: 'Botánico',
  GARDENER: 'Jardinero',
  CASHIER: 'Cajero',
  WAREHOUSE: 'Almacenero',
  DELIVERY: 'Repartidor',
};

interface SidebarProps {
  isMobileOpen: boolean;
  onCloseMobile: () => void;
}

const SidebarContent: React.FC<{ onNavigate?: () => void }> = ({ onNavigate }) => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const items = menuItemsForRole(user?.role);

  const handleLogout = () => {
    logout();
    navigate('/login', { replace: true });
  };

  return (
    <div className="flex h-full flex-col">
      <div className="flex items-center gap-3 px-5 py-5 border-b border-slate-200/80">
        <ViveroLogo className="h-9 w-9 shrink-0" />
        <div className="min-w-0">
          <p className="text-sm font-bold text-slate-900 leading-tight truncate">
            Jardinería Lurín
          </p>
          <p className="text-xs text-green-700 font-medium leading-tight">Smart</p>
        </div>
      </div>

      <nav className="flex-1 overflow-y-auto px-3 py-4 space-y-1" aria-label="Menú principal">
        {items.map((item) => (
          <NavLink
            key={item.path}
            to={item.path}
            onClick={onNavigate}
            className={({ isActive }) =>
              `block rounded-lg px-3 py-2.5 text-sm font-medium transition-colors ${
                isActive
                  ? 'bg-green-600 text-white shadow-sm'
                  : 'text-slate-600 hover:bg-slate-100 hover:text-slate-900'
              }`
            }
          >
            {item.label}
          </NavLink>
        ))}

        {items.length === 0 && (
          <p className="px-3 py-2 text-sm text-slate-400">
            Tu rol no tiene módulos asignados. Contacta al administrador.
          </p>
        )}
      </nav>

      {user && (
        <div className="border-t border-slate-200/80 px-4 py-4">
          <div className="flex items-center gap-3">
            <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-green-100 text-sm font-bold text-green-700">
              {user.username.charAt(0).toUpperCase()}
            </div>
            <div className="min-w-0 flex-1">
              <p className="text-sm font-semibold text-slate-900 truncate">{user.username}</p>
              <span className="inline-block rounded bg-slate-100 px-1.5 py-0.5 text-[11px] font-medium text-slate-600">
                {ROLE_LABELS[user.role] ?? user.role}
              </span>
            </div>
          </div>
          <button
            type="button"
            onClick={handleLogout}
            className="mt-3 w-full rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm font-medium text-red-600 transition-colors hover:bg-red-100"
          >
            Cerrar sesión
          </button>
        </div>
      )}
    </div>
  );
};

/** Menú lateral: escritorio fijo + cajón deslizante en móvil. */
export const Sidebar: React.FC<SidebarProps> = ({ isMobileOpen, onCloseMobile }) => {
  return (
    <>
      {/* Escritorio (lg+) */}
      <aside className="hidden lg:flex fixed inset-y-0 left-0 z-40 w-64 flex-col border-r border-slate-200 bg-white">
        <SidebarContent />
      </aside>

      {/* Móvil: overlay + drawer */}
      {isMobileOpen && (
        <>
          <div
            className="fixed inset-0 z-40 bg-slate-900/50 lg:hidden"
            onClick={onCloseMobile}
            aria-hidden="true"
          />
          <aside
            className="fixed inset-y-0 left-0 z-50 w-64 bg-white shadow-xl lg:hidden"
          >
            <SidebarContent onNavigate={onCloseMobile} />
          </aside>
        </>
      )}
    </>
  );
};
